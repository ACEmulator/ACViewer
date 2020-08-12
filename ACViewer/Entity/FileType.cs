using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Entity
{
    public class FileType
    {
        public uint ID;
        public string Name;
        public string Description;
        public Type Type;

        public FileType(uint id, string name, Type t, string description = "")
        {
            ID = id;
            Name = name;
            Type = t;
            Description = description;
        }

        public override string ToString()
        {
            return $"0x{ID:X2} - {Name}";
        }
    }
}
