using System.Drawing;
using System.Windows.Forms;

namespace Editor
{
    public class TextAlign
    {
        private StringFormat _Format;
        private TextFormatFlags _FormatFlags;

        public TextAlign(StringAlignment VerticalAlignment, StringAlignment HorizonAlignment)
        {
            _Format = new StringFormat(StringFormatFlags.NoClip);
            _Format.LineAlignment = VerticalAlignment;
            _Format.Alignment = HorizonAlignment;

            _FormatFlags = 0;
            if(VerticalAlignment == StringAlignment.Center)
            {
                _FormatFlags |= TextFormatFlags.VerticalCenter;
            }
            if(HorizonAlignment == StringAlignment.Center)
            {
                _FormatFlags |= TextFormatFlags.HorizontalCenter;
            }
        }

        public StringFormat GetFormat()
        {
            return _Format;
        }

        public TextFormatFlags GetFormatFlags()
        {
            return _FormatFlags;
        }

        public static TextAlign CenterCenter
        {
            get
            {
                return new TextAlign(StringAlignment.Center, StringAlignment.Center);
            }
        }

        public static TextAlign CenterLeft
        {
            get
            {
                return new TextAlign(StringAlignment.Center, StringAlignment.Near);
            }
        }
    }
}