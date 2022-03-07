using EditorUI;
using System.Collections.Generic;

namespace Editor
{
    public class DockingBlock : Control
    {
        public static DockingBlock _Instance = new DockingBlock();
        private List<DockingCard> _DockingCards;
        private int _DefalutHeight;

        public DockingBlock()
        {
            _DockingCards = new List<DockingCard>();
        }

        public void Initialize()
        {
            _DefalutHeight = DockingCard.GetDefalutHeight();
        }

        protected override void OnControlAdded(System.Windows.Forms.ControlEventArgs e)
        {
            base.OnControlAdded(e);
            DoLayout();
        }

        protected override void OnControlRemoved(System.Windows.Forms.ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            DoLayout();
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            System.Drawing.Bitmap Bitmap = new System.Drawing.Bitmap(Width, Height);
            System.Drawing.Graphics Graphics = System.Drawing.Graphics.FromImage(Bitmap);
            Graphics.Clear(BackColor);

            System.Drawing.SolidBrush Brush = Graphics2D.GetInstance().CreateSolidBrushFromColor(Color.EDITOR_UI_BAR_COLOR);
            Graphics.FillRectangle(Brush, 0, 0, Width, Height);

            e.Graphics.DrawImage(Bitmap, 0, 0);
            Graphics.Dispose();
            Bitmap.Dispose();
            base.OnPaint(e);
        }

        public void DoLayout()
        {
            int X = 0;
            foreach(DockingCard Card in _DockingCards)
            {
                Card.DoLayout(ref X);
            }
            Width = Parent.Width;
            Height = _DefalutHeight;
        }

        public static DockingBlock GetInstance()
        {
            return _Instance;
        }

        public bool GetFocused()
        {
            return Focused;
        }

        public void AddCard(DockingCard Card)
        {
            _DockingCards.Add(Card);
            Controls.Add(Card);
            ActivateCard(Card);
        }

        public void RemoveCard(DockingCard Card)
        {
            if (_DockingCards.Contains(Card))
            {
                int Index = _DockingCards.IndexOf(Card);
                _DockingCards.Remove(Card);
                Controls.Remove(Card);

                if(_DockingCards.Count > Index)
                {
                    ActivateCard(_DockingCards[Index]);
                }
                else if(_DockingCards.Count > 0)
                {
                    ActivateCard(_DockingCards[Index - 1]);
                }
            }
        }

        public void ActivateCard(DockingCard Card)
        {
            foreach (DockingCard DockingCard in _DockingCards)
            {
                if (DockingCard == Card)
                {
                    DockingCard.Active = true;
                }
                else
                {
                    DockingCard.Active = false;
                }
            }
            this.Refresh();
        }
    }
}