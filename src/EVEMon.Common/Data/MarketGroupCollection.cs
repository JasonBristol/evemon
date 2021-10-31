﻿using System.Collections.Generic;
using EVEMon.Common.Collections;
using EVEMon.Common.Serialization.Datafiles;

namespace EVEMon.Common.Data
{
    /// <summary>
    /// Represents a read-only collection of items.
    /// </summary>
    public sealed class MarketGroupCollection : ReadonlyCollection<MarketGroup>
    {
        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="src">The SRC.</param>
        internal MarketGroupCollection(MarketGroup group, ICollection<SerializableMarketGroup> src)
            : base(src?.Count ?? 0)
        {
            if (src == null)
                return;

            foreach (var subCat in src)
            {
                Items.Add(new MarketGroup(group, subCat));
            }
        }
    }
}