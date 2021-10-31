﻿using System.Collections.Generic;
using System.Linq;
using EVEMon.Common.Controls;
using EVEMon.Common.Extensions;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.CharacterMonitoring
{
    public partial class PlanetaryColumnsSelectWindow : ColumnSelectWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlanetaryColumnsSelectWindow"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public PlanetaryColumnsSelectWindow(IEnumerable<PlanetaryColumnSettings> settings)
            : base(settings)
        {
        }

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected override string GetHeader(int key) => ((PlanetaryColumn)key).GetDescription();

        /// <summary>
        /// Gets all keys.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<int> AllKeys
            => EnumExtensions.GetValues<PlanetaryColumn>()
                .Where(x => x != PlanetaryColumn.None).Select(x => (int)x);

        /// <summary>
        /// Gets the default columns.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<IColumnSettings> DefaultColumns
        {
            get
            {
                var settings = new PlanetarySettings();
                return settings.DefaultColumns;
            }
        }
    }
}
