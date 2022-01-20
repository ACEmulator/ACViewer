using System.Collections.Generic;

namespace ACViewer
{
    /// <summary>
    /// DID history
    /// </summary>
    public class History
    {
        private static readonly int MaxSize = 50;
        
        private readonly List<uint> DID = new List<uint>();

        public void Add(uint did)
        {
            // don't add consecutive duplicates
            if (DID.Count > 0 && DID[DID.Count - 1] == did)
                return;
            
            DID.Add(did);

            if (DID.Count > MaxSize)
                DID.RemoveAt(0);
        }

        public void Clear()
        {
            DID.Clear();
        }

        public uint? Pop()
        {
            if (DID.Count <= 1) return null;

            DID.RemoveAt(DID.Count - 1);

            return DID[DID.Count - 1];
        }
    }
}
