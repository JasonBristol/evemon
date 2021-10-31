using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EVEMon.Common.Controls;
using EVEMon.Common.Models;
using EVEMon.Common.Notifications;

namespace EVEMon.NotificationWindow
{
    public partial class SkillCompletionWindow : EVEMonForm
    {
        private SkillCompletionNotificationEventArgs m_notification;

        /// <summary>
        /// Constructor
        /// </summary>
        public SkillCompletionWindow()
        {
            InitializeComponent();
            RememberPositionKey = "SkillCompletionWindow";
        }

        /// <summary>
        /// Gets or sets the list of completed skills.
        /// </summary> 
        internal SkillCompletionNotificationEventArgs Notification
        {
            get { return m_notification; }
            set
            {
                m_notification = value;
                var text = new StringBuilder();
                foreach (var skill in m_notification.Skills)
                {
                    text.AppendLine(skill.ToString());
                }

                var textSize = TextRenderer.MeasureText(text.ToString(), Font);
                MinimumSize = new Size(280, textSize.Height + 30);

                detailsTextBox.Text = text.ToString();
            }
        }
    }
}