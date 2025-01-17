using System;
using System.Drawing;
using System.Windows.Forms;

namespace EVEMon.Controls
{
    /// <summary>
    /// A panel that does not display a gray text when disabled
    /// (and we need to disable them so that the button
    /// does not always lose focus and its nice "I'm hovered" color)
    /// </summary>
    public sealed class OverviewLabel : Label
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OverviewLabel"/> class.
        /// </summary>
        public OverviewLabel()
        {
            DoubleBuffered = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control can respond to user interaction.
        /// </summary>
        /// <value></value>
        /// <returns>true if the control can respond to user interaction; otherwise, false. The default is true.
        /// </returns>
        public new bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Triggered when the label should be repainted.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);

            using (var format = new StringFormat())
            {
                if (AutoEllipsis)
                    format.Trimming = StringTrimming.EllipsisCharacter;

                using (var foreground = new SolidBrush(ForeColor))
                {
                    e.Graphics.DrawString(Text, Font, foreground, Padding.Left, Padding.Right, format);
                }
            }
        }
    }
}