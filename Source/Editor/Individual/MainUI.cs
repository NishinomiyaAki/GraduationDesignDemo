namespace Editor
{
    internal class MainUI
    {
        private static MainUI _Instance = new MainUI();

        private GraduationDesignDemo.MainWindow _MainWindow;

        public GraduationDesignDemo.MainWindow MainWindow
        {
            get
            {
                return _MainWindow;
            }
        }

        public static MainUI GetInstance()
        {
            return _Instance;
        }

        public void Initialize(GraduationDesignDemo.MainWindow MainWindow)
        {
            _MainWindow = MainWindow;
        }

        public void ActivateDockingCard_Console()
        {
        }
    }
}