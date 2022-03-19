using EditorUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CrossEditor
{

    public class Node
    {
        const int SpanX = 5;
        const int SpanY = 5;

        public const int SlotStartY = 20;
        public const int SlotWidth = 14;
        public const int SlotHeight = 12;
        public const int SpanX1 = 2;

        public int ID;

        protected NodeGraphModel Owner;

        public bool bHasSubGraph;
        public NodeGraphModel SubGraph;

        public NodeType NodeType;
        public string Name;

        protected List<Slot> InSlots;
        protected List<Slot> OutSlots;

        public bool bSelected;
        public bool bDebugged;
        public bool bOperable;
        public bool bEditable;

        public int X;
        public int Y;
        public int Width;
        public int Height;

        public int ContentX;
        public int ContentY;
        public int ContentWidth;
        public int ContentHeight;

        public bool bError;

        public Action RenameEvent;

        public Node()
        {
            ID = 0;

            bHasSubGraph = false;

            NodeType = NodeType.Unknown;
            Name = "";

            InSlots = new List<Slot>();
            OutSlots = new List<Slot>();

            bSelected = false;
            bDebugged = false;
            bOperable = true;
            bEditable = true;

            ContentX = 0;
            ContentY = 0;
            ContentWidth = 0;
            ContentHeight = 0;

            bError = false;

            RenameEvent = () =>
            {
                if (bHasSubGraph)
                {
                    SubGraph.Name = Name;
                }
            };
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Node Name")]
        public string NodeName
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
                RenameEvent?.Invoke();
            }
        }

        public NodeGraphModel GetOwner() => Owner;

        public void SetOwner(NodeGraphModel Model)
        {
            Owner = Model;
        }

        public void SetSubGraph(NodeGraphModel SubGraph)
        {
            bHasSubGraph = true;
            this.SubGraph = SubGraph;
            this.SubGraph.Name = Name;
            this.SubGraph.Owner = this;
        }

        public void SetError()
        {
            bError = true;
        }

        public void ClearError()
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

        public virtual void CommitError(string ErrorInformation)
        {
            SetError();
            string ErrorMessage = string.Format("Error: Graph Node {0}({1}): {2}", Name, ID, ErrorInformation);
            ConsoleUI.GetInstance().AddLogItem(LogMessageType.Error, ErrorMessage);
            MainUI.GetInstance().ActivateDockingCard_Console();
        }

        public void SetDebugStatus()
        {
            bDebugged = true;
        }
        public void ResetDebugStatus()
        {
            bDebugged = false;
        }

        #region In Slot

        public void AddInSlot(Slot InSlot)
        {
            InSlot.bOutput = false;
            InSlot.Node = this;
            InSlot.Index = InSlots.Count;
            InSlots.Add(InSlot);
        }

        public void AddInSlot(string Name, SlotType SlotType, SlotSubType SlotSubType = SlotSubType.Default)
        {
            Slot InSlot = new Slot();
            InSlot.Name = Name;
            InSlot.SlotType = SlotType;
            InSlot.SlotSubType = SlotSubType;
            AddInSlot(InSlot);
        }

        public void RemoveInSlot(int Index)
        {
            if (Index < 0 || Index > InSlots.Count - 1)
                return;

            InSlots.RemoveAt(Index);
            RefreshSlotsIndex(InSlots);
        }

        public void RemoveLastInSlot()
        {
            RemoveInSlot(InSlots.Count - 1);
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

        #endregion

        // Insert and RemoveAt Operation may cause Slot.Index false, need refresh.
        protected void RefreshSlotsIndex(List<Slot> Slots)
        {
            for(int Index = 0; Index < Slots.Count; ++Index)
            {
                Slots[Index].Index = Index;
            }
        }

        #region Out Slot

        public void AddOutSlot(Slot OutSlot)
        {
            OutSlot.bOutput = true;
            OutSlot.Node = this;
            OutSlot.Index = OutSlots.Count;
            OutSlots.Add(OutSlot);
        }

        public void AddOutSlot(string Name, SlotType SlotType, SlotSubType SlotSubType = SlotSubType.Default)
        {
            Slot OutSlot = new Slot();
            OutSlot.Name = Name;
            OutSlot.SlotType = SlotType;
            OutSlot.SlotSubType = SlotSubType;
            AddOutSlot(OutSlot);
        }

        public void RemoveOutSlot(int Index)
        {
            if (Index < 0 || Index > OutSlots.Count - 1)
                return;

            OutSlots.RemoveAt(Index);
            RefreshSlotsIndex(OutSlots);
        }

        public void RemoveLastOutSlot()
        {
            RemoveOutSlot(OutSlots.Count - 1);
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

        #endregion

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

            string ShownString = Name;
            int NameWidthSpan = SpanX * 2;
            int NameWidth = GraphicsHelper.GetInstance().DefaultFont.MeasureString_Fast(ShownString) + NameWidthSpan;
            Width = Width > NameWidth ? Width : NameWidth;

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

            Color Color2 = NodeColor.GetColor(NodeType);
            GraphicsHelper.FillRectangle(Color2, X, Y, Width, SlotStartY);

            Color ColorRect;
            if (bSelected && bError)
            {
                ColorRect = Color.FromRGB(255, 102, 0);
            }
            else if (bSelected)
            {
                ColorRect = Color.FromRGB(255, 204, 0);
            }
            else if (bDebugged)
            {
                ColorRect = Color.EDITOR_UI_COLOR_WHITE;
            }
            else if (bError)
            {
                ColorRect = Color.FromRGB(255, 0, 0);
            }
            else
            {
                ColorRect = Color.FromRGB(0, 0, 0);
            }
            GraphicsHelper.DrawRectangle(ColorRect, X, Y, Width, Height);

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

        public virtual object HitTest(int WorldX, int WorldY)
        {
            if (UIManager.PointInRect(WorldX, WorldY, X, Y, Width, Height))
            {
                int Count = InSlots.Count;
                for (int i = Count - 1; i >= 0; i--)
                {
                    Slot InSlot = InSlots[i];
                    if (InSlot.HitTest(WorldX, WorldY))
                    {
                        return InSlot;
                    }
                }
                Count = OutSlots.Count;
                for (int i = Count - 1; i >= 0; i--)
                {
                    Slot OutSlot = OutSlots[i];
                    if (OutSlot.HitTest(WorldX, WorldY))
                    {
                        return OutSlot;
                    }
                }
                return this;
            }
            return null;
        }

        public virtual bool RectInRect(int X, int Y, int Width, int Height)
        {
            return UIManager.RectInRect(X, Y, Width, Height, this.X, this.Y, this.Width, this.Height);
        }

        public virtual void CloneTo(ref Node Target)
        {
            Target.ID = ID;

            Target.X = X;
            Target.Y = Y;
            Target.Width = Width;
            Target.Height = Height;

            if (bHasSubGraph)
            {
                NodeGraphModel CloneModel = Activator.CreateInstance(SubGraph.GetType()) as NodeGraphModel;
                SubGraph.CloneTo(ref CloneModel);
                Target.SetSubGraph(CloneModel);
            }

            Type Type = this.GetType();
            PropertyInfo[] Properties = Type.GetProperties();
            foreach (PropertyInfo Info in Properties)
            {
                if (PropertyInfoAttribute.GetPropertyInfoAttribute(Info).bHide == false)
                {
                    Info.SetValue(Target, Info.GetValue(this));
                }
            }
        }
    }
}
