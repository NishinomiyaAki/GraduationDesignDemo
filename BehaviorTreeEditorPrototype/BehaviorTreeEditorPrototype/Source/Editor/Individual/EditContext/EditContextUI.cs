namespace CrossEditor
{
    internal class EditContextUI
    {
        private static EditContextUI _Instance = new EditContextUI();

        public static EditContextUI GetInstance()
        {
            return _Instance;
        }

        public void RegisterEdit(Edit Edit)
        {
            // TODO: register Edit
        }
    }
}