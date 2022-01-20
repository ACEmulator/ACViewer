namespace ACViewer.Entity
{
    public class CylSphere
    {
        public ACE.DatLoader.Entity.CylSphere _cylSphere;

        public CylSphere(ACE.DatLoader.Entity.CylSphere cylSphere)
        {
            _cylSphere = cylSphere;
        }

        public override string ToString()
        {
            return $"Origin: {_cylSphere.Origin}, Radius: {_cylSphere.Radius}, Height: {_cylSphere.Height}";
        }
    }
}
