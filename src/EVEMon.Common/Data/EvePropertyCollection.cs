﻿using System.Collections.Generic;
using System.Linq;
using EVEMon.Common.Collections;
using EVEMon.Common.Serialization.Datafiles;

namespace EVEMon.Common.Data
{
    public sealed class EvePropertyCollection : ReadonlyCollection<EvePropertyValue>
    {
        #region Constructor

        /// <summary>
        /// Deserialization consructor.
        /// </summary>
        /// <param name="src"></param>
        internal EvePropertyCollection(ICollection<SerializablePropertyValue> src)
            : base(src?.Count ?? 0)
        {
            if (src == null)
                return;

            foreach (var prop in src.Select(
                srcProp => new EvePropertyValue(srcProp)).Where(prop => prop.Property != null))
            {
                Items.Add(prop);
            }
        }

        #endregion


        #region Indexers

        /// <summary>
        /// Gets a property from its id. If not found, return null.
        /// </summary>
        /// <param name="id">The property id we're searching for.</param>
        /// <returns>The wanted property when found; null otherwise.</returns>
        public EvePropertyValue? this[int id]
        {
            get
            {
                foreach (var prop in Items.TakeWhile(prop => prop.Property != null).Where(
                    prop => prop.Property.ID == id))
                {
                    return prop;
                }
                return null;
            }
        }

        #endregion
    }
}