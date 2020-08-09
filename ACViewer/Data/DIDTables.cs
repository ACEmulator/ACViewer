using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Data
{
    public static class DIDTables
    {
        public static Dictionary<uint, DIDTable> Setups;

        static DIDTables()
        {
            Setups = new Dictionary<uint, DIDTable>();
        }

        public static void Load()
        {
            var filename = @"Data\DIDTables.txt";

            var lines = File.ReadAllLines(filename);

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // comment
                if (line.StartsWith("#"))
                    continue;

                var pieces = line.Split(',');

                if (pieces.Length != 4)
                {
                    Console.WriteLine($"DIDTables.Load({filename}): line {i + 1} length {pieces.Length}");
                    continue;
                }

                var setupID = pieces[0].Length > 0 ? Convert.ToUInt32(pieces[0], 16) : 0;
                var mtableID = pieces[1].Length > 0 ? Convert.ToUInt32(pieces[1], 16) : 0;
                var stableID = pieces[2].Length > 0 ? Convert.ToUInt32(pieces[2], 16) : 0;
                var ctableID = pieces[3].Length > 0 ? Convert.ToUInt32(pieces[3], 16) : 0;

                var table = new DIDTable(setupID);
                table.MotionTableID = mtableID;
                table.SoundTableID = stableID;
                table.CombatTableID = ctableID;

                Setups.Add(setupID, table);
            }
        }

        public static DIDTable Get(uint setupID)
        {
            Setups.TryGetValue(setupID, out var setup);
            return setup;
        }
    }
}
