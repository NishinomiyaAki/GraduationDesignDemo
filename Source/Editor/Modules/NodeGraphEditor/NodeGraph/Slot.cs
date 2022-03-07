using EditorUI;
using System.Collections.Generic;

namespace CrossEditor
{
    public class Slot
    {
        private const int SpanX = 5;
        private const int SpanX1 = 3;

        public Node Node;

        public bool bOutput;
        public SlotType SlotType;
        public string Name;
        private List<Connection> Connections;

        public bool bSelected;
        public int Index;

        public int X;
        public int Y;
        public int Width;
        public int Height;

        public bool bError;
        public bool bHideName;

        public Slot()
        {
            bOutput = false;
            SlotType = SlotType.DataFlow;
            Name = "";
            Connections = new List<Connection>();
            bSelected = false;
            Index = -1;
            bError = false;
            bHideName = false;
        }

        public void SetError()
        {
            bError = true;
        }

        public void ClearError()
        {
            bError = false;
        }

        public List<Connection> GetConnections()
        {
            return Connections;
        }

        public void AddConnection(Connection Connection)
        {
            if (Connections.Contains(Connection) == false)
            {
                Connections.Add(Connection);
            }
        }

        public void RemoveConnection(Connection Connection)
        {
            if (Connections.Contains(Connection))
            {
                Connections.Remove(Connection);
            }
        }

        public void DoLayout_Connections()
        {
            foreach (Connection Connection in Connections)
            {
                Connection.DoLayout();
            }
        }

        public int MeasureNameWidth()
        {
            return GraphicsHelper.GetInstance().DefaultFont.MeasureString_Fast(Name);
        }

        public Vector2f GetInputSlot()
        {
            return new Vector2f(Node.X + X - SpanX, Node.Y + Y + Height / 2);
        }

        public Vector2f GetOutputSlot()
        {
            return new Vector2f(Node.X + X + Width + SpanX, Node.Y + Y + Height / 2);
        }

        public void Draw(int NodeX, int NodeY)
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            Color Color1;
            if (SlotType == SlotType.DataFlow)
            {
                Color1 = Color.FromRGB(110, 220, 120);
            }
            else
            {
                Color1 = Color.FromRGB(220, 70, 89);
            }
            GraphicsHelper.FillRectangle(Color1, NodeX + X, NodeY + Y, Width, Height);

            if(bHideName == false)
            {
                Color Color2 = Color.FromRGB(255, 255, 255);
                if (bError)
                {
                    Color2 = Color.FromRGB(255, 0, 0);
                }
                int Width1 = MeasureNameWidth();
                int Height1 = GraphicsHelper.DefaultFont.GetCharHeight();
                if (bOutput == false)
                {
                    int X1 = NodeX + X + Width + SpanX1;
                    int Y1 = NodeY + Y + Height / 2 - Height1 / 2;
                    GraphicsHelper.DrawString(null, Name, Color2, X1, Y1, Width1, Height1, TextAlign.CenterLeft);
                }
                else
                {
                    int X1 = NodeX + X - Width1 - SpanX1;
                    int Y1 = NodeY + Y + Height / 2 - Height1 / 2;
                    GraphicsHelper.DrawString(null, Name, Color2, X1, Y1, Width1, Height1, TextAlign.CenterLeft);
                }
            }
        }

        public object HitTest(int MouseX, int MouseY)
        {
            int X1 = Node.X + X;
            int Y1 = Node.Y + Y;
            if (UIManager.PointInRect(MouseX, MouseY, X1, Y1, Width, Height))
            {
                return this;
            }
            return null;
        }
    }
}