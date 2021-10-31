﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using EVEMon.Common.Extensions;

namespace EVEMon.Common.SettingsObjects
{
    /// <summary>
    /// Settings for Market Orders.
    /// </summary>
    /// <remarks>
    /// This is the optimized way to implement the object as serializable and satisfy all FxCop rules.
    /// Don't use auto-property with private setter for the collections as it does not work with XmlSerializer.
    /// </remarks>
    public sealed class MarketOrderSettings
    {
        private readonly Collection<MarketOrderColumnSettings> m_columns;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketOrderSettings"/> class.
        /// </summary>
        public MarketOrderSettings()
        {
            m_columns = new Collection<MarketOrderColumnSettings>();

            HideInactiveOrders = true;
        }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>The columns.</value>
        [XmlArray("columns")]
        [XmlArrayItem("column")]
        public Collection<MarketOrderColumnSettings> Columns => m_columns;

        /// <summary>
        /// Gets or sets a value indicating whether [hide inactive orders].
        /// </summary>
        /// <value><c>true</c> if [hide inactive orders]; otherwise, <c>false</c>.</value>
        [XmlElement("hideInactiveOrders")]
        public bool HideInactiveOrders { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [number abs format].
        /// </summary>
        /// <value><c>true</c> if [number abs format]; otherwise, <c>false</c>.</value>
        [XmlElement("numberAbsFormat")]
        public bool NumberAbsFormat { get; set; }

        /// <summary>
        /// Gets the default columns.
        /// </summary>
        /// <value>The default columns.</value>
        public IEnumerable<MarketOrderColumnSettings> DefaultColumns
        {
            get
            {
                var defaultColumns = new[]
                                                         {
                                                             MarketOrderColumn.Item,
                                                             MarketOrderColumn.SolarSystem,
                                                             MarketOrderColumn.UnitaryPrice,
                                                             MarketOrderColumn.Volume
                                                         };

                return EnumExtensions.GetValues<MarketOrderColumn>().Where(
                    column => column != MarketOrderColumn.None).Where(
                        column => Columns.All(columnSetting => columnSetting.Column != column)).Select(
                            column => new MarketOrderColumnSettings
                                              {
                                                  Column = column,
                                                  Visible = defaultColumns.Contains(column),
                                                  Width = -2
                                              });
            }
        }
    }
}