namespace CrossEditor
{
    internal class MainUI
    {
        private static MainUI _Instance = new MainUI();

        private BehaviorTreeEditorPrototype.MainWindow _MainWindow;
        private BehaviorTreeEditorPrototype.EditWindow _EditWindow;

        public BehaviorTreeEditorPrototype.MainWindow MainWindow
        {
            get
            {
                return _MainWindow;
            }
        }

        public BehaviorTreeEditorPrototype.EditWindow EditWindow
        {
            get
            {
                return _EditWindow;
            }
        }

        public static MainUI GetInstance()
        {
            return _Instance;
        }

        public void Initialize(BehaviorTreeEditorPrototype.MainWindow MainWindow, BehaviorTreeEditorPrototype.EditWindow EditWindow)
        {
            _MainWindow = MainWindow;
            _EditWindow = EditWindow;
        }

        public void ActivateDockingCard_Console()
        {
        }
    }
}