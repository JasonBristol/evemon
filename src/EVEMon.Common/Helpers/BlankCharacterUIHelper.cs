using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using EVEMon.Common.Collections;
using EVEMon.Common.Collections.Global;
using EVEMon.Common.Constants;
using EVEMon.Common.Data;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Extensions;
using EVEMon.Common.Models;
using EVEMon.Common.Serialization.Eve;
using EVEMon.Common.Serialization.Settings;

namespace EVEMon.Common.Helpers
{
    public static class BlankCharacterUIHelper
    {
        #region Fields

        private static readonly Dictionary<int, int> s_allRaceSkills = new Dictionary<int, int>
        {
            // Armor -- all empire-specific, see below
            // Drones
            { DBConstants.DroneAvionicsSkillID, 1 },
            { DBConstants.DronesSkillID, 1 },
            // Electronic Systems
            { DBConstants.ElectronicWarfareSkillID, 1 },
            { DBConstants.PropulsionJammingSkillID, 1 },
            // Engineering
            { DBConstants.CapacitorManagementSkillID, 3 },
            { DBConstants.CapacitorSystemsOperationSkillID, 3 },
            { DBConstants.CPUManagementSkillID, 4 },
            { DBConstants.ElectronicsUpgradesSkillID, 3 },
            { DBConstants.EnergyGridUpgradesSkillID, 1 },
            { DBConstants.PowerGridManagementSkillID, 4 },
            { DBConstants.WeaponUpgradesSkillID, 2 },
            // Gunnery -- some empire-specific
            { DBConstants.GunnerySkillID, 4 },
            { DBConstants.MotionPredictionSkillID, 2 },
            { DBConstants.RapidFiringSkillID, 2 },
            { DBConstants.SharpshooterSkillID, 2 },
            { DBConstants.TrajectoryAnalysisSkillID, 1 },
            // Missiles
            { DBConstants.MissileLauncherOperationSkillID, 1 },
            // Navigation
            { DBConstants.AfterburnerSkillID, 3 },
            { DBConstants.EvasiveManeuveringSkillID, 1 },
            { DBConstants.HighSpeedManeuveringSkillID, 1 },
            { DBConstants.NavigationSkillID, 3 },
            { DBConstants.WarpDriveOperationSkillID, 2 },
            // Neural Enhancement
            { DBConstants.CyberneticsSkillID, 1 },
            // Production
            { DBConstants.IndustrySkillID, 1 },
            // Resource Processing
            { DBConstants.MiningSkillID, 3 },
            { DBConstants.SalvagingSkillID, 3 },
            // Scanning
            { DBConstants.ArchaeologySkillID, 1 },
            { DBConstants.AstrometricAcquisitionSkillID, 1 },
            { DBConstants.AstrometricRangefindingSkillID, 1 },
            { DBConstants.AstrometricsSkillID, 3 },
            { DBConstants.HackingSkillID, 1 },
            { DBConstants.SurveySkillID, 3 },
            // Science
            { DBConstants.ScienceSkillID, 4 },
            // Shield -- all empire-specific
            // Spaceship Command -- some empire-specific
            { DBConstants.MiningFrigateSkillID, 1 },
            { DBConstants.SpaceshipCommandSkillID, 3 },
            // Targeting
            { DBConstants.LongRangeTargetingSkillID, 2 },
            { DBConstants.SignatureAnalysisSkillID, 2 },
            { DBConstants.TargetManagementSkillID, 3 },
            // Trade
            { DBConstants.MarketingSkillID, 1 },
            { DBConstants.TradeSkillID, 2 }
        };

        private static readonly Dictionary<int, int> s_amarrRaceSkills = new Dictionary<int, int>
        {
            // Armor
            { DBConstants.HullUpgradesSkillID, 3 },
            { DBConstants.MechanicSkillID, 3 },
            // Gunnery
            { DBConstants.SmallEnergyTurretSkillID, 1 },
            { DBConstants.ControlledBurstsSkillID, 2 },
            // Shield
            { DBConstants.ShieldManagementSkillID, 1 },
            { DBConstants.ShieldUpgradesSkillID, 1 },
            { DBConstants.TacticalShieldManipulationSkillID, 1 },
            // Spaceship Command
            { DBConstants.AmarrFrigateSkillID, 1 },
            { DBConstants.AmarrIndustrialSkillID, 1 }
       };

        private static readonly Dictionary<int, int> s_caldariRaceSkills = new Dictionary<int, int>
        {
            // Armor
            { DBConstants.HullUpgradesSkillID, 2 },
            { DBConstants.MechanicSkillID, 2 },
            // Gunnery
            { DBConstants.SmallHybridTurretSkillID, 1 },
            { DBConstants.ControlledBurstsSkillID, 2 },
            // Shield
            { DBConstants.ShieldManagementSkillID, 2 },
            { DBConstants.ShieldUpgradesSkillID, 2 },
            { DBConstants.TacticalShieldManipulationSkillID, 2 },
            // Spaceship Command
            { DBConstants.CaldariFrigateSkillID, 1 },
            { DBConstants.CaldariIndustrialSkillID, 1 }
        };

        private static readonly Dictionary<int, int> s_gallenteRaceSkills = new Dictionary<int, int>
        {
            // Armor
            { DBConstants.HullUpgradesSkillID, 3 },
            { DBConstants.MechanicSkillID, 3 },
            // Gunnery
            { DBConstants.SmallHybridTurretSkillID, 1 },
            { DBConstants.ControlledBurstsSkillID, 2 },
            // Shield
            { DBConstants.ShieldManagementSkillID, 1 },
            { DBConstants.ShieldUpgradesSkillID, 1 },
            { DBConstants.TacticalShieldManipulationSkillID, 1 },
            // Spaceship Command
            { DBConstants.GallenteFrigateSkillID, 1 },
            { DBConstants.GallenteIndustrialSkillID, 1 }
        };

        private static readonly Dictionary<int, int> s_minmatarRaceSkills = new Dictionary<int, int>
        {
            // Armor
            { DBConstants.HullUpgradesSkillID, 3 },
            { DBConstants.MechanicSkillID, 3 },
            // Gunnery
            { DBConstants.SmallProjectileTurretSkillID, 1 },
            { DBConstants.ControlledBurstsSkillID, 1 },
            // Shield
            { DBConstants.ShieldManagementSkillID, 1 },
            { DBConstants.ShieldUpgradesSkillID, 1 },
            { DBConstants.TacticalShieldManipulationSkillID, 1 },
            // Spaceship Command
            { DBConstants.MinmatarFrigateSkillID, 1 },
            { DBConstants.MinmatarIndustrialSkillID, 1 }
        };

        private static string s_filename;

        #endregion


        #region Properties

        /// <summary>
        /// Gets or sets the name of the character.
        /// </summary>
        /// <value>
        /// The name of the character.
        /// </value>
        public static string CharacterName { get; set; }

        /// <summary>
        /// Gets or sets the race.
        /// </summary>
        /// <value>
        /// The race.
        /// </value>
        public static Race Race { get; set; }

        /// <summary>
        /// Sets the gender.
        /// </summary>
        /// <value>
        /// The gender.
        /// </value>
        public static Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets the bloodline.
        /// </summary>
        /// <value>
        /// The bloodline.
        /// </value>
        public static Bloodline Bloodline { get; set; }

        /// <summary>
        /// Sets the ancestry.
        /// </summary>
        /// <value>
        /// The ancestry.
        /// </value>
        public static Ancestry Ancestry { get; set; }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Creates the character.
        /// </summary>
        /// <returns></returns>
        private static SerializableCCPCharacter CreateCharacter()
        {
            var serial = new SerializableCCPCharacter
            {
                ID = UriCharacter.BlankCharacterID,
                Name = CharacterName,
                Birthday = DateTime.UtcNow,
                Race = Race.ToString(),
                BloodLine = Bloodline.ToString().Replace("_", "-"),
                Ancestry = Ancestry.ToString().Replace("_", " "),
                Gender = Gender.ToString(),
                CorporationName = "Blank Character's Corp",
                CorporationID = 9999999,
                Balance = 0,
                // Default to Omega clones
                CloneState = AccountStatusMode.Omega.ToString(),
                Attributes = new SerializableCharacterAttributes
                {
                    Intelligence = EveConstants.CharacterBaseAttributePoints + 3,
                    Memory = EveConstants.CharacterBaseAttributePoints + 3,
                    Perception = EveConstants.CharacterBaseAttributePoints + 3,
                    Willpower = EveConstants.CharacterBaseAttributePoints + 3,
                    Charisma = EveConstants.CharacterBaseAttributePoints + 2
                },
                ImplantSets = new SerializableImplantSetCollection
                {
                    ActiveClone = new SerializableSettingsImplantSet
                    { Name = "Active Clone" },
                },
            };

            serial.Skills.AddRange(GetSkillsForRace());

            return serial;
        }

        /// <summary>
        /// Gets the skills for each race.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<SerializableCharacterSkill> GetSkillsForRace()
        {
            var startingSkills = GetStartingSkills();

            return startingSkills.Select(
                raceSkill => new
                {
                    raceSkill,
                    staticSkill = StaticSkills.GetSkillByID(raceSkill.Key)
                }).Where(raceSkill => raceSkill.staticSkill != null).Select(
                    skill => new SerializableCharacterSkill
                    {
                        ID = skill.raceSkill.Key,
                        Level = skill.raceSkill.Value,
                        Name = StaticSkills.GetSkillByID(skill.raceSkill.Key).Name,
                        Skillpoints =
                            StaticSkills.GetSkillByID(skill.raceSkill.Key).GetPointsRequiredForLevel
                                (skill.raceSkill.Value),
                        IsKnown = true,
                        OwnsBook = false,
                    });
        }

        /// <summary>
        /// Gets the starting skills.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<int, int> GetStartingSkills()
        {
            var startingSkills = new Dictionary<int, int>();

            switch (Race)
            {
                case Race.Amarr:
                    startingSkills = s_allRaceSkills.Concat(s_amarrRaceSkills).ToDictionary(x => x.Key, x => x.Value);
                    break;
                case Race.Caldari:
                    startingSkills = s_allRaceSkills.Concat(s_caldariRaceSkills).ToDictionary(x => x.Key, x => x.Value);
                    break;
                case Race.Gallente:
                    startingSkills = s_allRaceSkills.Concat(s_gallenteRaceSkills).ToDictionary(x => x.Key, x => x.Value);
                    break;
                case Race.Minmatar:
                    startingSkills = s_allRaceSkills.Concat(s_minmatarRaceSkills).ToDictionary(x => x.Key, x => x.Value);
                    break;
            }
            return startingSkills;
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Saves the blank character.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">callback</exception>
        public static async Task SaveAsync(Action callback)
        {
            callback.ThrowIfNull(nameof(callback));

            var serial = CreateCharacter();

            using (var fileDialog = new SaveFileDialog())
            {
                fileDialog.Title = @"Save Blank Character";
                fileDialog.Filter = @"Blank Character CCPXML (*.xml) | *.xml";
                fileDialog.FileName = $"{serial.Name}.xml";
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                var result = fileDialog.ShowDialog();
                if (result != DialogResult.OK)
                    return;

                var xmlDoc = (XmlDocument)Util.SerializeToXmlDocument(serial);
                var content = Util.GetXmlStringRepresentation(xmlDoc);
                await FileHelper.OverwriteOrWarnTheUserAsync(fileDialog.FileName,
                    async fs =>
                    {
                        using (var writer = new StreamWriter(fs, Encoding.UTF8))
                        {
                            await writer.WriteAsync(content);
                            await writer.FlushAsync();
                            await fs.FlushAsync();
                        }
                        return true;
                    });

                s_filename = fileDialog.FileName;
                callback.Invoke();
            }
        }

        /// <summary>
        /// Adds the blank character.
        /// </summary>
        public static async Task AddBlankCharacterAsync(Action callback)
        {
            // Add blank character
            var result = await GlobalCharacterCollection.TryAddOrUpdateFromUriAsync(new Uri(s_filename));

            if (result == null || result.HasError)
                return;

            var character = result.CreateCharacter();
            character.Monitored = true;

            callback.Invoke();
        }

        #endregion
    }
}
