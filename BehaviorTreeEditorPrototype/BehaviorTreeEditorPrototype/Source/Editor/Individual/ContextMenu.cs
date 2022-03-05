namespace CrossEditor
{
    internal class ContextMenu
    {
        private static ContextMenu _Instance = new ContextMenu();
        private Menu _Menu;
        private Menu _SubMenu;
        private System.Windows.Forms.Control _Form = null;
        private int _SavedMouseX;
        private int _SavedMouseY;

        public Menu CurrentMenu
        {
            get
            {
                return _Menu;
            }
        }

        public void SetForm(System.Windows.Forms.Control Form)
        {
            _Form = Form;
        }

        public static ContextMenu GetInstance()
        {
            return _Instance;
        }

        public void ShowMenu(Menu Menu, int MouseX, int MouseY)
        {
            if (_Form == null)
            {
                return;
            }

            if (_Menu != null)
            {
                HideMenu();
            }
            _SavedMouseX = MouseX;
            _SavedMouseY = MouseY;

            _Menu = Menu;
            _Menu.Location = new System.Drawing.Point(MouseX, MouseY);
            _Menu.DoLayout();

            _Form.Controls.Add(_Menu);
            _Menu.BringToFront();
            _Form.Refresh();
        }

        public void ShowSubMenu(Menu SubMenu, int X, int Y)
        {
            if (_SubMenu == null)
            {
                _SubMenu = SubMenu;
            }
            else
            {
                _Form.Controls.Remove(_SubMenu);
                _SubMenu = SubMenu;
            }
            _SubMenu.Location = new System.Drawing.Point(_SavedMouseX + X, _SavedMouseY + Y);
            _Form.Controls.Add(_SubMenu);
            _SubMenu.BringToFront();
            _Form.Refresh();
        }

        public void HideSubMenu(MenuItem MenuItem)
        {
            if (_SubMenu != null && !_SubMenu.Controls.Contains(MenuItem))
            {
                _Form.Controls.Remove(_SubMenu);
                _SubMenu = null;
            }
            _Form.Refresh();
        }

        public void HideMenu()
        {
            if (_Form == null)
            {
                return;
            }

            if (_Menu != null)
            {
                _Form.Controls.Remove(_Menu);
                _Menu = null;
            }
            if (_SubMenu != null)
            {
                _Form.Controls.Remove(_SubMenu);
                _SubMenu = null;
            }
            _Form.Refresh();
        }
    }
}