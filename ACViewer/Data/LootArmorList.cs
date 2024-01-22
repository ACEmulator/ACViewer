using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Data
{

    public static class LootArmorList
    {
        public static Dictionary<uint, LootItem> Loot{ get; set; }

        static LootArmorList()
        {
            Loot = new Dictionary<uint, LootItem>();
        }

        public static void Load()
        {
            var filename = @"Data\LootArmor.txt";

            var lines = File.ReadAllLines(filename);

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // comment
                if (line.StartsWith("#"))
                    continue;

                var pieces = line.Split(',');

                if (pieces.Length != 5)
                {
                    Console.WriteLine($"LootArmor.Load({filename}): line {i + 1} length {pieces.Length}");
                    continue;
                }

                var wcid = pieces[0].Length > 0 ? Convert.ToUInt32(pieces[0]) : 0;
                var name = pieces[1].Length > 0 ? pieces[1] : "";
                var clothingBase = pieces[2].Length > 0 ? pieces[2].ToUpper() : "";
                var palTemp = pieces[3].Length > 0 ? Convert.ToUInt32(pieces[3]) : 0;
                var shade = pieces[4].Length > 0 ? Convert.ToSingle(pieces[4]) : 0;

                var item = new LootItem(wcid);
                item.Name = name;
                item.ClothingBase = clothingBase;
                item.PaletteTemplate = palTemp;
                item.Shade = shade;

                Loot.Add(wcid, item);
            }
        }

        public static LootItem Get(uint wcid)
        {
            Loot.TryGetValue(wcid, out var lootItem);
            return lootItem;
        }
    }
}
