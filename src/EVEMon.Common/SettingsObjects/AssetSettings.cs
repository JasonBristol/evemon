using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using EVEMon.Common.Extensions;

namespace EVEMon.Common.SettingsObjects
{
    /// <summary>
    /// Settings for Assets.
    /// </summary>
    /// <remarks>
    /// This is the optimized way to implement the object as serializable and satisfy all FxCop rules.
    /// Don't use auto-property with private setter for the collections as it does not work with XmlSerializer.
    /// </remarks>
    public sealed class AssetSettings
    {
        private readonly Collection<AssetColumnSettings> m_columns;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetSettings"/> class.
        /// </summary>
        public AssetSettings()
        {
            m_columns = new Collection<AssetColumnSettings>();
        }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>The columns.</value>
        [XmlArray("columns")]
        [XmlArrayItem("column")]
        public Collection<AssetColumnSettings> Columns => m_columns;

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
        public IEnumerable<AssetColumnSettings> DefaultColumns
        {
            get
            {
                var defaultColumns = new[]
                                                   {
                                                       AssetColumn.ItemName,
                                                       AssetColumn.Quantity,
                                                       AssetColumn.Container, 
                                                       AssetColumn.Location,
                                                       AssetColumn.Jumps
                                                   };

                return EnumExtensions.GetValues<AssetColumn>().Where(
                    column => column != AssetColumn.None).Where(
                        column => Columns.All(columnSetting => columnSetting.Column != column)).Select(
                            column => new AssetColumnSettings
                                          {
                                              Column = column,
                                              Visible = defaultColumns.Contains(column),
                                              Width = -2
                                          });
            }
        }
    }
}