﻿using System.Collections.Generic;
using EVEMon.Common.Collections;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Serialization.Datafiles;

namespace EVEMon.Common.Data
{
    /// <summary>
    /// Represents a read-only collection of items.
    /// </summary>
    public sealed class ItemCollection : ReadonlyCollection<Item>
    {
        #region Constructor

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="group">Market Group for the item</param>
        /// <param name="src">One or more source serializable items</param>
        internal ItemCollection(MarketGroup group, ICollection<SerializableItem> src)
            : base(src?.Count ?? 0)
        {
            if (src == null)
                return;

            foreach (var item in src)
            {
                switch (item.Family)
                {
                    default:
                        Items.Add(new Item(group, item));
                        break;
                    case ItemFamily.Implant:
                        Items.Add(new Implant(group, item));
                        break;
                    case ItemFamily.Ship:
                        Items.Add(new Ship(group, item));
                        break;
                }
            }
        }

        #endregion
    }
}