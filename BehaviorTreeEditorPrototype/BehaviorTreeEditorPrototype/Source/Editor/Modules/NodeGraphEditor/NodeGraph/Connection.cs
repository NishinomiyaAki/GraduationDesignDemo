using EditorUI;
using System;

namespace CrossEditor
{
    [Serializable]
    public class Connection
    {
        private const int SegmentDraw = 20;

        public int ID;

        public Slot OutSlot;
        public Slot InSlot;
        public bool bSelected;

        public Vector2f TempEnd;

        public CubicBazier CubicBazier;

        public Connection()
        {
            ID = 0;
            bSelected = false;
            TempEnd = new Vector2f();
            CubicBazier = new CubicBazier();
        }

        public void SetOutSlot(Slot OutSlot)
        {
            if (OutSlot != null)
            {
                OutSlot.AddConnection(this);
            }
            this.OutSlot = OutSlot;
        }

        public void SetInSlot(Slot InSlot)
        {
            if (InSlot != null)
            {
                InSlot.AddConnection(this);
            }
            this.InSlot = InSlot;
        }

        private SlotType GetSlotType()
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

        public void DoLayout()
        {
            Vector2f Point1;
            if (OutSlot != null)
            {
                Point1 = OutSlot.GetOutputSlot();
            }
            else
            {
                Point1 = TempEnd;
            }
            Vector2f Point4;
            if (InSlot != null)
            {
                Point4 = InSlot.GetInputSlot();
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

        public void Draw()
        {
            Color Color;
            if (bSelected)
            {
                Color = Color.FromRGB(255, 204, 0);
            }
            else if (InSlot != null && OutSlot != null && 
                InSlot.Node is BTNode && OutSlot.Node is BTNode &&
                (InSlot.Node as BTNode)._bExecuting &&
                (OutSlot.Node as BTNode)._bExecuting)
            {
                Color = Color.FromRGB(0, 255, 0);
            }
            else
            {
                SlotType SlotType = GetSlotType();
                if (SlotType == SlotType.DataFlow)
                {
                    Color = Color.FromRGB(110, 220, 120);
                }
                else
                {
                    Color = Color.FromRGB(220, 70, 89);
                }
            }
            CubicBazier.Draw(Color, SegmentDraw);
        }

        public object HitTest(int MouseX, int MouseY)
        {
            float X1;
            float Y1;
            float X2;
            float Y2;
            CubicBazier.GetBoundingBox(out X1, out Y1, out X2, out Y2);

            int X = (int)X1;
            int Y = (int)Y1;
            int W = (int)(X2 - X1);
            int H = (int)(Y2 - Y1);
            if (UIManager.PointInRect(MouseX, MouseY, X, Y, W, H) == false)
            {
                return null;
            }

            int SegmentHitTest = (W + H) * 2;
            float Radius = 3.0f;
            if (CubicBazier.HitTest(new Vector2f(MouseX, MouseY), SegmentHitTest, Radius))
            {
                return this;
            }
            return null;
        }

        public void SaveToXml(Record RecordConnection)
        {
            RecordConnection.SetTypeString("Connection");
            RecordConnection.SetInt("ID", ID);
            RecordConnection.SetInt("OutSlotNodeID", OutSlot.Node.ID);
            RecordConnection.SetString("OutSlotName", OutSlot.Name);
            RecordConnection.SetInt("InSlotNodeID", InSlot.Node.ID);
            RecordConnection.SetString("InSlotName", InSlot.Name);
        }

        public void LoadFromXml(Record RecordConnection, object Graph)
        {
            if(Graph is NodeGraph)
            {
                NodeGraph NodeGraph = Graph as NodeGraph;

                ID = RecordConnection.GetInt("ID");
                int OutSlotNodeID = RecordConnection.GetInt("OutSlotNodeID");
                string OutSlotName = RecordConnection.GetString("OutSlotName");
                Slot OutSlot = NodeGraph.FindOutSlotByName(OutSlotNodeID, OutSlotName);
                SetOutSlot(OutSlot);
                int InSlotNodeID = RecordConnection.GetInt("InSlotNodeID");
                string InSlotName = RecordConnection.GetString("InSlotName");
                Slot InSlot = NodeGraph.FindInSlotByName(InSlotNodeID, InSlotName);
                SetInSlot(InSlot);
            }
            else if(Graph is BehaviorTree)
            {
                BehaviorTree BehaviorTree = Graph as BehaviorTree;

                ID = RecordConnection.GetInt("ID");
                int OutSlotNodeID = RecordConnection.GetInt("OutSlotNodeID");
                string OutSlotName = RecordConnection.GetString("OutSlotName");
                Slot OutSlot = BehaviorTree.FindSlotByName(OutSlotNodeID, OutSlotName);
                SetOutSlot(OutSlot);
                int InSlotNodeID = RecordConnection.GetInt("InSlotNodeID");
                string InSlotName = RecordConnection.GetString("InSlotName");
                Slot InSlot = BehaviorTree.FindSlotByName(InSlotNodeID, InSlotName);
                SetInSlot(InSlot);
            }
        }
    }
}