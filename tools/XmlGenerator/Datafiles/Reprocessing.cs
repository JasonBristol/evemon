﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EVEMon.Common.Collections;
using EVEMon.Common.Serialization.Datafiles;
using EVEMon.XmlGenerator.Providers;
using EVEMon.XmlGenerator.Utils;

namespace EVEMon.XmlGenerator.Datafiles
{
    internal static class Reprocessing
    {
        /// <summary>
        /// Generates the reprocessing datafile.
        /// </summary>
        internal static void GenerateDatafile()
        {
            var stopwatch = Stopwatch.StartNew();
            Util.ResetCounters();

            Console.WriteLine();
            Console.Write(@"Generating reprocessing datafile... ");

            var types = new List<SerializableItemMaterials>();

            foreach (var typeID in Database.InvTypesTable.Where(x => x.Generated).Select(x => x.ID))
            {
                Util.UpdatePercentDone(Database.ReprocessingTotalCount);

                var materials = Database.InvTypeMaterialsTable.Where(
                    x => x.ID == typeID).Select(
                        srcMaterial => new SerializableMaterialQuantity
                        {
                            ID = srcMaterial.MaterialTypeID,
                            Quantity = srcMaterial.Quantity
                        }).ToList();

                if (!materials.Any())
                    continue;

                var itemMaterials = new SerializableItemMaterials { ID = typeID };
                itemMaterials.Materials.AddRange(materials.OrderBy(x => x.ID));
                types.Add(itemMaterials);
            }

            // Serialize
            var datafile = new ReprocessingDatafile();
            datafile.Items.AddRange(types);

            Util.DisplayEndTime(stopwatch);

            Util.SerializeXml(datafile, DatafileConstants.ReprocessingDatafile);
        }
    }
}
