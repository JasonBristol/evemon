using System.Collections.Generic;
using System.Linq;
using EVEMon.Common.Collections.Global;
using EVEMon.Common.Serialization.Datafiles;

namespace EVEMon.Common.Data
{
    /// <summary>
    /// Stores all the data regarding reprocessing.
    /// </summary>
    public static class StaticReprocessing
    {
        private static readonly Dictionary<int, MaterialCollection> s_itemMaterialsByID = new Dictionary<int, MaterialCollection>();

        /// <summary>
        /// Initialize static reprocssing information.
        /// </summary>
        internal static void Load()
        {
            var datafile = Util.DeserializeDatafile<ReprocessingDatafile>(
                DatafileConstants.ReprocessingDatafile, Util.LoadXslt(Properties.Resources.DatafilesXSLT));

            foreach (var item in datafile.Items)
            {
                var materials = new MaterialCollection(item.Materials.Select(itemMaterial => new Material(itemMaterial)).ToList());
                s_itemMaterialsByID[item.ID] = materials;
            }

            GlobalDatafileCollection.OnDatafileLoaded();
        }

        /// <summary>
        /// Gets an enumeration of all the reprocessing materials.
        /// </summary>
        public static IEnumerable<MaterialCollection> AllReprocessingMaterials => s_itemMaterialsByID.Values;

        /// <summary>
        /// Gets the materials for the provided itemID.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static IEnumerable<Material> GetItemMaterialsByID(int id)
        {
            MaterialCollection result;
            s_itemMaterialsByID.TryGetValue(id, out result);
            return result;
        }
    }
}