using System.Collections.Generic;

namespace CrossEditor
{
    internal class Menu : Control
    {
        private List<MenuItem> _MenuItems;

        public void Initialize()
        {
            _MenuItems = new List<MenuItem>();
        }

        public void AddMenuItem(MenuItem MenuItem)
        {
            _MenuItems.Add(MenuItem);
        }

        public void AddSeperator()
        {
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            System.Drawing.Bitmap Bitmap = new System.Drawing.Bitmap(this.Width, this.Height);
            System.Drawing.Graphics Graphics = System.Drawing.Graphics.FromImage(Bitmap);

            e.Graphics.DrawImage(Bitmap, 0, 0);
            Graphics.Dispose();
            Bitmap.Dispose();
            base.OnPaint(e);
        }

        public void DoLayout()
        {
            Width = 0;
            Height = 0;
            foreach (MenuItem MenuItem in _MenuItems)
            {
                MenuItem.Location = new System.Drawing.Point(0, Height);
                MenuItem.DoLayout();
                Height += MenuItem.Height;
                Width = System.Math.Max(MenuItem.Width, Width);
            }
            foreach (MenuItem MenuItem in _MenuItems)
            {
                MenuItem.Width = Width;
                Controls.Add(MenuItem);
            }
        }
    }
}