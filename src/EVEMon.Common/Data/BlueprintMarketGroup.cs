using System;
using System.Collections.Generic;
using System.Linq;
using EVEMon.Common.Extensions;
using EVEMon.Common.Serialization.Datafiles;

namespace EVEMon.Common.Data
{
    public sealed class BlueprintMarketGroup : MarketGroup
    {
        #region Constructors

        /// <summary>
        /// Deserialization constructor for root category only.
        /// </summary>
        /// <param name="src"></param>
        /// <exception cref="System.ArgumentNullException">src</exception>
        public BlueprintMarketGroup(SerializableBlueprintMarketGroup src)
            : base(src)
        {
            src.ThrowIfNull(nameof(src));

            SubGroups = new BlueprintMarketGroupCollection(this, src.SubGroups);
            Blueprints = new BlueprintCollection(this, src.Blueprints);
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="src">The source.</param>
        /// <exception cref="System.ArgumentNullException">src</exception>
        public BlueprintMarketGroup(MarketGroup parent, SerializableBlueprintMarketGroup src)
            : base(parent, src)
        {
            src.ThrowIfNull(nameof(src));

            SubGroups = new BlueprintMarketGroupCollection(this, src.SubGroups);
            Blueprints = new BlueprintCollection(this, src.Blueprints);
        }

        #endregion


        #region Public Properties

        /// <summary>
        /// Gets the sub categories.
        /// </summary>
        public new BlueprintMarketGroupCollection SubGroups { get; }

        /// <summary>
        /// Gets the blueprints in this category.
        /// </summary>
        public BlueprintCollection Blueprints { get; }

        /// <summary>
        /// Gets the collection of all the blueprints in this category and its descendants.
        /// </summary>
        public IEnumerable<Blueprint> AllBlueprints
        {
            get
            {
                foreach (var blueprint in Blueprints)
                {
                    yield return blueprint;
                }

                foreach (var subBlueprint in SubGroups.SelectMany(cat => cat.AllBlueprints))
                {
                    yield return subBlueprint;
                }
            }
        }

        #endregion
    }
}