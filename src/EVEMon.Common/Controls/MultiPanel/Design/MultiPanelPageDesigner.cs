﻿using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using EVEMon.Common.Extensions;

namespace EVEMon.Common.Controls.MultiPanel.Design
{
    /// <summary>
    /// A designer hosting a page of the <see cref="MultiPanel"/> control.
    /// </summary>
    /// <remarks>
    /// Based on the work from Liron Levi on Code Project, under public domain. 
    /// See http://www.codeproject.com/KB/cs/multipanelcontrol.aspx
    /// </remarks>
    public class MultiPanelPageDesigner : ScrollableControlDesigner
    {
        private MultiPanelPage m_page;
        private readonly Font m_font = new Font("Courier New", 8F, FontStyle.Bold);

        private readonly StringFormat m_rightFormat =
            new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.DirectionRightToLeft);

        /// <summary>
        /// Overridden. Initializes the component.
        /// </summary>
        /// <param name="component">The <see cref="IComponent"/> hosted by the designer.</param>
        public override void Initialize(IComponent component)
        {
            // Should not happen.
            m_page = component as MultiPanelPage;
            if (m_page == null)
            {
                DisplayError(
                    new InvalidOperationException(
                        "You attempted to use a MultiPanelPageDesigner with a class that does not inherit from MultiPanelPage."));
            }

            base.Initialize(component);
        }

        /// <summary>
        /// Prevents the user to assign this page to anything else than a MultiPanel.
        /// </summary>
        /// <param name="parentDesigner"></param>
        /// <returns></returns>
        public override bool CanBeParentedTo(IDesigner parentDesigner) => (parentDesigner != null) && parentDesigner.Component is MultiPanel;

        /// <summary>
        /// Gets or sets the <see cref="MultiPanelPage.Text"/> property of the represented designer.
        /// </summary>
        [Category("Design")]
        [Description("The text identifying the page.")]
        public string Text
        {
            get { return m_page.Text; }
            set
            {
                m_page.Text = value;

                var service = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service == null)
                    return;

                var panel = m_page.Parent as MultiPanel;
                panel?.Refresh();
            }
        }

        /// <summary>
        /// Overridden. Gets the collection of verbs that are available to this designer.
        /// </summary>
        public override DesignerVerbCollection Verbs
        {
            get
            {
                var host = (IDesignerHost)GetService(typeof(IDesignerHost));
                var panel = (MultiPanel)m_page.Parent;
                return MultiPanelDesignerHelper.GetDesignerVerbs(host, panel);
            }
        }

        /// <summary>
        /// Overridden. Paint the adornments on the four corners.
        /// </summary>
        /// <param name="pea">
        /// Some <see cref="PaintEventArgs"/>.
        /// </param>
        protected override void OnPaintAdornments(PaintEventArgs pea)
        {
            base.OnPaintAdornments(pea);

            // My thanks to bschurter (Bruce), CodeProject member #1255339 for this!
            using (var p = new Pen(SystemColors.ControlDark, 1))
            {
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                pea.Graphics.DrawRectangle(p, 0, 0, m_page.Width - 1, m_page.Height - 1);
            }

            using (Brush b = new SolidBrush(Color.FromArgb(100, Color.Black)))
            {
                var fh = m_font.GetHeight(pea.Graphics);
                var tleft = new RectangleF(0, 0, (float)m_page.Width / 2, fh);
                var bleft = new RectangleF(0, m_page.Height - fh, (float)m_page.Width / 2, fh);
                var tright = new RectangleF((float)m_page.Width / 2, 0, (float)m_page.Width / 2, fh);
                var bright = new RectangleF((float)m_page.Width / 2, m_page.Height - fh, (float)m_page.Width / 2, fh);
                pea.Graphics.DrawString(m_page.Text, m_font, b, tleft);
                pea.Graphics.DrawString(m_page.Text, m_font, b, bleft);
                pea.Graphics.DrawString(m_page.Text, m_font, b, tright, m_rightFormat);
                pea.Graphics.DrawString(m_page.Text, m_font, b, bright, m_rightFormat);
            }
        }

        /// <summary>
        /// Overridden. Adds properties to or removes properties from the Properties grid in a design host at design time
        /// or provides new design-time properties that might correspond to properties on the associated control.
        /// </summary>
        /// <param name="properties">The original properties dictionary.</param>
        /// <exception cref="System.ArgumentNullException">properties</exception>
        protected override void PreFilterProperties(IDictionary properties)
        {
            properties.ThrowIfNull(nameof(properties));

            base.PreFilterProperties(properties);
            properties["Text"] = TypeDescriptor.CreateProperty(typeof(MultiPanelPageDesigner),
                                                               (PropertyDescriptor)properties["Text"], new Attribute[0]);
        }
    }
}