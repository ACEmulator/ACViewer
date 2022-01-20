namespace ACViewer.Entity
{
    public class Frame
    {
        public ACE.DatLoader.Entity.Frame _frame;

        public Frame(ACE.DatLoader.Entity.Frame frame)
        {
            _frame = frame;
        }

        public override string ToString()
        {
            return $"{_frame.Origin} - {_frame.Orientation}";
        }
    }
}
