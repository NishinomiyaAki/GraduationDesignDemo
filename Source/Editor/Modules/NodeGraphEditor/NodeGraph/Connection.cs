using EditorUI;
using Newtonsoft.Json;

namespace CrossEditor
{
    public class Connection
    {
        const int SegmentDraw = 50;

        public int ID;

        public Slot OutSlot;
        public Slot InSlot;

        public bool bDebugged;

        public Vector2f TempEnd;

        public CubicBazier CubicBazier;

        float _LineWidth = 2.0f;

        public Connection()
        {
            ID = 0;
            bDebugged = false;
            TempEnd = new Vector2f();
            CubicBazier = new CubicBazier();
        }

        public void BindOutSlot(Slot OutSlot)
        {
            if (OutSlot != null)
            {
                OutSlot.AddConnection(this);
            }
            this.OutSlot = OutSlot;
        }

        public void BindInSlot(Slot InSlot)
        {
            if (InSlot != null)
            {
                InSlot.AddConnection(this);
            }
            this.InSlot = InSlot;
        }

        SlotType GetSlotType()
        {
            if (InSlot != null)
            {
                return InSlot.SlotType;
            }
            else if (OutSlot != null)
            {
                return OutSlot.SlotType;
            }
            return SlotType.DataFlow;
        }

        public void SetDebugStatus(float weight)
        {
            bDebugged = true;
            _LineWidth = 2.0f + weight * 5 - 1;
        }

        public void ResetDebugStatus()
        {
            bDebugged = false;
            _LineWidth = 2.0f;
        }

        public virtual void DoLayout()
        {
            Vector2f Point1;
            if (OutSlot != null)
            {
                Point1 = OutSlot.GetSlotPosition();
            }
            else
            {
                Point1 = TempEnd;
            }
            Vector2f Point4;
            if (InSlot != null)
            {
                Point4 = InSlot.GetSlotPosition();
            }
            else
            {
                Point4 = TempEnd;
            }
            float DeltaX = Point4.X - Point1.X;
            if (DeltaX < 200)
            {
                DeltaX = 200;
            }
            DeltaX *= 0.5f;
            Vector2f Point2 = new Vector2f(Point1.X + DeltaX, Point1.Y);
            Vector2f Point3 = new Vector2f(Point4.X - DeltaX, Point4.Y);
            CubicBazier.Initialize(Point1, Point2, Point3, Point4);
        }

        public virtual void Draw()
        {
            ResetDebugStatus();

            GraphCamera2D Camera = GraphicsHelper.GetInstance().GetCamera();
            float X1, Y1, X2, Y2;
            CubicBazier.GetBoundingBox(out X1, out Y1, out X2, out Y2);
            if (Camera != null)
            {
                if (UIManager.RectInRect(
                (int)Camera.WorldX, (int)Camera.WorldY, (int)(Camera.WorldHeight * Camera.AspectRatio), (int)Camera.WorldHeight,
                (int)X1, (int)Y1, (int)(X2 - X1), (int)(Y2 - Y1)) == false)
                {
                    return;
                }
            }

            Color Color = SlotColor.GetColor(GetSlotType());
            Color = bDebugged ? Color.EDITOR_UI_COLOR_WHITE : Color;
            int SegmentCount = SegmentDraw;
            if (Camera != null)
            {
                SegmentCount = (int)(Camera.GetZoomRatio() * SegmentDraw);
            }
            CubicBazier.Draw(Color, SegmentCount, _LineWidth);
            
        }

        public virtual void CloneTo(ref Connection Target)
        {

        }
    }
}
