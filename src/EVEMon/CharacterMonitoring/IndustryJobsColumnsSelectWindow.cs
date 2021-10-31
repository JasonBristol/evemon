using System.Collections.Generic;
using System.Linq;
using EVEMon.Common.Controls;
using EVEMon.Common.Extensions;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.CharacterMonitoring
{
    public sealed class IndustryJobsColumnsSelectWindow : ColumnSelectWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndustryJobsColumnsSelectWindow"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public IndustryJobsColumnsSelectWindow(IEnumerable<IndustryJobColumnSettings> settings)
            : base(settings)
        {
        }

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected override string GetHeader(int key) => ((IndustryJobColumn)key).GetDescription();

        /// <summary>
        /// Gets all keys.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<int> AllKeys
            => EnumExtensions.GetValues<IndustryJobColumn>()
                .Where(x => x != IndustryJobColumn.None).Select(x => (int)x);

        /// <summary>
        /// Gets the default columns.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<IColumnSettings> DefaultColumns
        {
            get
            {
                var settings = new IndustryJobSettings();
                return settings.DefaultColumns;
            }
        }
    }
}