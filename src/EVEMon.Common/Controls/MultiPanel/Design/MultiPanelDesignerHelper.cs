﻿using System.ComponentModel.Design;
using System.Linq;
using System.Windows.Forms;

namespace EVEMon.Common.Controls.MultiPanel.Design
{
    public static class MultiPanelDesignerHelper
    {
        /// <summary>
        /// Gets the collection of verbs displayed in the top right menu of the designer.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="panel"></param>
        /// <returns></returns>
        public static DesignerVerbCollection GetDesignerVerbs(IDesignerHost host, MultiPanel panel)
        {
            var verbs = new DesignerVerbCollection
                                               {
                                                   new DesignerVerb("Add Page", (sender, args) => AddPage(host, panel)),
                                                   new DesignerVerb("Remove Page", (sender, args) => RemovePage(host, panel))
                                               };

            foreach (MultiPanelPage page in panel.Controls)
            {
                var pageCopy = page;
                verbs.Add(new DesignerVerb("Select \"" + page.Text + "\"", (sender, args) => SelectPage(host, panel, pageCopy)));
            }

            return verbs;
        }

        /// <summary>
        /// Event handler for the "Add Page" verb.
        /// </summary>
        /// <param name="dh"></param>
        /// <param name="panel"></param>
        private static void AddPage(IDesignerHost dh, MultiPanel panel)
        {
            var dt = dh.CreateTransaction("Added new page");

            // Gets a free name
            var i = 1;
            while (panel.Controls.Cast<Control>().Any(x => x.Name == "Page" + i))
            {
                i++;
            }
            var name = "Page" + i;

            // Creates the page
            var newPage = dh.CreateComponent(typeof(MultiPanelPage), name) as MultiPanelPage;
            if (newPage != null)
            {
                newPage.Text = name;
                panel.Controls.Add(newPage);

                // Update selection
                panel.SelectedPage = newPage;
            }

            dt.Commit();
        }

        /// <summary>
        /// Event handler for the "Remove Tab" verb.
        /// </summary>
        /// <param name="dh"></param>
        /// <param name="panel"></param>
        private static void RemovePage(IDesignerHost dh, MultiPanel panel)
        {
            var page = panel.SelectedPage;
            if (page == null)
                return;

            var dt = dh.CreateTransaction("Removed page");

            panel.Controls.Remove(page);
            dh.DestroyComponent(page);

            panel.SelectedPage = panel.Controls.Count > 0 ? (MultiPanelPage)panel.Controls[0] : null;

            dt.Commit();
        }

        /// <summary>
        /// Event handler for the "Select X page" handler.
        /// </summary>
        /// <param name="dh"></param>
        /// <param name="panel"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        private static void SelectPage(IDesignerHost dh, MultiPanel panel, MultiPanelPage page)
        {
            var dt = dh.CreateTransaction("Selected page");

            panel.SelectedPage = page;
            dt.Commit();
        }
    }
}