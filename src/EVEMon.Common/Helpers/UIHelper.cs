using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EVEMon.Common.Controls;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.UISettings;
using EVEMon.Common.Extensions;
using EVEMon.Common.Models;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.Common.Helpers
{
    /// <summary>
    /// Saves a couple of repetitive tasks.
    /// </summary>
    public static class UIHelper
    {
        public static Bitmap CharacterMonitorScreenshot { get; set; }

        /// <summary>
        /// Saves the plans to a file.
        /// </summary>
        /// <param name="plans">The plans.</param>
        public static async Task SavePlansAsync(IList<Plan> plans)
        {
            var character = (Character)plans.First().Character;

            // Prompt the user to pick a file name
            using (var sfdSave = new SaveFileDialog())
            {
                sfdSave.FileName = $"{character.Name} - Plans Backup";
                sfdSave.Title = @"Save to File";
                sfdSave.Filter = @"EVEMon Plans Backup Format (*.epb)|*.epb";
                sfdSave.FilterIndex = (int)PlanFormat.Emp;

                if (sfdSave.ShowDialog() == DialogResult.Cancel)
                    return;

                try
                {
                    var content = PlanIOHelper.ExportAsXML(plans);

                    // Moves to the final file
                    await FileHelper.OverwriteOrWarnTheUserAsync(
                        sfdSave.FileName,
                        async fs =>
                            {
                                // Emp is actually compressed xml
                                Stream stream = new GZipStream(fs, CompressionMode.Compress);
                                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                                {
                                    await writer.WriteAsync(content);
                                    await writer.FlushAsync();
                                    await stream.FlushAsync();
                                    await fs.FlushAsync();
                                }
                                return true;
                            });
                }
                catch (IOException err)
                {
                    ExceptionHandler.LogException(err, false);
                    MessageBox.Show($"There was an error writing out the file:\n\n{err.Message}",
                                    @"Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Displays the plan exportation window and then exports it.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static async Task ExportPlanAsync(Plan plan)
        {
            plan.ThrowIfNull(nameof(plan));

            await ExportPlanAsync(plan, (Character)plan.Character);
        }

        /// <summary>
        /// Exports the character's selected skills as plan.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="selectedSkills">The selected skills.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static async Task ExportCharacterSkillsAsPlanAsync(Character character, IEnumerable<Skill> selectedSkills = null)
        {
            character.ThrowIfNull(nameof(character));

            // Create a character without any skill
            var scratchpad = new CharacterScratchpad(character);
            scratchpad.ClearSkills();

            // Create a new plan
            var plan = new Plan(scratchpad) { Name = "Skills Plan" };

            var skills = selectedSkills ?? character.Skills.Where(skill => skill.IsPublic);

            // Add all trained skill levels that the character has trained so far
            foreach (var skill in skills)
            {
                plan.PlanTo(skill, skill.Level);
            }

            await ExportPlanAsync(plan, character);
        }

        /// <summary>
        /// Displays the plan exportation window and then exports it.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="character">The character.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.NotImplementedException"></exception>
        private static async Task ExportPlanAsync(Plan plan, Character character)
        {
            plan.ThrowIfNull(nameof(plan));

            character.ThrowIfNull(nameof(character));

            // Assemble an initial filename and remove prohibited characters
            var planSaveName = $"{character.Name} - {plan.Name}";
            var invalidFileChars = Path.GetInvalidFileNameChars();
            var fileInd = planSaveName.IndexOfAny(invalidFileChars);
            while (fileInd != -1)
            {
                planSaveName = planSaveName.Replace(planSaveName[fileInd], '-');
                fileInd = planSaveName.IndexOfAny(invalidFileChars);
            }

            // Prompt the user to pick a file name
            using (var sfdSave = new SaveFileDialog())
            {
                sfdSave.FileName = planSaveName;
                sfdSave.Title = @"Save to File";
                sfdSave.Filter =
                    @"EVEMon Plan Format (*.emp)|*.emp|XML  Format (*.xml)|*.xml|Text Format (*.txt)|*.txt";
                sfdSave.FilterIndex = (int)PlanFormat.Emp;

                if (sfdSave.ShowDialog() == DialogResult.Cancel)
                    return;

                // Serialize
                try
                {
                    var format = (PlanFormat)sfdSave.FilterIndex;

                    string content;
                    switch (format)
                    {
                        case PlanFormat.Emp:
                        case PlanFormat.Xml:
                            content = PlanIOHelper.ExportAsXML(plan);
                            break;
                        case PlanFormat.Text:
                            // Prompts the user and returns if canceled
                            var settings = PromptUserForPlanExportSettings(plan);
                            if (settings == null)
                                return;

                            content = PlanIOHelper.ExportAsText(plan, settings);
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    // Moves to the final file
                    await FileHelper.OverwriteOrWarnTheUserAsync(
                        sfdSave.FileName,
                        async fs =>
                            {
                                var stream = fs;
                                // Emp is actually compressed text
                                if (format == PlanFormat.Emp)
                                    stream = new GZipStream(fs, CompressionMode.Compress);

                                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                                {
                                    await writer.WriteAsync(content);
                                    await writer.FlushAsync();
                                    await stream.FlushAsync();
                                    await fs.FlushAsync();
                                }
                                return true;
                            });
                }
                catch (IOException err)
                {
                    ExceptionHandler.LogException(err, true);
                    MessageBox.Show($"There was an error writing out the file:\n\n{err.Message}",
                                    @"Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Prompt the user to select plan exportation settings.
        /// </summary>
        /// <returns></returns>
        public static PlanExportSettings PromptUserForPlanExportSettings(Plan plan)
        {
            var settings = Settings.Exportation.PlanToText;
            using (var f = new CopySaveOptionsWindow(settings, plan, false))
            {
                if (settings.Markup == MarkupType.Undefined)
                    settings.Markup = MarkupType.None;

                f.ShowDialog();
                if (f.DialogResult == DialogResult.Cancel)
                    return null;

                // Save the new settings
                if (!f.SetAsDefault)
                    return settings;

                Settings.Exportation.PlanToText = settings;
                Settings.Save();

                return settings;
            }
        }

        /// <summary>
        /// Displays the character exportation window and then exports it.
        /// Optionally it exports it as it would be after the plan finish.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="plan">The plan.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">character</exception>
        public static async Task ExportCharacterAsync(Character character, Plan plan = null)
        {
            character.ThrowIfNull(nameof(character));

            var isAfterPlanExport = plan != null;

            // Open the dialog box
            using (var characterSaveDialog = new SaveFileDialog())
            {
                characterSaveDialog.Title = $"Save {(isAfterPlanExport ? "After Plan " : string.Empty)}Character Info";
                characterSaveDialog.Filter =
                    @"Text Format|*.txt|CHR Format (EFT)|*.chr|HTML Format|*.html|XML Format (EVEMon)|*.xml";

                if (!isAfterPlanExport)
                    characterSaveDialog.Filter += @"|XML Format (CCP API)|*.xml|PNG Image|*.png";

                characterSaveDialog.FileName =
                    $"{character.Name}{(isAfterPlanExport ? $" (after plan {plan.Name})" : string.Empty)}";

                characterSaveDialog.FilterIndex = isAfterPlanExport
                                                      ? (int)CharacterSaveFormat.EVEMonXML
                                                      : (int)CharacterSaveFormat.CCPXML;

                if (characterSaveDialog.ShowDialog() == DialogResult.Cancel)
                    return;

                // Serialize
                try
                {
                    var format = (CharacterSaveFormat)characterSaveDialog.FilterIndex;

                    // Save character with the chosen format to our file
                    await FileHelper.OverwriteOrWarnTheUserAsync(
                        characterSaveDialog.FileName,
                        async fs =>
                            {
                                if (format == CharacterSaveFormat.PNG)
                                {
                                    Image image = CharacterMonitorScreenshot;
                                    image.Save(fs, ImageFormat.Png);
                                    await fs.FlushAsync();
                                    return true;
                                }

                                var content = CharacterExporter.Export(format, character, plan);
                                if ((format == CharacterSaveFormat.CCPXML) && string.IsNullOrEmpty(content))
                                {
                                    MessageBox.Show(
                                        @"This character has never been downloaded from CCP, cannot find it in the XML cache.",
                                        @"Cannot export the character", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return false;
                                }

                                using (var sw = new StreamWriter(fs))
                                {
                                    await sw.WriteAsync(content);
                                    await sw.FlushAsync();
                                    await fs.FlushAsync();
                                }
                                return true;
                            });
                }
                    // Handle exception
                catch (IOException exc)
                {
                    ExceptionHandler.LogException(exc, true);
                    MessageBox.Show(@"A problem occurred during exportation. The operation has not been completed.");
                }
            }
        }

        /// <summary>
        /// Adds the plans as toolstrip items to the list.
        /// </summary>
        /// <param name="plans">The plans.</param>
        /// <param name="list">The list.</param>
        /// <param name="initialize">The initialize.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void AddTo(this IEnumerable<Plan> plans, ToolStripItemCollection list,
                                 Action<ToolStripMenuItem, Plan> initialize)
        {
            plans.ThrowIfNull(nameof(plans));

            list.ThrowIfNull(nameof(list));

            initialize.ThrowIfNull(nameof(initialize));

            //Scroll through plans
            foreach (var plan in plans)
            {
                ToolStripMenuItem item;
                using (var planItem = new ToolStripMenuItem(plan.Name))
                {
                    initialize(planItem, plan);
                    item = planItem;
                }
                list.Add(item);
            }
        }

        /// <summary>
        /// Shows a no support message.
        /// </summary>
        /// <returns></returns>
        internal static object ShowNoSupportMessage()
        {
            MessageBox.Show($"The file is probably from an EVEMon version prior to 1.3.0.{Environment.NewLine}" +
                            @"This type of file is no longer supported.",
                            @"File type not supported", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return null;
        }
    }
}