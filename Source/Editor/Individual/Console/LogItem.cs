using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using EditorUI;

namespace CrossEditor
{
    class LogItem : Edit
    {
        LogMessageType _Type;
        string _Message;

        public LogItem(LogMessageType Type, string Message)
        {
            _Type = Type;
            _Message = Message;
        }

        public void DoLayout(ref int Y)
        {
            Location = new System.Drawing.Point(0, Y);
            Size = new System.Drawing.Size(Parent.Width, Edit.GetDefalutFontHeight());
            Y += this.Height + Edit._SpanY;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            System.Drawing.Bitmap Bitmap = new System.Drawing.Bitmap(this.Width, this.Height);
            System.Drawing.Graphics Graphics = System.Drawing.Graphics.FromImage(Bitmap);
            Graphics.Clear(BackColor);

            System.Drawing.Brush Brush = Graphics2D.GetInstance().CreateSolidBrushFromColor(Color.EDITOR_UI_GENERAL_TEXT_COLOR);
            Graphics.DrawString(GetStringContent(), GraphicsHelper.GetInstance().DefaultFont.GetFont(), Brush, _SpanX, _SpanY);

            e.Graphics.DrawImage(Bitmap, 0, 0);
            Graphics.Dispose();
            Bitmap.Dispose();
        }

        public string GetStringContent()
        {
            string TypeString = "";
            switch (_Type)
            {
                case LogMessageType.Debug:
                    TypeString = "Debug";
                    break;
                case LogMessageType.Error:
                    TypeString = "Error";
                    break;
                case LogMessageType.Information:
                    TypeString = "Info";
                    break;
                case LogMessageType.Warn:
                    TypeString = "Warn";
                    break;
            }
            return "[" + TypeString + "] - " + _Message;
        }
    }
}
