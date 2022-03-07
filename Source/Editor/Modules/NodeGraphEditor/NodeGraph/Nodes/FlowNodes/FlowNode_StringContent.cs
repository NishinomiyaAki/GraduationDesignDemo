using EditorUI;

namespace CrossEditor
{
    public class FlowNode_StringContent : FlowNode
    {
        private const int SpanX = 3;
        private const int SpanY = 3;
        private const int OffsetY = 2;

        public virtual string GetStringContent()
        {
            return "TestTest";
        }

        public override void GetContentSize(ref int ContentWidth, ref int ContentHeight)
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();
            Font DefaultFont = GraphicsHelper.DefaultFont;

            string StringContent = GetStringContent();
            ContentWidth = SpanX + DefaultFont.MeasureString_Fast(StringContent) + SpanX;
            ContentHeight = SpanY + DefaultFont.GetCharHeight() + SpanY;
        }

        public override void DrawContent(int ContentX, int ContentY, int ContentWidth, int ContentHeight)
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            Color Color1 = Color.FromRGBA(100, 100, 100, 200);
            GraphicsHelper.FillRectangle(Color1, ContentX, ContentY, ContentWidth, ContentHeight);

            string StringContent = GetStringContent();
            Color Color2 = Color.FromRGBA(255, 255, 255, 255);
            GraphicsHelper.DrawString(GraphicsHelper.DefaultFont, StringContent, Color2, ContentX, ContentY + OffsetY, ContentWidth, ContentHeight, TextAlign.CenterCenter);
        }
    }
}