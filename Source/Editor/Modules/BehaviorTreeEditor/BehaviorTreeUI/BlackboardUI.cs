using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace CrossEditor
{
    class BlackboardUI : DockingUI
    {
        private static BlackboardUI _Instance = new BlackboardUI();

        private Edit _Parent;
        private Panel _ParentEntriesPanel;

        private Edit _Current;
        private Panel _CurrentEntriesPanel;

        private string _FileName;
        private Blackboard _Blackboard;
        private object _SelectedObject;

        private bool _bModified;

        private int _SavedMouseX;
        private int _SavedMouseY;

        public static BlackboardUI GetInstance()
        {
            return _Instance;
        }

        public bool Initialize()
        {
            _FileName = "";
            _Blackboard = new Blackboard();
            _SelectedObject = null;
            _bModified = false;
            _SavedMouseX = 0;
            _SavedMouseY = 0;

            _Panel = new Panel();
            _Panel.Initialize();
            _Panel.VisibleChanged += OnPanelVisibleChanged;
            _Panel.MouseUp += OnPanelMouseUp;

            _Parent = new Edit();
            _Parent.SetText("Inherited Key");

            _ParentEntriesPanel = new Panel();
            _ParentEntriesPanel.Initialize();

            _Current = new Edit();
            _Current.SetText("Key");
            _Current.MouseUp += OnMouseUp;
            _Current.MouseDoubleClick += OnMouseDoubleClick;

            _CurrentEntriesPanel = new Panel();
            _CurrentEntriesPanel.Initialize();

            _Panel.Controls.Add(_Parent);
            _Panel.Controls.Add(_ParentEntriesPanel);
            _Panel.Controls.Add(_Current);
            _Panel.Controls.Add(_CurrentEntriesPanel);

            _Blackboard.SetPanel(_ParentEntriesPanel, _CurrentEntriesPanel);
            _Blackboard.EditMouseDown += OnEditMouseDown;
            _Blackboard.EditMouseEnter += OnEditMouseEnter;
            _Blackboard.EditMouseLeave += OnEditMouseLeave;
            _Blackboard.EditMouseUp += OnEditMouseUp;

            base.Initialize("Blackboard");

            return true;
        }


        private void OnEditMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Edit Self = sender as Edit;
            _SavedMouseX = e.X;
            _SavedMouseY = e.Y;
            ChangeSelectedObject(Self);
        }

        private void OnEditMouseEnter(object sender, EventArgs e)
        {
            Edit Self = sender as Edit;
            Self.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_MASK_COLOR);
        }

        private void OnEditMouseLeave(object sender, EventArgs e)
        {
            Edit Self = sender as Edit;
            if(Self == _SelectedObject)
            {
                Self.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_ACTIVE_TOOL_OR_MENU);
            }
            else
            {
                Self.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_GRAY_DRAW_COLOR);
            }
        }

        private void OnEditMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Edit Self = sender as Edit;

            if(e.Button == System.Windows.Forms.MouseButtons.Right &&
                e.X == _SavedMouseX && e.Y == _SavedMouseY)
            {
                showEditMenu(Self.GetScreenX() + e.X, Self.GetScreenY() + e.Y);
            }
        }

        private void showEditMenu(int MouseX, int MouseY)
        {
            Menu MenuContextMenu = new Menu();
            MenuContextMenu.Initialize();

            MenuItem MenuItem_Delete = new MenuItem();
            MenuItem_Delete.SetText("Delete");
            MenuItem_Delete.ClickedEvent += (Sender) =>
            {
                DoDelete();
            };

            MenuContextMenu.AddMenuItem(MenuItem_Delete);

            ContextMenu.GetInstance().SetForm(MainUI.GetInstance().EditWindow);
            ContextMenu.GetInstance().ShowMenu(MenuContextMenu, MouseX, MouseY);
        }

        private void ChangeSelectedObject(object Object)
        {
            if(_SelectedObject != null)
            {
                (_SelectedObject as Edit).BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_GRAY_DRAW_COLOR);
            }
            _SelectedObject = Object;
            (_SelectedObject as Edit).BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_ACTIVE_TOOL_OR_MENU);
            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(_Blackboard.GetEntry((_SelectedObject as Edit).Text));
            InspectorUI.InspectObject();
        }

        private void OnPanelMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Panel Self = sender as Panel;
            if(e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ShowPanelMenu(Self.GetScreenX() + e.X, Self.GetScreenY() + e.Y);
            }
        }

        private void ShowPanelMenu(int MouseX, int MouseY)
        {
            Menu MenuContextMenu = new Menu();
            MenuContextMenu.Initialize();

            MenuItem MenuItem_Save = new MenuItem();
            MenuItem_Save.SetText("Save");
            MenuItem_Save.ClickedEvent += (Sender) =>
            {
                DoSave();
            };

            MenuContextMenu.AddMenuItem(MenuItem_Save);

            ContextMenu.GetInstance().SetForm(MainUI.GetInstance().EditWindow);
            ContextMenu.GetInstance().ShowMenu(MenuContextMenu, MouseX, MouseY);
        }

        private void OnMouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Edit Self = sender as Edit;
            if(e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if(Self == _Current)
                {
                    _CurrentEntriesPanel.Visible = !_CurrentEntriesPanel.Visible;
                }
                else if(Self == _Parent)
                {
                    _ParentEntriesPanel.Visible = !_ParentEntriesPanel.Visible;
                }
            }
        }

        private void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Edit Self = sender as Edit;
            if(e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ShowContextMenu(Self.GetScreenX() + e.X, Self.GetScreenY() + e.Y);
            }
        }

        private void ShowContextMenu(int MouseX, int MouseY)
        {
            Menu MenuContextMenu = new Menu();
            MenuContextMenu.Initialize();

            #region New

            Menu Menu_New = new Menu();
            Menu_New.Initialize();

            MenuItem MenuItem_New_Bool = new MenuItem();
            MenuItem_New_Bool.SetText("Key Type Bool");
            MenuItem_New_Bool.ClickedEvent += (Sender) =>
            {
                AddEntry(new BlackboardKeyType_Bool());
            };
            Menu_New.AddMenuItem(MenuItem_New_Bool);

            MenuItem MenuItem_New_Float = new MenuItem();
            MenuItem_New_Float.SetText("Key Type Float");
            MenuItem_New_Float.ClickedEvent += (Sender) =>
            {
                AddEntry(new BlackboardKeyType_Float());
            };
            Menu_New.AddMenuItem(MenuItem_New_Float);

            MenuItem MenuItem_New_Int = new MenuItem();
            MenuItem_New_Int.SetText("Key Type Int");
            MenuItem_New_Int.ClickedEvent += (Sender) =>
            {
                AddEntry(new BlackboardKeyType_Int());
            };
            Menu_New.AddMenuItem(MenuItem_New_Int);

            MenuItem MenuItem_New_Vector = new MenuItem();
            MenuItem_New_Vector.SetText("Key Type Vector");
            MenuItem_New_Vector.ClickedEvent += (Sender) =>
            {
                AddEntry(new BlackboardKeyType_Vector());
            };
            Menu_New.AddMenuItem(MenuItem_New_Vector);

            #endregion

            MenuItem Menuitem_New = new MenuItem();
            Menuitem_New.SetText("New");
            Menuitem_New.SetMenu(Menu_New);

            MenuContextMenu.AddMenuItem(Menuitem_New);
            ContextMenu.GetInstance().SetForm(MainUI.GetInstance().EditWindow);
            ContextMenu.GetInstance().ShowMenu(MenuContextMenu, MouseX, MouseY);
        }

        public void AddEntry(BlackboardKeyType KeyType)
        {
            string EntryName = _Blackboard.GenerateEntryName(KeyType);
            BlackboardEntry Entry = new BlackboardEntry(EntryName, KeyType, false);
            _Blackboard.AddEntry(Entry);
            DoLayout();

            SetModified();
        }

        private void OnPanelVisibleChanged(object Sender, EventArgs e)
        {
            Panel Self = Sender as Panel;
            if(Self.Visible)
            {
                DoLayout();
            }
        }

        public void SetModified()
        {
            _bModified = true;
            UpdateDockingCardText();
        }

        public void ClearModified()
        {
            _bModified = false;
            UpdateDockingCardText();
        }

        public void UpdateDockingCardText()
        {
            if (_bModified)
            {
                _DockingCard.SetText("Blackboard*");
            }
            else
            {
                _DockingCard.SetText("Blackboard");
            }
        }

        public override void DoSave()
        {
            if(_FileName != "")
            {
                _Blackboard.SaveToXml(_FileName);
            }

            ClearModified();
        }

        public void DoDelete()
        {
            _Blackboard.RemoveEntry((_SelectedObject as Edit).Text);
            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(null);
            InspectorUI.InspectObject();
            DoLayout();

            SetModified();
        }

        public void OpenBlackboard(string FileName)
        {
            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(null);
            InspectorUI.InspectObject();
            if (_FileName != "")
            {
                DoSave();
            }
            _FileName = FileName;
            _Blackboard.LoadFromXml(FileName);
            DoLayout();
            ClearModified();
        }

        public void DoLayout()
        {
            int Y = 0;

            _Parent.Location = new System.Drawing.Point(0, Y);
            _Parent.Width = _Panel.Width;
            _Parent.Height = Edit.GetDefalutFontHeight();
            _Parent.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_GRAY_DRAW_COLOR);
            Y += _Parent.Height;

            _ParentEntriesPanel.Location = new System.Drawing.Point(0, Y);
            _ParentEntriesPanel.Width = _Panel.Width;
            _ParentEntriesPanel.Height = _Blackboard.GetParentEntriesCount() * Edit.GetDefalutFontHeight();
            Y += _ParentEntriesPanel.Height;

            _Current.Location = new System.Drawing.Point(0, Y);
            _Current.Width = _Panel.Width;
            _Current.Height = Edit.GetDefalutFontHeight();
            _Current.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_GRAY_DRAW_COLOR);
            Y += _Current.Height;

            _CurrentEntriesPanel.Location = new System.Drawing.Point(0, Y);
            _CurrentEntriesPanel.Width = _Panel.Width;
            _CurrentEntriesPanel.Height = _Blackboard.GetEntriesCount() * Edit.GetDefalutFontHeight();
            Y += _CurrentEntriesPanel.Height;

            _Blackboard.DoLayout();
        }

        public bool CheckBlackboard(BlackboardData Blackboard)
        {
            return Blackboard == _Blackboard.BlackboardData;
        }
    }
}
