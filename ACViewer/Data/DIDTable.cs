using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACViewer.Data
{
    public class DIDTable: IEquatable<DIDTable>
    {
        public uint SetupID;
        public uint MotionTableID;
        public uint SoundTableID;
        public uint CombatTableID;

        public DIDTable() { }

        public DIDTable(uint setupID)
        {
            SetupID = setupID;
        }

        public bool Equals(DIDTable table)
        {
            return SetupID.Equals(table.SetupID);
        }

        public override int GetHashCode()
        {
            return SetupID.GetHashCode();
        }
    }
}
