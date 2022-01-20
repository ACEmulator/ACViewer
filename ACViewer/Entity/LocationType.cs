namespace ACViewer.Entity
{
    public class LocationType
    {
        public ACE.DatLoader.Entity.LocationType _locationType;

        public LocationType(ACE.DatLoader.Entity.LocationType locationType)
        {
            _locationType = locationType;
        }

        public override string ToString()
        {
            return $"PartID: {_locationType.PartId}, Frame: {new Frame(_locationType.Frame)}";
        }
    }
}
