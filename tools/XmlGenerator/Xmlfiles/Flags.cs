using System;
using System.Diagnostics;
using System.Linq;
using EVEMon.Common.Collections;
using EVEMon.XmlGenerator.Providers;
using EVEMon.XmlGenerator.Utils;
using EVEMon.XmlGenerator.Xmlfiles.Serialization;

namespace EVEMon.XmlGenerator.Xmlfiles
{
    public static class Flags
    {
        internal static void GenerateXmlfile()
        {
            var stopwatch = Stopwatch.StartNew();

            Console.WriteLine();
            Console.Write(@"Generating Flags Xml file");

            var flags = new SerializableRoot<SerializableInvFlagsRow>
            {
                Rowset = new SerialiazableRowset<SerializableInvFlagsRow>
                {
                    Name = "flags",
                    Key = "flagID",
                    Columns = "flagID,flagName,flagText"
                }
            };

            flags.Rowset.Rows.AddRange(Database.InvFlagsTable.Select(
                flag => new SerializableInvFlagsRow
                {
                    ID = flag.ID,
                    Name = flag.Name,
                    Text = flag.Text
                }));

            Util.DisplayEndTime(stopwatch);
            Console.WriteLine();

            // Serialize
            Util.SerializeXmlTo(flags, "invFlags", "Flags.xml");
        }
    }
}