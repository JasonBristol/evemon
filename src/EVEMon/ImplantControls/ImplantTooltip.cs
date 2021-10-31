﻿using System;
using System.Drawing;
using System.Windows.Forms;
using EVEMon.Common.Data;
using EVEMon.Common.Factories;

namespace EVEMon.ImplantControls
{
    /// <summary>
    /// A tooltip used to display the details of an implant.
    /// </summary>
    public partial class ImplantTooltip : Form
    {
        private const TextFormatFlags TooltipFlags =
            TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.EndEllipsis;

        private Implant m_implant;
        private readonly Font m_toolTipFont = FontFactory.GetFont("Tahoma", 8.25f);
        private readonly Font m_titleFont = FontFactory.GetFont("Tahoma", 9.75f, FontStyle.Bold);
        private const int ToolTipMargin = 5;
        private const int InnerMargin = 10;
        private const int MaxWidth = 250;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ImplantTooltip()
        {
            InitializeComponent();
            ShowInTaskbar = false;
            TopMost = true;
        }

        /// <summary>
        /// Gets or sets the represented implant.
        /// </summary>
        public Implant Implant
        {
            get { return m_implant; }
            set
            {
                if (m_implant == value)
                    return;
                m_implant = value;
                OnImplantChanged();
                Invalidate();
            }
        }

        /// <summary>
        /// When an implant changes, we computes the required size.
        /// </summary>
        private void OnImplantChanged()
        {
            var proposedSize = new Size(MaxWidth, 1000);
            var titleSize = TextRenderer.MeasureText(m_implant.Name, m_titleFont, proposedSize);

            proposedSize = new Size(Math.Max(titleSize.Width, MaxWidth) + ToolTipMargin * 2, 1000);
            var size = TextRenderer.MeasureText(m_implant.Description, m_toolTipFont, proposedSize, TooltipFlags);
            size.Height += titleSize.Height + InnerMargin + ToolTipMargin * 2;
            size.Width = Math.Max(size.Width, proposedSize.Width);

            Size = size;
        }

        /// <summary>
        /// Performs the painting.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Background
            using (var brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(brush, DisplayRectangle);
            }

            // Border
            using (var pen = new Pen(SystemBrushes.WindowFrame))
            {
                var borderRect = new Rectangle(0, 0, DisplayRectangle.Width - 1, DisplayRectangle.Height - 1);
                e.Graphics.DrawRectangle(pen, borderRect);
            }

            // Title
            var titleSize = new Size(Width - ToolTipMargin * 2, 1000);
            titleSize = TextRenderer.MeasureText(m_implant.Name, m_titleFont, titleSize);
            TextRenderer.DrawText(e.Graphics, m_implant.Name, m_titleFont,
                                  new Point(ToolTipMargin, ToolTipMargin), SystemColors.ControlText, TooltipFlags);

            // Content
            var top = ToolTipMargin + titleSize.Height + InnerMargin;
            var rect = new Rectangle(ToolTipMargin, top, Width - 2 * ToolTipMargin, Height - (top + ToolTipMargin));
            TextRenderer.DrawText(e.Graphics, m_implant.Description, m_toolTipFont, rect, SystemColors.ControlText, TooltipFlags);
        }
    }
}