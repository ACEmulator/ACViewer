using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Data
{
    public class LootItem : IEquatable<LootItem>
    {
        public uint WCID { get; set; }
        public string Name { get; set; }
        public string ClothingBase { get; set; }
        public uint PaletteTemplate { get; set; }
        public float Shade{ get; set; }

        public LootItem() { }

        public LootItem(uint wcid)
        {
            WCID = wcid;
        }

        public bool Equals(LootItem table)
        {
            return WCID.Equals(table.WCID);
        }

        public override int GetHashCode()
        {
            return WCID.GetHashCode();
        }
    }
}
