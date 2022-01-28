namespace ACViewer.Model
{
    public class InstanceBufferStats
    {
        public int TotalUninstancedVertices;
        public int TotalVerticesWithInstancing;
        public int TotalUniqueGfxObjIDs;
        public int TotalDrawCalls;

        public override string ToString()
        {
            return $"{TotalUninstancedVertices:N0} / {TotalVerticesWithInstancing:N0} / {TotalUniqueGfxObjIDs:N0} / {TotalDrawCalls:N0}";
        }
    }
}
