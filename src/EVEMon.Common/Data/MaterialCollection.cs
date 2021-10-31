using System.Collections.Generic;
using EVEMon.Common.Collections;

namespace EVEMon.Common.Data
{
    public sealed class MaterialCollection : ReadonlyCollection<Material>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialCollection"/> class.
        /// </summary>
        /// <param name="materials">The materials.</param>
        internal MaterialCollection(ICollection<Material> materials)
            : base(materials?.Count ?? 0)
        {
            if (materials == null)
                return;

            foreach (var material in materials)
            {
                Items.Add(material);
            }
        }
    }
}