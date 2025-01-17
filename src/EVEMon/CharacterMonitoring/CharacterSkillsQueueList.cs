﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EVEMon.Common;
using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;
using EVEMon.Common.Factories;
using EVEMon.Common.Helpers;
using EVEMon.Common.Interfaces;
using EVEMon.Common.Models;
using EVEMon.SkillPlanner;

namespace EVEMon.CharacterMonitoring
{
    internal sealed partial class CharacterSkillsQueueList : UserControl
    {
        #region Fields

        // Skills drawing - Region & text padding
        private const int PadTop = 2;
        private const int PadLeft = 6;
        private const int PadRight = 7;

        // Skills drawing - Boxes
        private const int BoxWidth = 57;
        private const int BoxHeight = 14;
        private const int LowerBoxHeight = 8;
        private const int BoxHPad = 6;
        private const int BoxVPad = 2;
        private const int MinimumHeight = 39;

        // Skills drawing - Font & brushes
        private readonly Font m_boldSkillsQueueFont;
        private readonly Font m_skillsQueueFont;

        private object m_lastTooltipItem;
        private BlinkAction m_blinkAction;
        private QueuedSkill m_selectedSkill;

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public CharacterSkillsQueueList()
        {
            InitializeComponent();

            lbSkillsQueue.Hide();

            m_skillsQueueFont = FontFactory.GetFont("Tahoma", 8.25F);
            m_boldSkillsQueueFont = FontFactory.GetFont("Tahoma", 8.25F, FontStyle.Bold);
            noSkillsQueueLabel.Font = FontFactory.GetFont("Tahoma", 11.25F, FontStyle.Bold);
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the character associated with this monitor.
        /// </summary>
        internal CCPCharacter Character { get; set; }

        /// <summary>
        /// Gets the item's height.
        /// </summary>
        private int GetItemHeight => Math.Max(m_skillsQueueFont.Height * 2 + PadTop * 2 + LowerBoxHeight, MinimumHeight);

        #endregion


        #region Inherited events

        /// <summary>
        /// On load subscribe the events.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode || this.IsDesignModeHosted())
                return;

            EveMonClient.CharacterSkillQueueUpdated += EveMonClient_CharacterSkillQueueUpdated;
            EveMonClient.QueuedSkillsCompleted += EveMonClient_QueuedSkillsCompleted;
            EveMonClient.SettingsChanged += EveMonClient_SettingsChanged;
            EveMonClient.TimerTick += EveMonClient_TimerTick;
            Disposed += OnDisposed;
        }

        /// <summary>
        /// Unsubscribe events on disposing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisposed(object sender, EventArgs e)
        {
            EveMonClient.CharacterSkillQueueUpdated -= EveMonClient_CharacterSkillQueueUpdated;
            EveMonClient.QueuedSkillsCompleted -= EveMonClient_QueuedSkillsCompleted;
            EveMonClient.SettingsChanged -= EveMonClient_SettingsChanged;
            EveMonClient.TimerTick -= EveMonClient_TimerTick;
            Disposed -= OnDisposed;
        }

        /// <summary>
        /// When the control becomes visible again, we update the content.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
                UpdateContent();
        }

        #endregion


        #region Display update

        /// <summary>
        /// Updates all the content.
        /// </summary>
        private void UpdateContent()
        {
            // Returns if not visible
            if (!Visible)
                return;

            // When no character, we just hide the list
            if (Character == null)
            {
                noSkillsQueueLabel.Visible = true;
                lbSkillsQueue.Visible = false;
                return;
            }

            var scrollBarPosition = lbSkillsQueue.TopIndex;

            // Update the skills queue list
            lbSkillsQueue.BeginUpdate();
            try
            {
                // Add items in the list
                lbSkillsQueue.Items.Clear();
                foreach (var skill in Character.SkillQueue)
                {
                    lbSkillsQueue.Items.Add(skill);
                }

                // Display or hide the "no queue skills" label.
                noSkillsQueueLabel.Visible = !Character.SkillQueue.Any();
                lbSkillsQueue.Visible = !noSkillsQueueLabel.Visible;

                // Invalidate display
                lbSkillsQueue.Invalidate();
            }
            finally
            {
                lbSkillsQueue.EndUpdate();
                lbSkillsQueue.TopIndex = scrollBarPosition;
            }
        }

        #endregion


        #region Drawing

        /// <summary>
        /// Handles the DrawItem event of the lbSkillsQueue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DrawItemEventArgs"/> instance containing the event data.</param>
        private void lbSkillsQueue_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lbSkillsQueue.Items.Count)
                return;

            var item = lbSkillsQueue.Items[e.Index] as QueuedSkill;

            if (item == null)
                return;

            DrawItem(item, e);
        }

        /// <summary>
        /// Handles the MeasureItem event of the lbSkillsQueue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MeasureItemEventArgs"/> instance containing the event data.</param>
        private void lbSkillsQueue_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            e.ItemHeight = GetItemHeight;
        }

        /// <summary>
        /// Draws the list item for the given skill
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="e"></param>
        private void DrawItem(QueuedSkill skill, DrawItemEventArgs e)
        {
            var g = e.Graphics;

            // Draw background
            g.FillRectangle(e.Index % 2 == 0 ? Brushes.LightGray : Brushes.White, e.Bounds);
            
            // Measure texts
            const TextFormatFlags Format = TextFormatFlags.NoPadding | TextFormatFlags.NoClipping;
            var hasSkill = (skill.Skill != null) && (skill.Skill != Skill.UnknownSkill);

            var skillPoints = (skill.Skill != null) && (skill.Level > skill.Skill.Level + 1) ?
                skill.CurrentSP : (!hasSkill ? skill.StartSP : skill.Skill.SkillPoints);
            var skillPointsToNextLevel = !hasSkill ? skill.EndSP :
                skill.Skill.StaticData.GetPointsRequiredForLevel(Math.Min(skill.Level, 5));
            var pointsLeft = skillPointsToNextLevel - skillPoints;
            var timeSpanFromPoints = !hasSkill ? skill.EndTime.Subtract(DateTime.UtcNow) :
                skill.Skill.GetTimeSpanForPoints(pointsLeft);
            var remainingTimeText = timeSpanFromPoints.ToDescriptiveText(
                DescriptiveTextOptions.SpaceBetween);

            var fractionCompleted = e.Index == 0 ? skill.FractionCompleted : 0.0;

            var indexText = $"{e.Index + 1}. ";
            var rankText = $" (Rank {(skill.Skill == null ? 0 : skill.Rank)})";
            var spPerHourText = $" SP/Hour: {skill.SkillPointsPerHour}";
            var spText = $"SP: {skillPoints:N0}/{skillPointsToNextLevel:N0}";
            var trainingTimeText = $" Training Time: {remainingTimeText}";
            var levelText = $"Level {skill.Level}";
            var percentText = $"{Math.Floor(fractionCompleted * 100) / 100:P0} Done";

            var indexTextSize = TextRenderer.MeasureText(g, indexText, m_boldSkillsQueueFont, Size.Empty, Format);
            var skillNameSize = TextRenderer.MeasureText(g, skill.SkillName, m_boldSkillsQueueFont, Size.Empty, Format);
            var rankTextSize = TextRenderer.MeasureText(g, rankText, m_skillsQueueFont, Size.Empty, Format);
            var levelTextSize = TextRenderer.MeasureText(g, levelText, m_skillsQueueFont, Size.Empty, Format);
            var spPerHourTextSize = TextRenderer.MeasureText(g, spPerHourText, m_skillsQueueFont, Size.Empty, Format);
            var spTextSize = TextRenderer.MeasureText(g, spText, m_skillsQueueFont, Size.Empty, Format);
            var ttTextSize = TextRenderer.MeasureText(g, trainingTimeText, m_skillsQueueFont, Size.Empty, Format);
            var pctTextSize = TextRenderer.MeasureText(g, percentText, m_skillsQueueFont, Size.Empty, Format);

            // Draw texts
            var highlightColor = Color.Black;

            // First line
            var left = e.Bounds.Left + PadLeft;
            var top = e.Bounds.Top + PadTop;
            TextRenderer.DrawText(g, indexText, m_boldSkillsQueueFont,
                new Rectangle(left, top, indexTextSize.Width + PadLeft, indexTextSize.Height), highlightColor);
            left += indexTextSize.Width;
            TextRenderer.DrawText(g, skill.SkillName, m_boldSkillsQueueFont,
                new Rectangle(left, top, skillNameSize.Width + PadLeft, skillNameSize.Height), highlightColor);
            left += skillNameSize.Width;
            TextRenderer.DrawText(g, rankText, m_skillsQueueFont,
                new Rectangle(left, top, rankTextSize.Width + PadLeft, rankTextSize.Height), highlightColor);

            // Second line
            left = e.Bounds.Left + PadLeft + indexTextSize.Width;
            top += skillNameSize.Height;
            TextRenderer.DrawText(g, spText, m_skillsQueueFont,
                new Rectangle(left, top, spTextSize.Width + PadLeft, spTextSize.Height), highlightColor);
            left += spTextSize.Width + PadLeft;
            TextRenderer.DrawText(g, spPerHourText, m_skillsQueueFont,
                new Rectangle(left, top, spPerHourTextSize.Width + PadLeft, spPerHourTextSize.Height), highlightColor);
            left += spPerHourTextSize.Width + PadLeft;
            TextRenderer.DrawText(g, trainingTimeText, m_skillsQueueFont,
                new Rectangle(left, top, ttTextSize.Width + PadLeft, ttTextSize.Height), highlightColor);

            // Boxes
            DrawBoxes(fractionCompleted, skill, e);

            // Draw progression bar
            DrawProgressionBar(fractionCompleted, e);

            // Draw level and percent texts
            TextRenderer.DrawText(g, levelText, m_skillsQueueFont,
                new Rectangle(e.Bounds.Right - BoxWidth - PadRight - BoxHPad - levelTextSize.Width,
                    e.Bounds.Top + PadTop, levelTextSize.Width + PadRight, levelTextSize.Height), highlightColor);
            TextRenderer.DrawText(g, percentText, m_skillsQueueFont,
                new Rectangle(e.Bounds.Right - BoxWidth - PadRight - BoxHPad - pctTextSize.Width,
                    e.Bounds.Top + PadTop + levelTextSize.Height, pctTextSize.Width + PadRight, pctTextSize.Height), highlightColor);

            // Draw the queue color bar
            DrawQueueColorBar(skill, e);
        }

        /// <summary>
        /// Draws the progression bar.
        /// </summary>
        /// <param name="fractionCompleted">The fraction completed.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DrawItemEventArgs"/> instance containing the event data.</param>
        private static void DrawProgressionBar(double fractionCompleted, DrawItemEventArgs e)
        {
            var g = e.Graphics;

            g.DrawRectangle(Pens.Black,
                new Rectangle(e.Bounds.Right - BoxWidth - PadRight,
                    e.Bounds.Top + PadTop + BoxHeight + BoxVPad, BoxWidth, LowerBoxHeight));

            var pctBarRect = new Rectangle(e.Bounds.Right - BoxWidth - PadRight + 2,
                e.Bounds.Top + PadTop + BoxHeight + BoxVPad + 2,
                BoxWidth - 3, LowerBoxHeight - 3);

            g.FillRectangle(Brushes.DarkGray, pctBarRect);
            var fillWidth = (int)(pctBarRect.Width * fractionCompleted);
            if (fillWidth <= 0)
                return;

            var fillRect = new Rectangle(pctBarRect.X, pctBarRect.Y, fillWidth, pctBarRect.Height);
            g.FillRectangle(Brushes.Black, fillRect);
        }

        /// <summary>
        /// Draws the boxes.
        /// </summary>
        /// <param name="fractionCompleted">The fraction completed.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DrawItemEventArgs"/> instance containing the event data.</param>
        private void DrawBoxes(double fractionCompleted, QueuedSkill skill, DrawItemEventArgs e)
        {
            var g = e.Graphics;

            g.DrawRectangle(Pens.Black,
                new Rectangle(e.Bounds.Right - BoxWidth - PadRight, e.Bounds.Top + PadTop, BoxWidth,
                    BoxHeight));

            const int LevelBoxWidth = (BoxWidth - 4 - 3) / 5;
            for (var level = 1; level <= 5; level++)
            {
                var brect =
                    new Rectangle(e.Bounds.Right - BoxWidth - PadRight + 2 + LevelBoxWidth * (level - 1) + (level - 1),
                        e.Bounds.Top + PadTop + 2, LevelBoxWidth, BoxHeight - 3);

                // Box color
                g.FillRectangle(skill.Skill != null && level < skill.Level ? Brushes.Black : Brushes.DarkGray, brect);

                if (skill.Skill == null)
                    continue;

                foreach (var qskill in Character.SkillQueue)
                {
                    if ((!qskill.IsTraining && skill == qskill && level == qskill.Level)
                        || (skill == qskill && level <= qskill.Level && level > skill.Skill.Level
                            && Math.Abs(fractionCompleted) < double.Epsilon))
                    {
                        g.FillRectangle(Brushes.RoyalBlue, brect);
                    }

                    // Blinking indicator of skill level in training
                    if (!qskill.IsTraining || skill != qskill || level != skill.Level ||
                        Math.Abs(fractionCompleted) < double.Epsilon)
                    {
                        continue;
                    }

                    if (m_blinkAction == BlinkAction.Blink)
                        g.FillRectangle(Brushes.RoyalBlue, brect);

                    m_blinkAction = m_blinkAction == BlinkAction.Reset
                        ? BlinkAction.Blink
                        : BlinkAction.Stop;
                }
            }
        }

        /// <summary>
        /// Draws the queue color bar.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DrawItemEventArgs"/> instance containing the event data.</param>
        private void DrawQueueColorBar(QueuedSkill skill, DrawItemEventArgs e)
        {
            var g = e.Graphics;

            // Draw the background
            var qBarRect = new Rectangle(e.Bounds.Left, GetItemHeight - LowerBoxHeight, e.Bounds.Width, LowerBoxHeight);
            g.FillRectangle(Brushes.DimGray, qBarRect);

            var oneDaySkillQueueWidth = Character.SkillQueue.GetOneDaySkillQueueWidth(qBarRect.Width);
            IList<RectangleF> skillRects = Character.SkillQueue.GetSkillRects(skill, qBarRect.Width, LowerBoxHeight - 1).ToList();
            var lessThanDayBrush = Settings.UI.SafeForWork ? Brushes.LightGray : Brushes.Khaki;
            var moreThanDayBrush = Settings.UI.SafeForWork ? Brushes.DarkGray : Brushes.CornflowerBlue;
            var skillRectFirst = skillRects.First();

            // Skill starts before the 24h marker
            if (skillRectFirst.X < oneDaySkillQueueWidth)
            {
                // Iterate only through rectangles with width
                foreach (var skillRect in skillRects.Skip(1).Where(rect => rect.Width > 0))
                {
                    var brush = oneDaySkillQueueWidth - skillRect.X <= 0 ? moreThanDayBrush : lessThanDayBrush;

                    g.FillRectangle(brush,
                        new RectangleF(skillRect.X, GetItemHeight - LowerBoxHeight, skillRect.Width, skillRect.Height));
                }
                return;
            }

            g.FillRectangle(moreThanDayBrush,
                new RectangleF(skillRectFirst.X, GetItemHeight - LowerBoxHeight, skillRectFirst.Width, skillRectFirst.Height));
        }

        #endregion


        #region Local events

        /// <summary>
        /// Handles the MouseWheel event of the lbSkillsQueue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void lbSkillsQueue_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!lbSkillsQueue.VerticalScrollBarVisible())
                return;
            if (e.Delta == 0)
                return;

            // Update the drawing based upon the mouse wheel scrolling.
            var numberOfItemLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines /
                                          Math.Abs(e.Delta);
            var lines = numberOfItemLinesToMove;
            if (lines == 0)
                return;

            // Quit if at the top and requesting an up movement
            if (lines > 0 && lbSkillsQueue.GetVerticalScrollBarPosition() == 0)
                return;

            // Compute the number of lines to move
            var direction = lines / Math.Abs(lines);
            var numberOfPixelsToMove = new int[lines * direction];
            for (var i = 1; i <= Math.Abs(lines); i++)
            {
                object item = null;
                var queue = lbSkillsQueue.Items;

                // Going up
                if (direction == Math.Abs(direction))
                {
                    // Retrieve the next top item
                    if (lbSkillsQueue.TopIndex - i >= 0)
                        item = queue[lbSkillsQueue.TopIndex - i];
                }
                    // Going down
                else
                {
                    // Compute the height of the items from current the topindex (included)
                    var height = 0;
                    for (var j = lbSkillsQueue.TopIndex + i - 1; j < queue.Count; j++)
                        height += GetItemHeight;

                    // Retrieve the next bottom item
                    if (height > lbSkillsQueue.ClientSize.Height)
                        item = queue[lbSkillsQueue.TopIndex + i - 1];
                }

                // If found a new item as top or bottom
                if (item != null)
                    numberOfPixelsToMove[i - 1] = GetItemHeight * direction;
                else
                    lines -= direction;
            }

            // Scroll 
            if (lines != 0)
                lbSkillsQueue.Invalidate();
        }

        /// <summary>
        /// Handles the MouseDown event of the lbSkills control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void lbSkillsQueue_MouseDown(object sender, MouseEventArgs e)
        {
            // Retrieve the item at the given point and quit if none
            var index = lbSkillsQueue.IndexFromPoint(e.Location);
            if (index < 0 || index >= lbSkillsQueue.Items.Count)
                return;

            var item = lbSkillsQueue.Items[index] as QueuedSkill;

            // Beware, this last index may actually means a click in the whitespace at the bottom
            // Let's deal with this special case
            if (index == lbSkillsQueue.Items.Count - 1)
            {
                var itemRect = lbSkillsQueue.GetItemRectangle(index);
                if (!itemRect.Contains(e.Location))
                    item = null;
            }

            if (e.Button != MouseButtons.Right)
                return;

            // Right click for skills below lv5 : we display a context menu to plan higher levels
            lbSkillsQueue.Cursor = Cursors.Default;

            // Set the selected item
            m_selectedSkill = item;

            // Display the context menu
            contextMenuStrip.Show(lbSkillsQueue, e.Location);
        }

        /// <summary>
        /// On mouse move, we hide the tooltip.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbSkillsQueue_MouseMove(object sender, MouseEventArgs e)
        {
            lbSkillsQueue.Cursor = CustomCursors.ContextMenu;

            for (var i = 0; i < lbSkillsQueue.Items.Count; i++)
            {
                // Skip until we found the mouse location
                var rect = lbSkillsQueue.GetItemRectangle(i);
                if (!rect.Contains(e.Location))
                    continue;

                var item = lbSkillsQueue.Items[i] as QueuedSkill;

                // Updates the tooltip
                DisplayTooltip(item);
                return;
            }

            // If we went so far, we're not over anything
            m_lastTooltipItem = null;
            ttToolTip.Active = false;
        }

        /// <summary>
        /// Handles the Opening event of the contextMenuStrip control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            e.Cancel = !Character.SkillQueue.Any();
            if (!e.Cancel)
            {
                BuildContextMenu(m_selectedSkill);
                showInSkillBrowserMenuItem.Visible = showInSkillExplorerMenuItem.Visible =
                    showInMenuSeparator.Visible = tsmiAddSkill.Visible =
                    addSkillSeparator.Visible = (m_selectedSkill != null);
            }
        }

        /// <summary>
        /// Builds the context menu for the selected queued skill.
        /// </summary>
        /// <param name="queuedSkill">The queued skill.</param>
        private void BuildContextMenu(QueuedSkill queuedSkill)
        {
            if (m_selectedSkill == null)
                return;

            tsmiAddSkill.DropDownItems.Clear();

            // Reset the menu
            tsmiAddSkill.Text = $"Add {queuedSkill.Skill.Name}";

            // Build the level options
            for (long level = queuedSkill.Level; level <= 5; level++)
            {
                ToolStripMenuItem tempMenuLevel = null;
                try
                {
                    tempMenuLevel = new ToolStripMenuItem($"Level {Skill.GetRomanFromInt(level)} to");

                    Character.Plans.AddTo(tempMenuLevel.DropDownItems,
                        (menuPlanItem, plan) =>
                        {
                            menuPlanItem.Click += menuPlanItem_Click;
                            menuPlanItem.Tag = new KeyValuePair<Plan, SkillLevel>(plan,
                                new SkillLevel(queuedSkill.Skill, level));
                        });

                    var menuLevel = tempMenuLevel;
                    tempMenuLevel = null;

                    tsmiAddSkill.DropDownItems.Add(menuLevel);
                }
                finally
                {
                    tempMenuLevel?.Dispose();
                }
            }
        }

        /// <summary>
        /// Displays the tooltip for the given skill.
        /// </summary>
        /// <param name="skill"></param>
        private void DisplayTooltip(QueuedSkill skill)
        {
            if (skill == null)
                return;

            if (ttToolTip.Active && m_lastTooltipItem != null && m_lastTooltipItem == skill)
                return;

            m_lastTooltipItem = skill;

            ttToolTip.Active = false;
            ttToolTip.SetToolTip(lbSkillsQueue, GetTooltip(skill));
            ttToolTip.Active = true;
        }

        /// <summary>
        /// Gets the tooltip text for the given skill
        /// </summary>
        /// <param name="skill"></param>
        private static string GetTooltip(QueuedSkill skill)
        {
            if (skill.Skill == null)
                return string.Empty;

            var sp = skill.Level > skill.Skill.Level + 1 ? skill.CurrentSP : skill.Skill.SkillPoints;
            var nextLevel = Math.Min(5, skill.Level);
            double fractionCompleted = skill.FractionCompleted;
            var nextLevelSP = skill.Skill == Skill.UnknownSkill
                ? skill.EndSP
                : skill.Skill.StaticData.GetPointsRequiredForLevel(nextLevel);
            var pointsLeft = nextLevelSP - sp;
            var timeSpanFromPoints = skill.Skill == Skill.UnknownSkill
                ? skill.EndTime.Subtract(DateTime.UtcNow)
                : skill.Skill.GetTimeSpanForPoints(pointsLeft);
            var remainingTimeText = timeSpanFromPoints.ToDescriptiveText(
                DescriptiveTextOptions.IncludeCommas | DescriptiveTextOptions.UppercaseText);

            if (sp < skill.Skill.StaticData.GetPointsRequiredForLevel(1))
            {
                // Training hasn't got past level 1 yet
                var untrainedToolTip = new StringBuilder();
                untrainedToolTip.Append("Not yet trained to Level I (");
                untrainedToolTip.AppendFormat("{0:P0}", Math.Floor(fractionCompleted * 100.0) *
                    0.01).AppendLine(")");
                untrainedToolTip.Append("Next level I: ").AppendFormat("{0:N0}", pointsLeft);
                untrainedToolTip.AppendLine(" skill points remaining");
                untrainedToolTip.Append("Training time remaining: ");
                untrainedToolTip.AppendLine(remainingTimeText);

                AddSkillBoilerPlate(untrainedToolTip, skill);

                return untrainedToolTip.ToString();
            }

            // So, it's a left click on a skill, we display the tool tip
            // Currently training skill?
            if (skill.IsTraining && fractionCompleted > 0)
            {
                var partiallyTrainedToolTip = new StringBuilder();
                partiallyTrainedToolTip.Append("Partially Completed (");
                partiallyTrainedToolTip.AppendFormat("{0:P0}", Math.Floor(fractionCompleted *
                    100.0) * 0.01).AppendLine(")");
                partiallyTrainedToolTip.Append("Training to level ");
                partiallyTrainedToolTip.Append(Skill.GetRomanFromInt(nextLevel));
                partiallyTrainedToolTip.Append(": ").AppendFormat("{0:N0}", pointsLeft);
                partiallyTrainedToolTip.AppendLine(" skill points remaining");
                partiallyTrainedToolTip.Append("Training time remaining: ");
                partiallyTrainedToolTip.AppendLine(remainingTimeText);

                AddSkillBoilerPlate(partiallyTrainedToolTip, skill);

                return partiallyTrainedToolTip.ToString();
            }

            // Currently training skill but next queued level?
            if (skill.IsTraining && Math.Abs(fractionCompleted) < double.Epsilon)
            {
                var partiallyTrainedToolTip = new StringBuilder();
                partiallyTrainedToolTip.Append("Previous level not yet completed");
                partiallyTrainedToolTip.AppendLine();
                partiallyTrainedToolTip.Append("Queued to level ");
                partiallyTrainedToolTip.Append(Skill.GetRomanFromInt(nextLevel));
                partiallyTrainedToolTip.Append(": ").AppendFormat("{0:N0}", pointsLeft);
                partiallyTrainedToolTip.AppendLine(" skill points remaining");
                partiallyTrainedToolTip.Append("Training time to next level: ");
                partiallyTrainedToolTip.AppendLine(remainingTimeText);

                AddSkillBoilerPlate(partiallyTrainedToolTip, skill);

                return partiallyTrainedToolTip.ToString();
            }

            // Partially trained skill and not in training?
            if (skill.Skill.IsPartiallyTrained && !skill.IsTraining)
            {
                var partiallyTrainedToolTip = new StringBuilder();
                partiallyTrainedToolTip.Append("Partially Completed (");
                partiallyTrainedToolTip.AppendFormat("{0:P0}", Math.Floor(fractionCompleted *
                    100.0) * 0.01).AppendLine(")");
                partiallyTrainedToolTip.Append("Queued to level ");
                partiallyTrainedToolTip.Append(Skill.GetRomanFromInt(nextLevel));
                partiallyTrainedToolTip.Append(": ").AppendFormat("{0:N0}", pointsLeft);
                partiallyTrainedToolTip.AppendLine(" skill points remaining");
                partiallyTrainedToolTip.Append("Training time remaining: ");
                partiallyTrainedToolTip.AppendLine(remainingTimeText);

                AddSkillBoilerPlate(partiallyTrainedToolTip, skill);

                return partiallyTrainedToolTip.ToString();
            }

            // We've completed all the skill points for the current level
            if (!skill.Skill.IsPartiallyTrained && skill.Level <= 5)
            {
                var levelCompleteToolTip = new StringBuilder();
                levelCompleteToolTip.Append("Completed Level ");
                levelCompleteToolTip.Append(Skill.GetRomanFromInt(skill.Level - 1));
                levelCompleteToolTip.Append(": ").AppendFormat("{0:N0}", sp);
                levelCompleteToolTip.Append("/").AppendFormat("{0:N0}", nextLevelSP);
                levelCompleteToolTip.AppendLine();
                levelCompleteToolTip.Append("Queued level ");
                levelCompleteToolTip.Append(Skill.GetRomanFromInt(nextLevel));
                levelCompleteToolTip.Append(": ").AppendFormat("{0:N0}", pointsLeft);
                levelCompleteToolTip.AppendLine(" skill points required");
                levelCompleteToolTip.Append("Training time to next level: ");
                levelCompleteToolTip.AppendLine(remainingTimeText);

                AddSkillBoilerPlate(levelCompleteToolTip, skill);

                return levelCompleteToolTip.ToString();
            }

            // Error in calculating SkillPoints
            var calcError = new StringBuilder();
            calcError.AppendLine("Partially Trained (Could not calculate all skill details)");
            calcError.Append("Next level ").AppendFormat("{0:N0}", nextLevel);
            calcError.Append(": ").AppendFormat("{0:N0}", pointsLeft);
            calcError.AppendLine(" skill points remaining");
            calcError.Append("Training time remaining: ");
            calcError.AppendLine(remainingTimeText);

            AddSkillBoilerPlate(calcError, skill);

            return calcError.ToString();
        }

        /// <summary>
        /// Adds the skill boiler plate.
        /// </summary>
        /// <param name="toolTip">The tool tip.</param>
        /// <param name="skill">The skill.</param>
        private static void AddSkillBoilerPlate(StringBuilder toolTip, QueuedSkill skill)
        {
            toolTip.AppendLine().AppendLine(skill.Skill.Description.WordWrap(100));
            toolTip.Append("Primary: ").Append(skill.Skill.PrimaryAttribute).Append(", ");
            toolTip.Append("Secondary: ").Append(skill.Skill.SecondaryAttribute).Append(" (");
            toolTip.AppendFormat("{0:F0}", skill.SkillPointsPerHour).Append(" SP/Hour)");
        }

        /// <summary>
        /// Handler for a context menu item click on a skill.
        /// Add the entire skill queue to a plan.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void tsmiCreatePlanFromSkillQueue_Click(object sender, EventArgs e)
        {
            if (Character == null)
                return;

            // Create new plan
            var newPlan = PlanWindow.CreateNewPlan(Character, EveMonConstants.CurrentSkillQueueText);

            if (newPlan == null)
                return;

            // Add skill queue to new plan and insert it on top of the plans
            var planCreated = PlanIOHelper.CreatePlanFromCharacterSkillQueue(newPlan, Character);

            // Show the editor for this plan
            if (planCreated)
                PlanWindow.ShowPlanWindow(plan: newPlan);
        }

        /// <summary>
        /// Handler for a context menu item click on a skill.
        /// Add a skill to the plan.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void menuPlanItem_Click(object sender, EventArgs e)
        {
            var planItem = (ToolStripMenuItem)sender;
            var tag = (KeyValuePair<Plan, SkillLevel>)planItem.Tag;

            var operation = tag.Key.TryPlanTo(tag.Value.Skill, tag.Value.Level);
            // If this operation does not change the plan, do nothing
            if (operation == null || operation.Type == PlanOperations.None)
                return;

            var planWindow = PlanWindow.ShowPlanWindow(plan: operation.Plan);
            if (planWindow == null)
                return;

            PlanHelper.SelectPerform(new PlanToOperationWindow(operation), planWindow, operation);
        }

        /// <summary>
        /// Shows the selected skill in Skill Browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showInSkillBrowserMenuItem_Click(object sender, EventArgs e)
        {
            // Open the skill browser
            PlanWindow.ShowPlanWindow(Character).ShowSkillInBrowser(m_selectedSkill?.Skill);
        }

        /// <summary>
        /// Shows the selected skill in Skill Explorer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showInSkillExplorerMenuItem_Click(object sender, EventArgs e)
        {
            // Open the skill explorer
            SkillExplorerWindow.ShowSkillExplorerWindow(Character).ShowSkillInExplorer(m_selectedSkill?.Skill);
        }

        #endregion


        #region Global events

        /// <summary>
        /// Handles the TimerTick event of the EveMonClient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            if (!Visible || Character == null || !Character.IsTraining)
                return;

            if (m_blinkAction == BlinkAction.Stop)
                m_blinkAction = BlinkAction.Reset;
        }

        /// <summary>
        /// When the skill queue changed, we refresh the content.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_CharacterSkillQueueUpdated(object sender, CharacterChangedEventArgs e)
        {
            if (e.Character != Character)
                return;

            UpdateContent();
        }

        /// <summary>
        /// When the queue changed, we refresh the content.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_QueuedSkillsCompleted(object sender, QueuedSkillsEventArgs e)
        {
            if (e.Character != Character)
                return;

            UpdateContent();
        }

        /// <summary>
        /// When the settings changed, we refresh the content.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_SettingsChanged(object sender, EventArgs e)
        {
            UpdateContent();
        }

        #endregion
    }
}
