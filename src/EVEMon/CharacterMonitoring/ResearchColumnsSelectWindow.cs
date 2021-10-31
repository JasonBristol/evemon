using System.Collections.Generic;
using System.Linq;
using EVEMon.Common.Controls;
using EVEMon.Common.Extensions;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.CharacterMonitoring
{
    public sealed class ResearchColumnsSelectWindow : ColumnSelectWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchColumnsSelectWindow"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public ResearchColumnsSelectWindow(IEnumerable<ResearchColumnSettings> settings)
            : base(settings)
        {
        }

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected override string GetHeader(int key) => ((ResearchColumn)key).GetDescription();

        /// <summary>
        /// Gets all keys.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<int> AllKeys
            => EnumExtensions.GetValues<ResearchColumn>()
                .Where(x => x != ResearchColumn.None).Select(x => (int)x);

        /// <summary>
        /// Gets the default columns.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<IColumnSettings> DefaultColumns
        {
            get
            {
                var settings = new ResearchSettings();
                return settings.DefaultColumns;
            }
        }
    }
}