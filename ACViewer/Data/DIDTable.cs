using System;

namespace ACViewer.Data
{
    public class DIDTable: IEquatable<DIDTable>
    {
        public uint SetupID { get; set; }
        public uint MotionTableID { get; set; }
        public uint SoundTableID { get; set; }
        public uint CombatTableID { get; set; }

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
