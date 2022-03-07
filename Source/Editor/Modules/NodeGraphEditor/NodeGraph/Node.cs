using EditorUI;
using System;
using System.Collections.Generic;

namespace Editor
{
    public class Node
    {
        private const int SpanX = 10;
        private const int SpanY = 10;
        private const int SlotStartY = 30;
        private const int SlotWidth = 14;
        private const int SlotHeight = 14;
        private const int SpanX1 = 2;

        public int ID;

        public NodeType NodeType;
        public string Name;
        private List<Slot> InSlots;
        private List<Slot> OutSlots;
        public bool bSelected;

        public int X;
        public int Y;
        public int Width;
        public int Height;

        private int ContentX;
        private int ContentY;
        private int ContentWidth;
        private int ContentHeight;

        public bool bError;

        public Node()
        {
            ID = 0;
            NodeType = NodeType.Unknown;
            Name = "";
            InSlots = new List<Slot>();
            OutSlots = new List<Slot>();
            bSelected = false;

            ContentX = 0;
            ContentY = 0;
            ContentWidth = 0;
            ContentHeight = 0;

            bError = false;
        }

        public virtual void SetError()
        {
            bError = true;
        }

        public virtual void ClearError()
        {
            bError = false;
            foreach (Slot InSlot in InSlots)
            {
                InSlot.ClearError();
            }
            foreach (Slot OutSlot in OutSlots)
            {
                OutSlot.ClearError();
            }
        }

        public void AddInSlot(Slot InSlot)
        {
            InSlot.bOutput = false;
            InSlot.Node = this;
            InSlot.Index = InSlots.Count;
            InSlots.Add(InSlot);
        }

        public void AddInSlot(string Name, SlotType SlotType)
        {
            Slot InSlot = new Slot();
            InSlot.Name = Name;
            InSlot.SlotType = SlotType;
            AddInSlot(InSlot);
        }

        public Slot GetInSlot(int Index)
        {
            return InSlots[Index];
        }

        public List<Slot> GetInSlots()
        {
            return InSlots;
        }

        public Slot FindInSlot(string InSlotName)
        {
            foreach (Slot InSlot in InSlots)
            {
                if (InSlot.Name == InSlotName)
                {
                    return InSlot;
                }
            }
            return null;
        }

        public void AddOutSlot(Slot OutSlot)
        {
            OutSlot.bOutput = true;
            OutSlot.Node = this;
            OutSlot.Index = OutSlots.Count;
            OutSlots.Add(OutSlot);
        }

        public void AddOutSlot(string Name, SlotType SlotType)
        {
            Slot OutSlot = new Slot();
            OutSlot.Name = Name;
            OutSlot.SlotType = SlotType;
            AddOutSlot(OutSlot);
        }

        public Slot GetOutSlot(int Index)
        {
            return OutSlots[Index];
        }

        public List<Slot> GetOutSlots()
        {
            return OutSlots;
        }

        public Slot FindOutSlot(string OutSlotName)
        {
            foreach (Slot OutSlot in OutSlots)
            {
                if (OutSlot.Name == OutSlotName)
                {
                    return OutSlot;
                }
            }
            return null;
        }

        public Node GetInputNode(int InSlotIndex, out int OutSlotIndex)
        {
            Slot Slot = GetInSlot(InSlotIndex);
            List<Connection> Connections = Slot.GetConnections();
            int ConnectionCount = Connections.Count;
            if (ConnectionCount >= 1)
            {
                if (ConnectionCount > 1)
                {
                    DebugHelper.Assert(false);
                }
                Connection Connection = Connections[0];
                OutSlotIndex = Connection.OutSlot.Index;
                return Connection.OutSlot.Node;
            }
            else
            {
                OutSlotIndex = -1;
                return null;
            }
        }

        public List<Node> GetOutputNodes(int OutSlotIndex)
        {
            Slot Slot = GetOutSlot(OutSlotIndex);
            List<Connection> Connections = Slot.GetConnections();
            List<Node> Nodes = new List<Node>();
            foreach (Connection Connection in Connections)
            {
                Nodes.Add(Connection.InSlot.Node);
            }
            return Nodes;
        }

        public virtual void GetContentSize(ref int ContentWidth, ref int ContentHeight)
        {
        }

        public virtual void DrawContent(int ContentX, int ContentY, int ContentWidth, int ContentHeight)
        {
        }

        public virtual void DoLayout()
        {
            GetContentSize(ref ContentWidth, ref ContentHeight);
            int MaxInSlotWidth = 0;
            foreach (Slot InSlot in InSlots)
            {
                int InSlotWidth = InSlot.MeasureNameWidth() + SpanX1 + SlotWidth;
                if (InSlotWidth > MaxInSlotWidth)
                {
                    MaxInSlotWidth = InSlotWidth;
                }
            }
            int MaxOutSlotWidth = 0;
            foreach (Slot OutSlot in OutSlots)
            {
                int OutSlotWidth = OutSlot.MeasureNameWidth() + SpanX1 + SlotWidth;
                if (OutSlotWidth > MaxOutSlotWidth)
                {
                    MaxOutSlotWidth = OutSlotWidth;
                }
            }
            ContentX = SpanX + MaxInSlotWidth + SpanX;
            Width = ContentX + ContentWidth + SpanX + MaxOutSlotWidth + SpanX;
            int Y1 = SlotStartY + SpanY;
            foreach (Slot InSlot in InSlots)
            {
                InSlot.X = SpanX;
                InSlot.Y = Y1;
                InSlot.Width = SlotWidth;
                InSlot.Height = SlotHeight;
                Y1 += (SlotHeight + SpanY);
            }
            int Y2 = SlotStartY + SpanY;
            foreach (Slot OutSlot in OutSlots)
            {
                OutSlot.X = Width - SpanX - SlotWidth;
                OutSlot.Y = Y2;
                OutSlot.Width = SlotWidth;
                OutSlot.Height = SlotHeight;
                Y2 += (SlotHeight + SpanY);
            }
            int Y3 = SlotStartY + SpanY;
            ContentY = Y3;
            Y3 += (ContentHeight + SpanY);
            Height = Math.Max(Math.Max(Y1, Y2), Y3);
        }

        public virtual void DoLayoutWithConnections()
        {
            DoLayout();
            foreach (Slot InSlot in InSlots)
            {
                InSlot.DoLayout_Connections();
            }
            foreach (Slot OutSlot in OutSlots)
            {
                OutSlot.DoLayout_Connections();
            }
        }

        public virtual void Draw()
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            Color Color1 = Color.FromRGBA(0, 0, 0, 160);
            GraphicsHelper.FillRectangle(Color1, X, Y, Width, Height);

            Color Color2 = Color.FromRGB(0, 0, 0);
            if (NodeType == NodeType.Expression)
            {
                Color2 = Color.FromRGB(96, 132, 91);
            }
            else if (NodeType == NodeType.Statement)
            {
                Color2 = Color.FromRGB(69, 110, 137);
            }
            else if (NodeType == NodeType.ControlFlow)
            {
                Color2 = Color.FromRGB(180, 105, 20);
            }
            else if (NodeType == NodeType.Event)
            {
                Color2 = Color.FromRGB(142, 20, 19);
            }
            else
            {
                DebugHelper.Assert(false);
            }
            GraphicsHelper.FillRectangle(Color2, X, Y, Width, SlotStartY);

            if (bSelected && bError)
            {
                GraphicsHelper.DrawRectangle(Color.FromRGB(255, 102, 0), X, Y, Width, Height);
            }
            else if (bSelected)
            {
                GraphicsHelper.DrawRectangle(Color.FromRGB(255, 204, 0), X, Y, Width, Height);
            }
            else if (bError)
            {
                GraphicsHelper.DrawRectangle(Color.FromRGB(255, 0, 0), X, Y, Width, Height);
            }
            else
            {
                GraphicsHelper.DrawRectangle(Color.FromRGB(0, 0, 0), X, Y, Width, Height);
            }

            Color Color3 = Color.FromRGB(255, 255, 255);
            int Height1 = SlotStartY;
            int NameOffsetY = 2;
            GraphicsHelper.DrawString(null, Name, Color3, X, Y + NameOffsetY, Width, Height1, TextAlign.CenterCenter);

            foreach (Slot InSlot in InSlots)
            {
                InSlot.Draw(X, Y);
            }

            foreach (Slot OutSlot in OutSlots)
            {
                OutSlot.Draw(X, Y);
            }

            DrawContent(X + ContentX, Y + ContentY, ContentWidth, ContentHeight);
        }

        public virtual object HitTest(int MouseX, int MouseY)
        {
            if (UIManager.PointInRect(MouseX, MouseY, X, Y, Width, Height))
            {
                int Count = InSlots.Count;
                for (int i = Count - 1; i >= 0; i--)
                {
                    Slot InSlot = InSlots[i];
                    object HitObject = InSlot.HitTest(MouseX, MouseY);
                    if (HitObject != null)
                    {
                        return HitObject;
                    }
                }
                Count = OutSlots.Count;
                for (int i = Count - 1; i >= 0; i--)
                {
                    Slot OutSlot = OutSlots[i];
                    object HitObject = OutSlot.HitTest(MouseX, MouseY);
                    if (HitObject != null)
                    {
                        return HitObject;
                    }
                }
                return this;
            }
            return null;
        }

        public virtual void SaveToXml(Record RecordNode)
        {
            Type Type = this.GetType();
            RecordNode.SetTypeString(Type.Name);
            RecordNode.SetInt("ID", ID);
            RecordNode.SetInt("X", X);
            RecordNode.SetInt("Y", Y);
        }

        public virtual void LoadFromXml(Record RecordNode)
        {
            ID = RecordNode.GetInt("ID");
            X = RecordNode.GetInt("X");
            Y = RecordNode.GetInt("Y");
        }

        public void CommitNodeError(string ErrorInformation)
        {
            SetError();
            string ErrorMessage = string.Format("Error: Graph Node {0}({1}): {2}", Name, ID, ErrorInformation);
            ConsoleUI.GetInstance().AddLogItem(LogMessageType.Error, ErrorMessage);
            MainUI.GetInstance().ActivateDockingCard_Console();
        }

        public void CommitInSlotError(int InSlotIndex, string ErrorInformation)
        {
            SetError();
            Slot Slot = GetInSlot(InSlotIndex);
            Slot.SetError();
            string ErrorMessage = string.Format("Error: Graph Slot {0}({1})-{2}({3}): {4}", Name, ID, Slot.Name, InSlotIndex, ErrorInformation);
            ConsoleUI.GetInstance().AddLogItem(LogMessageType.Error, ErrorMessage);
            MainUI.GetInstance().ActivateDockingCard_Console();
        }
    }
}