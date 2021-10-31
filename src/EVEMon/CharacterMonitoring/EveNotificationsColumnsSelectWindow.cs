﻿using System.Collections.Generic;
using System.Linq;
using EVEMon.Common.Controls;
using EVEMon.Common.Extensions;
using EVEMon.Common.SettingsObjects;

namespace EVEMon.CharacterMonitoring
{
    public sealed class EveNotificationsColumnsSelectWindow : ColumnSelectWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EveNotificationsColumnsSelectWindow"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public EveNotificationsColumnsSelectWindow(IEnumerable<EveNotificationColumnSettings> settings)
            : base(settings)
        {
        }

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected override string GetHeader(int key) => ((EveNotificationColumn)key).GetDescription();

        /// <summary>
        /// Gets all keys.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<int> AllKeys
            => EnumExtensions.GetValues<EveNotificationColumn>()
                .Where(x => x != EveNotificationColumn.None).Select(x => (int)x);

        /// <summary>
        /// Gets the default columns.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<IColumnSettings> DefaultColumns
        {
            get
            {
                var settings = new EveNotificationSettings();
                return settings.DefaultColumns;
            }
        }
    }
}