using System;

namespace ACViewer.Data
{
    public class TeleportRow
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }

        public TeleportLocationType LocationType { get; set; }

        public TeleportRow() { }

        public TeleportRow(string name, string type, string location)
        {
            Name = name;
            Type = type;
            Location = location;

            if (System.Enum.TryParse(type, out TeleportLocationType locationType))
                LocationType = locationType;
        }

        public bool Contains(string str)
        {
            return Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1
                || Type.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1
                || Location.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1;
        }
    }
}
