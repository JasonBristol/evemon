using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EVEMon.Common;
using EVEMon.Common.Controls;
using EVEMon.Common.Data;
using EVEMon.Common.Helpers;
using EVEMon.Common.Models;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.PieChart
{
    public partial class SkillsPieChart : EVEMonForm
    {
        private readonly Character m_character;
        private const int Alpha = 125;


        #region Construction, loading, closing

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="character"></param>
        public SkillsPieChart(Character character)
        {
            InitializeComponent();
            RememberPositionKey = "SkillsPieChart";

            // Fields
            m_character = character;
        }

        /// <summary>
        /// On loading
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode)
                return;

            // Layout
            skillPieChartControl.LeftMargin(20F);
            skillPieChartControl.TopMargin(15F);
            skillPieChartControl.RightMargin(20F);
            skillPieChartControl.BottomMargin(15F);
            skillPieChartControl.FitChart(true);
            skillPieChartControl.SliceRelativeHeight(Settings.UI.SkillPieChart.SliceHeight);
            skillPieChartControl.InitialAngle(Settings.UI.SkillPieChart.InitialAngle);
            skillPieChartControl.StyleOfShadow(ShadowStyle.GradualShadow);
            skillPieChartControl.ColorTypeOfEdge(EdgeColorType.DarkerThanSurface);
            skillPieChartControl.EdgeLineWidth(1F);

            Text = string.Format(CultureInfo.CurrentCulture, "Skillgroup chart for {0}", m_character.Name);

            // Events
            skillPieChartControl.AngleChange += skillPieChartControl_AngleChange;

            // Read settings
            sortBySizeCheck.Checked = Settings.UI.SkillPieChart.SortBySize;
            mergeMinorCheck.Checked = Settings.UI.SkillPieChart.MergeMinorGroups;
            pieHeight.Value = (decimal)Settings.UI.SkillPieChart.SliceHeight;
            pieAngle.Value = (decimal)Settings.UI.SkillPieChart.InitialAngle;

            // Check there are enough colors or create them
            if (Settings.UI.SkillPieChart.Colors.Count < m_character.SkillGroups.Count)
            {
                var newColors = new List<Color>();
                while (newColors.Count < m_character.SkillGroups.Count)
                {
                    newColors.Add(Color.FromArgb(Alpha, Color.Red));
                    newColors.Add(Color.FromArgb(Alpha, Color.Green));
                    newColors.Add(Color.FromArgb(Alpha, Color.Blue));
                }
                skillPieChartControl.Colors = newColors;
            }
            else
                skillPieChartControl.Colors = Settings.UI.SkillPieChart.Colors.Select(color => (Color)color);

            // Initialize plans combox Box                        
            planSelector.SelectedIndex = 0;
            foreach (var plan in m_character.Plans)
            {
                planSelector.Items.Add(plan.Name);
            }

            // Update the display
            UpdatePieChart();
        }

        /// <summary>
        /// On closing, store settings and unsubscribe events
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Store colors to settings
            Settings.UI.SkillPieChart.Colors.Clear();
            foreach (var c in skillPieChartControl.Colors)
            {
                Settings.UI.SkillPieChart.Colors.Add((SerializableColor)c);
            }

            // Store other settings
            Settings.UI.SkillPieChart.SliceHeight = (float)pieHeight.Value;
            Settings.UI.SkillPieChart.InitialAngle = (float)pieAngle.Value;
            Settings.UI.SkillPieChart.MergeMinorGroups = mergeMinorCheck.Checked;
            Settings.UI.SkillPieChart.SortBySize = sortBySizeCheck.Checked;

            // Events
            skillPieChartControl.AngleChange -= skillPieChartControl_AngleChange;
        }

        #endregion


        #region Fetching data to the pie chart

        /// <summary>
        /// Updates the pie chart display
        /// </summary>
        private void UpdatePieChart()
        {
            // Prevents updating before OnLoad() completed.
            if (planSelector.SelectedIndex < 0)
                return;

            var groupCount = StaticSkills.AllGroups.Count();
            var scratchpad = new CharacterScratchpad(m_character);

            // Retrieve the selected Plan
            if (planSelector.SelectedIndex > 0)
            {
                var plan = m_character.Plans[planSelector.SelectedIndex - 1];

                // Updates the scratchpad
                foreach (var entry in plan)
                {
                    scratchpad.Train(entry);
                }
            }


            // Get group to index map and groups list
            var groups = new List<SkillGroup>();
            var indices = new Dictionary<StaticSkillGroup, int>();
            foreach (var group in StaticSkills.AllGroups)
            {
                indices[group] = groups.Count;
                groups.Add(m_character.SkillGroups[group.ID]);
            }

            // Get start SP, before plan
            var srcSkillPoints = new decimal[groupCount];
            foreach (var skillGroup in groups)
            {
                var groupIndex = indices[skillGroup.StaticData];
                srcSkillPoints[groupIndex] = skillGroup.TotalSP;
            }

            // Get target SP and skills count, after plan
            var skillCounts = new int[groupCount];
            var targetSkillPoints = new decimal[groupCount];
            foreach (var skill in StaticSkills.AllSkills)
            {
                var sp = scratchpad.GetSkillPoints(skill);
                var groupIndex = indices[skill.Group];

                targetSkillPoints[groupIndex] += sp;
                if (sp != 0)
                    skillCounts[groupIndex]++;
            }

            // Get groups names and descriptions
            var names = new string[groupCount];
            var descriptions = new string[groupCount];
            for (var i = 0; i < srcSkillPoints.Length; i++)
            {
                names[i] = groups[i].Name;
                descriptions[i] = groups[i].Name;

                var srcSP = srcSkillPoints[i];
                var destSP = targetSkillPoints[i];

                var description = new StringBuilder();
                description.Append($"{names[i]} ({skillCounts[i]} skills, {srcSP:N0} skillpoints");
                if (srcSP != destSP)
                    description.Append($" / {destSP:N0} after plan completion");

                description.Append(")");
                descriptions[i] = description.ToString();
            }

            // Merge minor groups
            if (mergeMinorCheck.Checked)
                Merge(ref targetSkillPoints, ref names, ref descriptions);

            // Compute the slices displacements
            var tinyGroups = 0;
            var slicesDiscplacements = new float[targetSkillPoints.Length];
            for (var i = 0; i < targetSkillPoints.Length; i++)
            {
                slicesDiscplacements[i] = targetSkillPoints[i] < 100000 ? 0.06F + 0.008F * ++tinyGroups : 0.05F;
            }

            // Assign and sort
            skillPieChartControl.Values(targetSkillPoints);
            skillPieChartControl.Texts(names);
            skillPieChartControl.ToolTips(descriptions);
            skillPieChartControl.SliceRelativeDisplacements(slicesDiscplacements);
            skillPieChartControl.OrderSlices(sortBySizeCheck.Checked);
        }

        /// <summary>
        /// Performs the merge
        /// </summary>
        /// <param name="targetSkillPoints"></param>
        /// <param name="names"></param>
        /// <param name="descriptions"></param>
        private static void Merge(ref decimal[] targetSkillPoints, ref string[] names, ref string[] descriptions)
        {
            // Gets total SP and threshold (1% of total SP)
            var totalSP = targetSkillPoints.Sum();
            var threshold = totalSP / 100;

            // Gathers group indices to merge
            var mergedGroupIndices = new List<int>();
            for (var i = 0; i < targetSkillPoints.Length; i++)
            {
                if (targetSkillPoints[i] < threshold)
                    mergedGroupIndices.Add(i);
            }

            // Prepare the merging lists
            var newTargetSkillPoints = new List<decimal>();
            var newDescriptions = new List<string>();
            var newNames = new List<string>();

            if (mergedGroupIndices.Count != 0)
            {
                newTargetSkillPoints.Add(0);
                newDescriptions.Add("");
                newNames.Add("Other");
            }

            // Merge
            var isFirstMerged = true;
            for (var i = 0; i < targetSkillPoints.Length; i++)
            {
                // Is Merged ?
                if (mergedGroupIndices.Contains(i))
                {
                    if (!isFirstMerged)
                        newDescriptions[0] += "\n";
                    isFirstMerged = false;

                    newTargetSkillPoints[0] += targetSkillPoints[i];
                    newDescriptions[0] += descriptions[i];
                }
                    // Not merged
                else
                {
                    newTargetSkillPoints.Add(targetSkillPoints[i]);
                    newDescriptions.Add(descriptions[i]);
                    newNames.Add(names[i]);
                }
            }

            // Replace the old arrays
            targetSkillPoints = newTargetSkillPoints.ToArray();
            descriptions = newDescriptions.ToArray();
            names = newNames.ToArray();
        }

        #endregion


        #region Events for the controls

        /// <summary>
        /// When the user rotates the pie chart, we update the numeric box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void skillPieChartControl_AngleChange(object sender, EventArgs e)
        {
            var angleChangeEventArgs = e as AngleChangeEventArgs;
            if (angleChangeEventArgs != null)
                pieAngle.Value = (decimal)angleChangeEventArgs.NewAngle;
        }

        /// <summary>
        /// When the user changes the numeric box for the pie height, we update the pie chart control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pieHeight_ValueChanged(object sender, EventArgs e)
        {
            skillPieChartControl.SliceRelativeHeight((float)pieHeight.Value);
        }

        /// <summary>
        /// When the user changes the numeric box for the angle, we update the pie chart control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pieAngle_ValueChanged(object sender, EventArgs e)
        {
            skillPieChartControl.InitialAngle((float)pieAngle.Value);
        }

        /// <summary>
        /// Close button click. Close.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// When the user double click the control, we allow him to change the clicked group's color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void skillPieChartControl_DoubleClick(object sender, EventArgs e)
        {
            // Retrieve the clicked segment
            var ev = (MouseEventArgs)e;
            var pieChart3D = skillPieChartControl.PieChart;
            var index = pieChart3D.FindPieSliceUnderPoint(new PointF(ev.X, ev.Y));

            // If none clicked, we return. Otherwise we open the color picker.
            if (index == -1)
                return;

            if (m_colorDialog.ShowDialog() != DialogResult.OK)
                return;

            // The user picked a new color, we update our colors list.
            if (sortBySizeCheck.Checked)
            {
                var realIndex = skillPieChartControl.GetIndex(index);
                var colors = skillPieChartControl.Colors.ToArray();
                colors[realIndex] = Color.FromArgb(Alpha, m_colorDialog.Color);
                skillPieChartControl.Colors = colors;
            }
            else
            {
                var colors = skillPieChartControl.Colors.ToArray();
                colors[index] = Color.FromArgb(Alpha, m_colorDialog.Color);
                skillPieChartControl.Colors = colors;
            }

            // Forces an update of the control
            skillPieChartControl.OrderSlices(sortBySizeCheck.Checked);
        }

        /// <summary>
        /// Save button. Export to PNG.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveButton_Click(object sender, EventArgs e)
        {
            using (var pie = new Bitmap(skillPieChartControl.Width, skillPieChartControl.Height))
            {
                var bounds = new Rectangle(0, 0, skillPieChartControl.Width, skillPieChartControl.Height);
                skillPieChartControl.DrawToBitmap(pie, bounds);

                var savePieResult = savePieDialog.ShowDialog();
                if (savePieResult == DialogResult.OK)
                    pie.Save(savePieDialog.FileName, ImageFormat.Png);
            }
        }

        /// <summary>
        /// Forces an update when the sort by size combo box state changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sortBySizeCheck_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePieChart();
        }

        /// <summary>
        /// When the selected plan change, we refresh all the data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void planSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePieChart();
        }

        /// <summary>
        /// When the merge minor option changes, we refresh all the data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mergeMinorCheck_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePieChart();
        }

        #endregion
    }
}