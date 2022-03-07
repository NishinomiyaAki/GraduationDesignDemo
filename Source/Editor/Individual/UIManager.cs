namespace CrossEditor
{
    internal class UIManager
    {
        private static UIManager Instance = new UIManager();

        public const string DefaultFontFamily = "Arial";

        public static UIManager GetInstance()
        {
            return Instance;
        }

        public Font GetDefaultFont(int FontSize)
        {
            return new Font(DefaultFontFamily, FontSize);
        }

        public static bool PointInRect(int MouseX, int MouseY, int X, int Y, int W, int H)
        {
            if (MouseX > X && MouseX < (X + W) && MouseY > Y && MouseY < (Y + H))
            {
                return true;
            }
            return false;
        }

        public static object LoadUIImage(string ImagePath)
        {
            return null;
        }
    }
}