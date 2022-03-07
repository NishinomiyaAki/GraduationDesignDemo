namespace CrossEditor
{
    public class DockingUI
    {
        public DockingCard _DockingCard;
        public Panel _Panel;

        public void Initialize(string Text)
        {
            _DockingCard = new DockingCard();
            _DockingCard.Initialize(Text, _Panel);
        }

        public virtual void DoSave()
        {

        }
    }
}