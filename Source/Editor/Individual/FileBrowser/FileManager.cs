using EditorUI;
using System;
using System.IO;

namespace Editor
{
    internal class FileManager
    {
        private static FileManager _Instance = new FileManager();
        private const int _IntervalX = 10;
        private const int _SpanX = 20;
        private const int _SpanY = 3;
        private string _RootPath;
        private Panel _DetailPanel;
        private Panel _IconPanel;
        private DirectoryInfo _CurrentPath;
        private FileItem _CurrentSelected;
        private FileIcon _IconSelected;
        private System.Windows.Forms.Panel _Container;

        public static FileManager GetInstance()
        {
            return _Instance;
        }

        public DirectoryInfo GetCurrentPath()
        {
            return _CurrentPath;
        }

        public void Initialize(System.Windows.Forms.Panel Container, string Path)
        {
            _Container = Container;
            _RootPath = Path;

            _DetailPanel = new Panel();
            _DetailPanel.Initialize();

            _IconPanel = new Panel();
            _IconPanel.Initialize();
            _IconPanel.MouseDown += OnIconPanelMouseDown;

            _Container.Controls.Add(_DetailPanel);
            _Container.Controls.Add(_IconPanel);

            AddDirectory(_CurrentPath, _DetailPanel);
            AddIcon(null);
            DoLayout();
        }

        public void RefreshCurrentDirectory()
        {
            _DetailPanel.Controls.Clear();
            AddDirectory(_CurrentPath, _DetailPanel);
            DoLayout();
        }

        protected void OnIconPanelMouseDown(object Sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ShowIconPanelContextMenu((Sender as Control).GetScreenX() + e.X, (Sender as Control).GetScreenY() + e.Y);
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _IconPanel.Controls.Clear();
                AddIcon(_CurrentPath);
                DoIconLayout();
            }
        }

        private void ShowIconPanelContextMenu(int MouseX, int MouseY)
        {
            Menu MenuContextMenu = new Menu();
            MenuContextMenu.Initialize();

            Menu Menu_New = new Menu();
            Menu_New.Initialize();
            MenuItem MenuItem_New = new MenuItem();
            MenuItem_New.SetText("New");
            MenuItem_New.SetMenu(Menu_New);

            MenuItem MenuItem_New_VS = new MenuItem();
            MenuItem_New_VS.SetText("Visual Script");
            MenuItem_New_VS.ClickedEvent += (MenuItem Sender) =>
            {
                FileIcon VSIcon = new FileIcon();
                VSIcon.Initialize(false, ".VS");
                VSIcon.MouseEnter += OnMouseEnter;
                VSIcon.MouseLeave += OnMouseLeave;
                VSIcon.MouseDown += OnIconMouseDown;
                VSIcon.MouseDoubleClick += OnIconDoubleClick;
                _IconPanel.Controls.Add(VSIcon);
                DoIconLayout();
            };
            Menu_New.AddMenuItem(MenuItem_New_VS);

            MenuItem MenuItem_New_BT = new MenuItem();
            MenuItem_New_BT.SetText("Behavior Tree");
            MenuItem_New_BT.ClickedEvent += (MenuItem Sender) =>
            {
                FileIcon BTIcon = new FileIcon();
                BTIcon.Initialize(false, ".BT");
                BTIcon.MouseEnter += OnMouseEnter;
                BTIcon.MouseLeave += OnMouseLeave;
                BTIcon.MouseDown += OnIconMouseDown;
                BTIcon.MouseDoubleClick += OnIconDoubleClick;
                _IconPanel.Controls.Add(BTIcon);
                DoIconLayout();
            };
            Menu_New.AddMenuItem(MenuItem_New_BT);

            MenuItem MenuItem_New_BB = new MenuItem();
            MenuItem_New_BB.SetText("Blackboard");
            MenuItem_New_BB.ClickedEvent += (MenuItem Sender) =>
            {
                FileIcon BBIcon = new FileIcon();
                BBIcon.Initialize(false, ".BB");
                BBIcon.MouseEnter += OnMouseEnter;
                BBIcon.MouseLeave += OnMouseLeave;
                BBIcon.MouseDown += OnIconMouseDown;
                BBIcon.MouseDoubleClick += OnIconDoubleClick;
                _IconPanel.Controls.Add(BBIcon);
                DoIconLayout();
            };
            Menu_New.AddMenuItem(MenuItem_New_BB);

            MenuItem MenuItem_New_Folder = new MenuItem();
            MenuItem_New_Folder.SetText("Folder");
            MenuItem_New_Folder.ClickedEvent += (MenuItem Sender) =>
            {
                FileIcon FolderIcon = new FileIcon();
                FolderIcon.Initialize(true, "");
                FolderIcon.MouseEnter += OnMouseEnter;
                FolderIcon.MouseLeave += OnMouseLeave;
                FolderIcon.MouseDown += OnIconMouseDown;
                FolderIcon.MouseDoubleClick += OnIconDoubleClick;
                _IconPanel.Controls.Add(FolderIcon);
                DoIconLayout();
            };
            Menu_New.AddMenuItem(MenuItem_New_Folder);

            MenuContextMenu.AddMenuItem(MenuItem_New);

            ContextMenu.GetInstance().SetForm(MainUI.GetInstance().MainWindow);
            ContextMenu.GetInstance().ShowMenu(MenuContextMenu, MouseX, MouseY);
        }

        private void ShowIconContextMenu(int MouseX, int MouseY)
        {
            Menu MenuContextMenu = new Menu();
            MenuContextMenu.Initialize();

            MenuItem MenuItem_Delete = new MenuItem();
            MenuItem_Delete.SetText("Delete");
            MenuItem_Delete.ClickedEvent += (MenuItem Sender) =>
            {
                if (_IconSelected.IsDirectory)
                {
                    DirectoryInfo SelectedInfo = _IconSelected.Info as DirectoryInfo;
                    SelectedInfo.Delete();
                    _IconPanel.Controls.Remove(_IconSelected);
                    RefreshCurrentDirectory();
                }
                else
                {
                    FileInfo SelectedInfo = _IconSelected.Info as FileInfo;
                    if (string.Compare(SelectedInfo.Extension, ".BB", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        BlackboardManager.GetInstance().UnregisterBlackboard(SelectedInfo);
                    }
                    else if (string.Compare(SelectedInfo.Extension, ".BT", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        BehaviorTreeManager.GetInstance().UnregisterBehaviorTree(SelectedInfo);
                    }
                    _IconPanel.Controls.Remove(_IconSelected);
                    _IconSelected.Dispose();
                    SelectedInfo.Delete();
                    RefreshCurrentDirectory();
                }
            };

            MenuContextMenu.AddMenuItem(MenuItem_Delete);

            ContextMenu.GetInstance().SetForm(MainUI.GetInstance().MainWindow);
            ContextMenu.GetInstance().ShowMenu(MenuContextMenu, MouseX, MouseY);
        }

        public void AddDirectory(DirectoryInfo Directory, Panel Container)
        {
            if (Directory == null)
            {
                Directory = new DirectoryInfo(_RootPath);
            }

            FileItem CurrentDirectory = new FileItem();
            CurrentDirectory.Initialize(true);
            CurrentDirectory.SetInfo(Directory);
            CurrentDirectory.SetText(Directory.Name + "\\");
            CurrentDirectory.MouseEnter += OnMouseEnter;
            CurrentDirectory.MouseLeave += OnMouseLeave;
            CurrentDirectory.MouseClick += OnMouseClick;
            CurrentDirectory.MouseDoubleClick += OnMouseDoubleClick;

            Panel CurrentPanel = new Panel();
            CurrentPanel.Initialize();
            CurrentPanel.Visible = false;

            Container.Controls.Add(CurrentDirectory);
            Container.Controls.Add(CurrentPanel);
            foreach (DirectoryInfo SubDirectory in Directory.GetDirectories())
            {
                AddDirectory(SubDirectory, CurrentPanel);
            }
            foreach (FileInfo SubFile in Directory.GetFiles())
            {
                AddFile(SubFile, CurrentPanel);
            }
        }

        public void AddFile(FileInfo File, Panel Container)
        {
            FileItem CurrentFile = new FileItem();
            CurrentFile.Initialize(false);
            CurrentFile.SetInfo(File);
            CurrentFile.SetText(File.Name);
            CurrentFile.MouseEnter += OnMouseEnter;
            CurrentFile.MouseLeave += OnMouseLeave;
            CurrentFile.MouseClick += OnMouseClick;
            Container.Controls.Add(CurrentFile);
        }

        public void AddIcon(DirectoryInfo Directory)
        {
            if (Directory != null)
            {
                foreach (DirectoryInfo SubDirectory in Directory.GetDirectories())
                {
                    FileIcon DirectoryIcon = new FileIcon();
                    DirectoryIcon.Initialize(true, SubDirectory);
                    DirectoryIcon.MouseEnter += OnMouseEnter;
                    DirectoryIcon.MouseLeave += OnMouseLeave;
                    DirectoryIcon.MouseDown += OnIconMouseDown;
                    DirectoryIcon.MouseDoubleClick += OnIconDoubleClick;
                    _IconPanel.Controls.Add(DirectoryIcon);
                }
                foreach (FileInfo SubFile in Directory.GetFiles())
                {
                    FileIcon FileIcon = new FileIcon();
                    FileIcon.Initialize(false, SubFile);
                    FileIcon.MouseEnter += OnMouseEnter;
                    FileIcon.MouseLeave += OnMouseLeave;
                    FileIcon.MouseDown += OnIconMouseDown;
                    FileIcon.MouseDoubleClick += OnIconDoubleClick;
                    _IconPanel.Controls.Add(FileIcon);
                }
            }
        }

        public void DoLayout()
        {
            _DetailPanel.Location = new System.Drawing.Point(0, 0);
            _DetailPanel.Size = new System.Drawing.Size(500, _DetailPanel.Parent.Height);
            _DetailPanel.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_HILIGHT_COLOR_GRAY);
            _DetailPanel.MouseWheel += OnMouseWheel;

            DoSubLayout(_DetailPanel);
            foreach (Control Child in _DetailPanel.Controls)
            {
                _DetailPanel.Width = Math.Max(Child.Width + Child.Bounds.X, _DetailPanel.Width);
            }
            //_DetailPanel.Height = Math.Max(_DetailPanel.Parent.Height, _DetailPanel.Height);

            _IconPanel.Location = new System.Drawing.Point(_DetailPanel.Width + _IntervalX, 0);
            _IconPanel.Size = new System.Drawing.Size(_IconPanel.Parent.Width - _DetailPanel.Width - _IntervalX, _IconPanel.Parent.Height);
            _IconPanel.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_HILIGHT_COLOR_GRAY);
            _IconPanel.MouseWheel += OnMouseWheel;

            DoIconLayout();
        }

        public void DoSubLayout(Panel Panel)
        {
            int Y = 0;
            foreach (Control SubControl in Panel.Controls)
            {
                if (SubControl is FileItem)
                {
                    FileItem SubEdit = SubControl as FileItem;
                    SubEdit.Location = new System.Drawing.Point(_SpanX, Y + _SpanY);
                    SubEdit.Size = new System.Drawing.Size(
                        Edit._SpanX + GraphicsHelper.GetInstance().DefaultFont.MeasureString_Fast(SubEdit.Text) + Edit._SpanX + _SpanX,
                        Edit._SpanY + GraphicsHelper.GetInstance().DefaultFont.GetCharHeight() + Edit._SpanY);
                    SubEdit.BackColor = System.Drawing.Color.Transparent;
                    if (SubEdit == _CurrentSelected)
                    {
                        SubEdit.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_ACTIVE_TOOL_OR_MENU);
                    }

                    Y += _SpanY + SubEdit.Height + _SpanY;
                }
                if (SubControl is Panel && SubControl.Visible)
                {
                    Panel SubPanel = SubControl as Panel;
                    SubPanel.Location = new System.Drawing.Point(_SpanX, Y);
                    DoSubLayout(SubPanel);
                    foreach (Control Child in SubPanel.Controls)
                    {
                        SubPanel.Width = Math.Max(SubPanel.Width, Child.Width);
                    }
                    Y += SubControl.Height;
                }
            }
            if (Panel != _DetailPanel)
            {
                Panel.Height = Y;
            }
            else
            {
                Panel.Height = Math.Max(_DetailPanel.Parent.Height, Y);
            }
        }

        public void DoIconLayout()
        {
            int MaxRowCount = _IconPanel.Width / FileIcon._Width;
            int MaxColoumnCount = _IconPanel.Controls.Count / MaxRowCount + ((_IconPanel.Controls.Count % MaxRowCount) == 0 ? 0 : 1);
            foreach (FileIcon SubControl in _IconPanel.Controls)
            {
                int Index = _IconPanel.Controls.GetChildIndex(SubControl);
                SubControl.Location = new System.Drawing.Point((Index % MaxRowCount) * FileIcon._Width, (Index / MaxRowCount) * FileIcon._Height);
                if (SubControl == _IconSelected)
                {
                    SubControl.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_ACTIVE_TOOL_OR_MENU);
                }
                SubControl.DoLayout();
            }
            _IconPanel.Height = Math.Max(MaxColoumnCount * FileIcon._Width, _IconPanel.Height);
        }

        private void OnMouseWheel(object Sender, System.Windows.Forms.MouseEventArgs e)
        {
            Panel Self = Sender as Panel;
            if (e.Delta < 0 && Self.Location.Y >= (Self.Parent.Height - Self.Height))
            {
                int Y = Self.Location.Y - 20;
                Y = Y >= (Self.Parent.Height - Self.Height) ? Y : (Self.Parent.Height - Self.Height);
                Self.Location = new System.Drawing.Point(Self.Location.X, Y);
            }
            if (e.Delta > 0 && Self.Location.Y <= 0)
            {
                int Y = Self.Location.Y + 20;
                Y = Y <= 0 ? Y : 0;
                Self.Location = new System.Drawing.Point(Self.Location.X, Y);
            }
        }

        private void OnMouseEnter(object Sender, System.EventArgs e)
        {
            (Sender as Control).BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_ACTIVE_TOOL_OR_MENU);
        }

        private void OnMouseLeave(object Sender, System.EventArgs e)
        {
            if ((Sender is FileItem) && !(Sender as FileItem).IsSelected)
            {
                (Sender as FileItem).BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_HILIGHT_COLOR_GRAY);
            }
            if ((Sender is FileIcon) && !(Sender as FileIcon).IsSelected)
            {
                (Sender as FileIcon).BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_HILIGHT_COLOR_GRAY);
            }
        }

        private void OnMouseClick(object Sender, System.EventArgs e)
        {
            if (_CurrentSelected != null)
            {
                _CurrentSelected.IsSelected = false;
                _CurrentSelected.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_HILIGHT_COLOR_GRAY);
            }
            _CurrentSelected = (Sender as FileItem);
            _CurrentSelected.IsSelected = true;
            _CurrentSelected.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_ACTIVE_TOOL_OR_MENU);

            if (_CurrentSelected.IsDirectory)
            {
                _CurrentPath = _CurrentSelected.Info as DirectoryInfo;
            }
            else
            {
                _CurrentPath = (_CurrentSelected.Info as FileInfo).Directory;
            }

            _IconPanel.Controls.Clear();
            AddIcon(_CurrentPath);
            DoIconLayout();

            if (_IconSelected != null)
            {
                _IconSelected.IsSelected = false;
                _IconSelected.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_HILIGHT_COLOR_GRAY);
            }
            _IconSelected = MatchIcon(_CurrentSelected.Info);
            if (_IconSelected != null)
            {
                _IconSelected.IsSelected = true;
                _IconSelected.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_ACTIVE_TOOL_OR_MENU);
            }
        }

        private FileIcon MatchIcon(object Info)
        {
            foreach (FileIcon FileIcon in _IconPanel.Controls)
            {
                if (FileIcon.FullName == (Info as DirectoryInfo)?.FullName)
                {
                    return FileIcon;
                }
                if (FileIcon.FullName == (Info as FileInfo)?.FullName)
                {
                    return FileIcon;
                }
            }
            return null;
        }

        private void OnMouseDoubleClick(object Sender, EventArgs e)
        {
            FileItem Self = Sender as FileItem;
            int Index = Self.Parent.Controls.GetChildIndex(Self);
            Self.Parent.Controls[Index + 1].Visible = !Self.Parent.Controls[Index + 1].Visible;
            DoLayout();
        }

        private void OnIconMouseDown(object Sender, System.Windows.Forms.MouseEventArgs e)
        {
            FileIcon Self = Sender as FileIcon;
            if (_IconSelected != null)
            {
                _IconSelected.IsSelected = false;
                _IconSelected.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_HILIGHT_COLOR_GRAY);
            }
            _IconSelected = Self;
            _IconSelected.IsSelected = true;
            _IconSelected.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_ACTIVE_TOOL_OR_MENU);

            if (_CurrentSelected != null)
            {
                _CurrentSelected.IsSelected = false;
                _CurrentSelected.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_HILIGHT_COLOR_GRAY);
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ShowIconContextMenu(Self.GetScreenX() + e.X, Self.GetScreenY() + e.Y);
            }
        }

        private void OnIconDoubleClick(object Sender, EventArgs e)
        {
            FileIcon Self = Sender as FileIcon;
            if (Self.IsDirectory)
            {
                _CurrentPath = Self.Info as DirectoryInfo;
                _IconPanel.Controls.Clear();
                AddIcon(_CurrentPath);
                DoIconLayout();
            }
            else
            {
                FileInfo Info = (Self.Info as FileInfo);
                if (Info != null && string.Compare(Info.Extension, ".VS", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    MainUI.GetInstance().MainWindow.OpenNodeGraph(Info.FullName);
                }
                else if (Info != null && string.Compare(Info.Extension, ".BT", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    MainUI.GetInstance().MainWindow.OpenBehaviorTree(Info.FullName);
                }
                else if (Info != null && string.Compare(Info.Extension, ".BB", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    MainUI.GetInstance().MainWindow.OpenBlackboard(Info.FullName);
                }
            }
        }
    }
}