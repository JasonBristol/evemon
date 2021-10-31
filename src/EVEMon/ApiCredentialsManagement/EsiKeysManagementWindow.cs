using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EVEMon.Common;
using EVEMon.Common.Constants;
using EVEMon.Common.Controls;
using EVEMon.Common.CustomEventArgs;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Factories;
using EVEMon.Common.Models;

namespace EVEMon.ApiCredentialsManagement
{
    public partial class EsiKeysManagementWindow : EVEMonForm
    {
        private readonly Dictionary<Character, bool> m_monitoredCharacters = new Dictionary<Character, bool>();

        private int m_refreshingCharactersCounter;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EsiKeysManagementWindow()
        {
            InitializeComponent();

            esiKeysListBox.Font = FontFactory.GetFont("Tahoma", 9.75f);
            charactersListView.Font = FontFactory.GetFont("Tahoma", 9.75f);
            esiKeysLabel.Font = FontFactory.GetFont("Tahoma", 12F);
            esiKeyListLabel.Font = FontFactory.GetFont("Tahoma", 12F);
            charactersLabel.Font = FontFactory.GetFont("Tahoma", 12F);
            charactersListLabel.Font = FontFactory.GetFont("Tahoma", 12F);
        }

        /// <summary>
        /// On loading, intialize the controls and subscribe events.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode)
                return;

            ListViewHelper.EnableDoubleBuffer(charactersListView);

            EveMonClient.ESIKeyCollectionChanged += EveMonClient_ESIKeyCollectionChanged;
            EveMonClient.ESIKeyInfoUpdated += EveMonClient_ESIKeyInfoUpdated;
            EveMonClient.CharacterCollectionChanged += EveMonClient_CharacterCollectionChanged;
            EveMonClient.CharacterUpdated += EveMonClient_CharacterUpdated;
            EveMonClient.AccountStatusUpdated += EveMonClient_AccountStatusUpdated;
            Disposed += OnDisposing;

            UpdateESIKeysList();
            UpdateCharactersList();
            AdjustLastColumn();

            // Selects the second page if no API key known so far
            if (EveMonClient.Characters.Count == 0)
                tabControl.SelectedIndex = 1;
        }

        /// <summary>
        /// Occurs on disposing.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnDisposing(object sender, EventArgs e)
        {
            // Unsubscribe events
            EveMonClient.ESIKeyCollectionChanged -= EveMonClient_ESIKeyCollectionChanged;
            EveMonClient.ESIKeyInfoUpdated -= EveMonClient_ESIKeyInfoUpdated;
            EveMonClient.CharacterCollectionChanged -= EveMonClient_CharacterCollectionChanged;
            EveMonClient.CharacterUpdated -= EveMonClient_CharacterUpdated;
            EveMonClient.AccountStatusUpdated -= EveMonClient_AccountStatusUpdated;
            Disposed -= OnDisposing;

            // Update the monitored status of selected characters
            foreach (var monitoredCharacter in m_monitoredCharacters)
                if (EveMonClient.Characters.Contains(monitoredCharacter.Key))
                    monitoredCharacter.Key.Monitored = monitoredCharacter.Value;
        }

        /// <summary>
        /// When the size changes, we adjust the characters' columns.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (charactersListView != null)
                AdjustLastColumn();
        }


        #region Global Events Handlers

        /// <summary>
        /// When the ESI key collection changes, we update the content.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_ESIKeyCollectionChanged(object sender, EventArgs e)
        {
            UpdateESIKeysList();
        }

        /// <summary>
        /// When the ESI key info updates, we update the content.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_ESIKeyInfoUpdated(object sender, EventArgs e)
        {
            if (!Visible)
                return;

            esiKeysListBox.Invalidate();
        }

        /// <summary>
        /// When the characters collection changed, we update the characters list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_CharacterCollectionChanged(object sender, EventArgs e)
        {
            UpdateCharactersList();
        }

        /// <summary>
        /// When the character changes, the displayed names changes too.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_CharacterUpdated(object sender, CharacterChangedEventArgs e)
        {
            UpdateCharactersList();
        }

        /// <summary>
        /// When the account status updates, we update the content.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EveMonClient_AccountStatusUpdated(object sender, EventArgs e)
        {
            if (!Visible)
                return;

            esiKeysListBox.Invalidate();
        }

        #endregion


        #region ESI keys management

        /// <summary>
        /// Updates the ESI keys list.
        /// </summary>
        private void UpdateESIKeysList()
        {
            if (!Visible)
                return;

            esiKeysListBox.ESIKeys = EveMonClient.ESIKeys;
            esiKeysMultiPanel.SelectedPage = EveMonClient.ESIKeys.Any() ? esiKeysListPage : noESIKeysPage;
        }

        /// <summary>
        /// Handles the MouseClick event of the apiKeysListBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void apiKeysListBox_MouseClick(object sender, MouseEventArgs e)
        {
            var itemClicked = false;

            // Search for the clicked item
            for (var index = 0; index < esiKeysListBox.ESIKeys.Count(); index++)
            {
                var rect = esiKeysListBox.GetItemRectangle(index);

                // Did click occured generally on the item ?
                if (!rect.Contains(e.Location))
                    continue;

                itemClicked = true;

                var yOffset = (rect.Height - EsiKeysListBox.CheckBoxSize.Height) / 2;
                var cbRect = new Rectangle(rect.Left + esiKeysListBox.Margin.Left, rect.Top + yOffset,
                                                 EsiKeysListBox.CheckBoxSize.Width, EsiKeysListBox.CheckBoxSize.Height);
                cbRect.Inflate(2, 2);

                // Did click occured on the checkbox ?
                if (e.Button == MouseButtons.Middle || !cbRect.Contains(e.Location))
                    continue;

                var esiKey = esiKeysListBox.ESIKeys.ElementAt(index);
                esiKey.Monitored = !esiKey.Monitored;
                esiKeysListBox.Invalidate();
            }

            if (!itemClicked)
                esiKeysListBox.SelectedIndex = -1;
        }

        /// <summary>
        /// When the selection changes, we update the controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void apiKeysListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            deleteESIKeyMenu.Enabled = esiKeysListBox.SelectedIndex != -1;
            editESIKeyMenu.Enabled = esiKeysListBox.SelectedIndex != -1;
        }

        /// <summary>
        /// On double click, forces the edition.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void apiKeysListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Search for the double-clicked item
            var index = 0;
            foreach (var esiKey in esiKeysListBox.ESIKeys)
            {
                var rect = esiKeysListBox.GetItemRectangle(index);
                index++;

                if (!rect.Contains(e.Location))
                    continue;

                // Open the edition window
                using (var window = new EsiKeyUpdateOrAdditionWindow(esiKey))
                {
                    window.ShowDialog(this);
                    return;
                }
            }
        }

        /// <summary>
        /// API key toolbar > Edit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editAPIKeyMenu_Click(object sender, EventArgs e)
        {
            var esiKey = esiKeysListBox.ESIKeys.ElementAt(esiKeysListBox.SelectedIndex);
            using (var window = new EsiKeyUpdateOrAdditionWindow(esiKey))
            {
                window.ShowDialog(this);
            }
        }

        /// <summary>
        /// ESI key toolbar > Add.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addESIKeyMenu_Click(object sender, EventArgs e)
        {
            using (var window = new EsiKeyUpdateOrAdditionWindow())
            {
                window.ShowDialog(this);
            }
        }

        /// <summary>
        /// Accounts toolbar > Delete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteESIKeyMenu_Click(object sender, EventArgs e)
        {
            if (esiKeysListBox.SelectedIndex == -1)
                return;

            var apiKey = esiKeysListBox.ESIKeys.ElementAt(esiKeysListBox.SelectedIndex);
            using (var window = new EsiKeyDeletionWindow(apiKey))
            {
                window.ShowDialog(this);
            }

            deleteESIKeyMenu.Enabled = esiKeysListBox.SelectedIndex != -1;
            editESIKeyMenu.Enabled = esiKeysListBox.SelectedIndex != -1;
        }

        /// <summary>
        /// Handles the KeyDown event of the esiKeysListBox control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void esiKeysListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                deleteESIKeyMenu_Click(sender, e);
        }

        #endregion


        #region Characters management

        /// <summary>
        /// Updates the characters list.
        /// </summary>
        private void UpdateCharactersList()
        {
            if (!Visible)
                return;

            // Begin the update
            m_refreshingCharactersCounter++;

            // Update the list view item
            UpdateCharactersListContent();

            // Invalidates the API keys list
            esiKeysListBox.Invalidate();

            // Make a help message appears when no characters exist
            charactersMultiPanel.SelectedPage = EveMonClient.Characters.Count == 0 ? noCharactersPage : charactersListPage;

            // End of the update
            m_refreshingCharactersCounter--;
        }

        /// <summary>
        /// Recreate the items in the characters listview
        /// </summary>
        private void UpdateCharactersListContent()
        {
            var position = charactersListView.GetVerticalScrollBarPosition();

            charactersListView.BeginUpdate();
            try
            {
                // Retrieve current selection and grouping option
                var oldSelection = new List<Character>(charactersListView.SelectedItems.Cast<ListViewItem>()
                                                                       .Select(x => x.Tag).OfType<Character>());

                charactersListView.Groups.Clear();
                charactersListView.Items.Clear();

                // Grouping (no ESI key, ESI key #1, ESI key #2, character files, character urls)
                var isGrouping = groupingMenu.Checked;
                var noESIKeyGroup = new ListViewGroup("No ESI key");
                var fileGroup = new ListViewGroup("Character files");
                var urlGroup = new ListViewGroup("Character urls");
                var apiKeyGroups = new Dictionary<ESIKey, ListViewGroup>();

                if (isGrouping)
                    ArrangeByGroup(fileGroup, apiKeyGroups, noESIKeyGroup, urlGroup);

                // Add items
                foreach (var character in EveMonClient.Characters.OrderBy(x => x.Name))
                {
                    var item = new ListViewItem { Checked = character.Monitored, Tag = character };

                    // Retrieve the texts for the different columns
                    IEnumerable<ESIKey> esiKeys = character.Identity.ESIKeys.OrderBy(esiKey => esiKey.ID);
                    var apiKeyIDText = esiKeys.Any()
                                              ? string.Join(", ", esiKeys.Select(esiKey => esiKey.ID))
                                              : string.Empty;
                    var typeText = "CCP";
                    var uriText = "-";

                    var uriCharacter = character as UriCharacter;
                    if (uriCharacter != null)
                    {
                        typeText = uriCharacter.Uri.IsFile ? "File" : "Url";
                        uriText = uriCharacter.Uri.ToString();

                        if (isGrouping)
                            item.Group = uriCharacter.Uri.IsFile ? fileGroup : urlGroup;
                    }
                        // Grouping CCP characters
                    else if (isGrouping)
                    {
                        if (!esiKeys.Any())
                            item.Group = noESIKeyGroup;
                        else
                            item.Group = apiKeyGroups[esiKeys.First()];
                    }

                    // Add the item and its subitems
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, typeText));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, character.CharacterID.ToString(
                        CultureConstants.DefaultCulture)));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, character.Name));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, apiKeyIDText));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, character.
                        EffectiveCharacterStatus.ToString()));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, uriText));

                    charactersListView.Items.Add(item);

                    if (oldSelection.Contains(character))
                        item.Selected = true;
                }

                // Auto-resize the columns
                charactersListView.Columns.Cast<ColumnHeader>().Where(
                    column => column.Index != charactersListView.Columns.Count - 1).ToList().ForEach(column => column.Width = -2);

                AdjustLastColumn();
            }
            finally
            {
                charactersListView.EndUpdate();
                charactersListView.SetVerticalScrollBarPosition(position);
            }

            // Forces a refresh of the enabled/disabled items
            UpdateControlsUsability();
        }

        /// <summary>
        /// Arranges the by group.
        /// </summary>
        /// <param name="fileGroup">The file group.</param>
        /// <param name="esiKeyGroups">The ESI key groups.</param>
        /// <param name="noESIKeyGroup">The no ESI key group.</param>
        /// <param name="urlGroup">The URL group.</param>
        private void ArrangeByGroup(ListViewGroup fileGroup, Dictionary<ESIKey, ListViewGroup> esiKeyGroups,
                                    ListViewGroup noESIKeyGroup, ListViewGroup urlGroup)
        {
            var hasNoESIKey = false;
            var hasFileChars = false;
            var hasUrlChars = false;

            // Scroll through listview items to gather the groups
            foreach (var character in EveMonClient.Characters)
            {
                var uriCharacter = character as UriCharacter;

                // Uri character ?
                if (uriCharacter != null)
                {
                    if (uriCharacter.Uri.IsFile)
                        hasFileChars = true;
                    else
                        hasUrlChars = true;
                }
                // CCP character ?
                else
                {
                    if (!character.Identity.ESIKeys.Any())
                        hasNoESIKey = true;
                    else
                    {
                        foreach (var apiKey in character.Identity.ESIKeys.Where(
                            esiKey => !esiKeyGroups.ContainsKey(esiKey)))
                        {
                            esiKeyGroups.Add(apiKey, new ListViewGroup($"Key ID #{apiKey.ID}"));
                        }
                    }
                }
            }

            // Add the groups
            if (hasNoESIKey)
                charactersListView.Groups.Add(noESIKeyGroup);

            foreach (var group in esiKeyGroups.Values)
            {
                charactersListView.Groups.Add(group);
            }

            if (hasFileChars)
                charactersListView.Groups.Add(fileGroup);

            if (hasUrlChars)
                charactersListView.Groups.Add(urlGroup);
        }

        /// <summary>
        /// Adjusts the last column width.
        /// </summary>
        private void AdjustLastColumn()
        {
            var lastColumn = charactersListView.Columns[charactersListView.Columns.Count - 1];
            var pad = Size.Width - charactersListView.Size.Width;
            var width = charactersListView.Columns.Cast<ColumnHeader>().Where(column => column.Index != lastColumn.Index).Select(
                column => column.Width).Sum();

            var lastColumnMaxWidth = charactersListView.Columns[lastColumn.Index].ListView.Items.Cast<ListViewItem>().Select(
                item => TextRenderer.MeasureText(item.SubItems[lastColumn.Index].Text, Font).Width).Concat(
                    new[] { TextRenderer.MeasureText(charactersListView.Columns[lastColumn.Index].Text, Font).Width }).
                                         Concat(new[] { charactersListView.ClientSize.Width - width - pad }).Max() + pad;

            lastColumn.Width = lastColumnMaxWidth;
        }

        /// <summary>
        /// Updates the controls usability.
        /// </summary>
        private void UpdateControlsUsability()
        {
            // "Edit uri" enabled when an uri char is selected
            editUriMenu.Enabled = charactersListView.SelectedItems.Count > 0 &&
                                  charactersListView.SelectedItems[0].Tag is UriCharacter;

            // Delete char enabled if one character selected
            deleteCharacterMenu.Enabled = charactersListView.SelectedItems.Count > 0;
        }

        /// <summary>
        /// We monitor/unmonitor characters as they are checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void charactersListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (m_refreshingCharactersCounter != 0)
                return;

            // Add the character with changed monitoring status to the dictionary,
            // we will deal with them on closing
            var character = (Character)e.Item.Tag;
            m_monitoredCharacters[character] = e.Item.Checked;
        }

        /// <summary>
        /// Handle the "delete" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void charactersListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                deleteCharacterMenu_Click(sender, e);
        }

        /// <summary>
        /// On double click, we edit if this is an uri character.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void charactersListView_DoubleClick(object sender, EventArgs e)
        {
            editUriButton_Click(sender, e);
        }

        /// <summary>
        /// When the index changes, we enable or disable the toolbar buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void charactersListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControlsUsability();
        }

        /// <summary>
        /// Characters toolbar > Import...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importCharacterMenu_Click(object sender, EventArgs e)
        {
            using (var form = new CharacterImportationWindow())
            {
                form.ShowDialog(this);
            }
        }

        /// <summary>
        /// Characters toolbar > Delete...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteCharacterMenu_Click(object sender, EventArgs e)
        {
            // Retrieve the selected URI character
            if (charactersListView.SelectedItems.Count == 0)
                return;

            var item = charactersListView.SelectedItems[0];
            var character = item.Tag as Character;

            // Opens the character deletion
            using (var window = new CharacterDeletionWindow(character))
            {
                window.ShowDialog(this);
            }
        }

        /// <summary>
        /// Characters toolbar > Edit Uri...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editUriButton_Click(object sender, EventArgs e)
        {
            // Retrieve the selected URI character
            if (charactersListView.SelectedItems.Count == 0)
                return;

            var item = charactersListView.SelectedItems[0];
            var uriCharacter = item.Tag as UriCharacter;

            // Returns if the selected item is not an Uri character
            if (uriCharacter == null)
                return;

            // Opens the importation form
            using (var form = new CharacterImportationWindow(uriCharacter))
            {
                form.ShowDialog(this);
            }
        }

        /// <summary>
        /// Characters toolbar > Group items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void groupingMenu_Click(object sender, EventArgs e)
        {
            m_refreshingCharactersCounter++;
            UpdateCharactersListContent();
            m_refreshingCharactersCounter--;
        }

        #endregion


        #region Local Events

        /// <summary>
        /// Close on "close" button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the ColumnWidthChanging event of the charactersListView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ColumnWidthChangingEventArgs"/> instance containing the event data.</param>
        private void charactersListView_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            e.NewWidth = charactersListView.Columns[e.ColumnIndex].Width;
        }

        #endregion
    }
}
