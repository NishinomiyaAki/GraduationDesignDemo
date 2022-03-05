using EditorUI;
using System.Collections.Generic;

namespace CrossEditor
{
    internal class DockingManager
    {
        private static DockingManager _Instance = new DockingManager();
        private DockingBlock _DockingBlock;
        private List<DockingUI> _DockingUIs;
        private System.Windows.Forms.Control _Container;

        public DockingManager()
        {
            _DockingUIs = new List<DockingUI>();
        }

        public void Initialize(System.Windows.Forms.Control Control)
        {
            _Container = Control;
            _DockingBlock = DockingBlock.GetInstance();
            _DockingBlock.Initialize();
            _Container.Controls.Add(_DockingBlock);
        }

        public void AddUI(DockingUI DockingUI)
        {
            if (_DockingUIs.Contains(DockingUI))
            {
                _DockingBlock.ActivateCard(DockingUI._DockingCard);
                ChangePanel(DockingUI._Panel);
                return;
            }
            _DockingUIs.Add(DockingUI);
            _DockingBlock.AddCard(DockingUI._DockingCard);
            PanelLayout(DockingUI._Panel);
            _Container.Controls.Add(DockingUI._Panel);
            ChangePanel(DockingUI._Panel);
        }

        public void RemoveUI(DockingUI DockingUI)
        {
            if (_DockingUIs.Contains(DockingUI))
            {
                int Index = _DockingUIs.IndexOf(DockingUI);
                DockingUI.DoSave();
                _DockingUIs.Remove(DockingUI);
                _DockingBlock.RemoveCard(DockingUI._DockingCard);
                _Container.Controls.Remove(DockingUI._Panel);

                if (_DockingUIs.Count > Index)
                {
                    ChangePanel(_DockingUIs[Index]._Panel);
                }
                else if (_DockingUIs.Count > 0)
                {
                    ChangePanel(_DockingUIs[Index - 1]._Panel);
                }
            }
        }

        public void CloseAll()
        {
            for (int i = _DockingUIs.Count - 1; i >= 0; i--)
            {
                RemoveUI(_DockingUIs[i]);
            }
        }

        public void ChangePanel(Panel Panel)
        {
            // avoid flicker caused by auto refresh
            foreach (DockingUI DockingUI in _DockingUIs)
            {
                if (DockingUI._Panel == Panel)
                {
                    DockingUI._Panel.Visible = true;
                    
                }
            }

            foreach (DockingUI DockingUI in _DockingUIs)
            {
                if (DockingUI._Panel != Panel)
                {
                    DockingUI._Panel.Visible = false;
                }
            }
        }

        public static DockingManager GetInstance()
        {
            return _Instance;
        }

        public System.Windows.Forms.Control GetContainer()
        {
            return _Container;
        }

        public void PanelLayout(Panel Panel)
        {
            Panel.Size = new System.Drawing.Size(_Container.Width, _Container.Height - DockingCard.GetDefalutHeight());
            Panel.Location = new System.Drawing.Point(0, DockingCard.GetDefalutHeight());
            Panel.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_GRAY_DRAW_COLOR);
            Panel.Visible = false;
        }

        public void DoLayout()
        {
            _DockingBlock.Size = new System.Drawing.Size(_Container.Width, DockingCard.GetDefalutHeight());
            foreach(DockingUI UI in _DockingUIs)
            {
                UI._Panel.Size = new System.Drawing.Size(_Container.Width, _Container.Height - DockingCard.GetDefalutHeight());
            }
        }
    }
}