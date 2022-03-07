using EditorUI;
using System;
using System.Collections.Generic;

namespace Editor
{
    enum ResultType
    {
        Succeeded,
        Failed,
        Aborted,
        Executing
    }

    internal class BTNode : Node
    {
        private const int _SpanX = 10;
        private const int _SpanY = 10;
        private const int _SlotWidth = 14;
        private const int _SlotSpanY = 12;
        private const int _Interval = 2;

        public BTNodeType _NodeType;
        public string _Name;
        public string _Custom;
        public int _ExecuteIndex;
        public Slot _InSlot;
        public Slot _OutSlot;
        public Slot _MainOutSlot;
        public List<BTAuxiliaryNode> _Decorators;
        public List<BTAuxiliaryNode> _Services;
        public bool _bExecuting;
        
        private int _ContentX;
        private int _ContentY;
        private int _ContentWidth;
        private int _ContentHeight;

        public Font _TitleFont;
        public Font _DetailFont;

        public BTNode()
        {
            ID = 0;
            _NodeType = BTNodeType.Unknown;
            _Name = "";
            _Custom = "";
            _ExecuteIndex = -1;
            _InSlot = null;
            _OutSlot = null;
            _MainOutSlot = null;
            _Decorators = new List<BTAuxiliaryNode>();
            _Services = new List<BTAuxiliaryNode>();
            _bExecuting = false;
            bSelected = false;

            _TitleFont = new Font(UIManager.DefaultFontFamily, 14);
            _DetailFont = new Font(UIManager.DefaultFontFamily, 8);

            _ContentX = 0;
            _ContentY = 0;
            _ContentWidth = 0;
            _ContentHeight = 0;

            bError = false;
        }

        public override void SetError()
        {
            bError = true;
        }

        public override void ClearError()
        {
            bError = false;
            _InSlot.ClearError();
            _OutSlot.ClearError();
            _MainOutSlot.ClearError();
        }

        public void SetInSlot()
        {
            _InSlot = new Slot();
            _InSlot.Name = "InSlot";
            _InSlot.SlotType = SlotType.ControlFlow;
            _InSlot.Node = this;
            _InSlot.bOutput = false;
            _InSlot.bHideName = true;
        }

        public Slot GetInSlot()
        {
            return _InSlot;
        }

        public void SetOutSlot()
        {
            _OutSlot = new Slot();
            _OutSlot.Name = "OutSlot";
            _OutSlot.SlotType = SlotType.ControlFlow;
            _OutSlot.Node = this;
            _OutSlot.bOutput = true;
            _OutSlot.bHideName = true;
        }

        public Slot GetOutSlot()
        {
            return _OutSlot;
        }

        public void SetMainOutSlot()
        {
            _MainOutSlot = new Slot();
            _MainOutSlot.Name = "MainOutSlot";
            _MainOutSlot.SlotType = SlotType.DataFlow;
            _MainOutSlot.Node = this;
            _MainOutSlot.bOutput = true;
            _MainOutSlot.bHideName = true;
        }

        public Slot GetMainOutSlot()
        {
            return _MainOutSlot;
        }

        public Slot FindSlot(string SlotName)
        {
            if(_InSlot != null && _InSlot.Name == SlotName)
            {
                return _InSlot;
            }
            else if(_OutSlot != null && _OutSlot.Name == SlotName)
            {
                return _OutSlot;
            }
            else if (_MainOutSlot != null && _MainOutSlot.Name == SlotName)
            {
                return _MainOutSlot;
            }
            else
            {
                return null;
            }
        }

        public BTNode GetParentNode()
        {
            if(_InSlot == null)
            {
                return null;
            }
            List<Connection> Connections = _InSlot.GetConnections();
            if (Connections.Count == 1)
            {
                return Connections[0].OutSlot.Node as BTNode;
            }
            else
            {
                return null;
            }
        }

        public List<BTNode> GetChildNodes()
        {
            List<BTNode> Nodes = new List<BTNode>();
            if(_OutSlot == null)
            {
                return Nodes;
            }
            List<Connection> Connections = _OutSlot.GetConnections();
            foreach (Connection Connection in Connections)
            {
                if(Connection.InSlot != null)
                {
                    Nodes.Add(Connection.InSlot.Node as BTNode);
                }
            }
            return Nodes;
        }

        public List<BTNode> GetMainChildNodes()
        {
            List<BTNode> Nodes = new List<BTNode>();
            if (_MainOutSlot == null)
            {
                return Nodes;
            }
            List<Connection> Connections = _MainOutSlot.GetConnections();
            foreach (Connection Connection in Connections)
            {
                if (Connection.InSlot != null)
                {
                    Nodes.Add(Connection.InSlot.Node as BTNode);
                }
            }
            return Nodes;
        }

        public void AddAuxiliaryNode(BTAuxiliaryNode AuxiliaryNode)
        {
            AuxiliaryNode._ParentNode = this;
            if(AuxiliaryNode._NodeType == BTNodeType.Decorator)
            {
                AuxiliaryNode._Index = _Decorators.Count;
                _Decorators.Add(AuxiliaryNode);
            }
            else if(AuxiliaryNode._NodeType == BTNodeType.Service)
            {
                AuxiliaryNode._Index = _Services.Count;
                _Services.Add(AuxiliaryNode);
            }
        }

        public void AddAuxiliaryNodeDirectly(BTAuxiliaryNode AuxiliaryNode)
        {
            if(AuxiliaryNode._NodeType == BTNodeType.Decorator)
            {
                _Decorators.Add(AuxiliaryNode);
                _Decorators.Sort((left, right) =>
                {
                    return left._Index.CompareTo(right._Index);
                });
            }
            else if (AuxiliaryNode._NodeType == BTNodeType.Service)
            {
                _Services.Add(AuxiliaryNode);
                _Services.Sort((left, right) =>
                {
                    return left._Index.CompareTo(right._Index);
                });
            }
        }

        public void RemoveAuxiliaryNode(BTAuxiliaryNode AuxiliaryNode)
        {
            if(AuxiliaryNode._NodeType == BTNodeType.Decorator)
            {
                _Decorators.Remove(AuxiliaryNode);
            }
            else if(AuxiliaryNode._NodeType == BTNodeType.Service)
            {
                _Services.Remove(AuxiliaryNode);
            }
            DoLayout();
        }

        public List<BTAuxiliaryNode> GetDecorators()
        {
            return _Decorators;
        }

        public List<BTAuxiliaryNode> GetServices()
        {
            return _Services;
        }

        public override void DoLayout()
        {
            int X = 0, Y = 0, MaxWidth = 0;
            X += _SpanX;
            Y += _SpanY;
            if (_InSlot != null)
            {
                X += _SlotWidth + _SpanX;
            }

            foreach (BTAuxiliaryNode Decorator in _Decorators)
            {
                int DecoratorWidth = 0, DecoratorHeight = 0;
                Decorator.GetContentSize(ref DecoratorWidth, ref DecoratorHeight);
                Decorator._X = X;
                Decorator._Y = Y + _Interval;
                MaxWidth = Math.Max(MaxWidth, DecoratorWidth);
                Decorator._Height = DecoratorHeight;
                Y += _Interval + DecoratorHeight + _Interval;
            }

            GetContentSize(ref _ContentWidth, ref _ContentHeight);
            _ContentX = X;
            _ContentY = Y + _Interval;
            MaxWidth = Math.Max(MaxWidth, _ContentWidth);
            Y += _Interval + _ContentHeight + _Interval;

            foreach (BTAuxiliaryNode Service in _Services)
            {
                int ServiceWidth = 0, ServiceHeight = 0;
                Service.GetContentSize(ref ServiceWidth, ref ServiceHeight);
                Service._X = X;
                Service._Y = Y + _Interval;
                MaxWidth = Math.Max(MaxWidth, ServiceWidth);
                Service._Height = ServiceHeight;
                Y += _Interval + Service._Height + _Interval;
            }

            Y += _SpanY;
            int SlotHeight = Y - 2 * _SlotSpanY;

            foreach (BTAuxiliaryNode Decorator in _Decorators)
            {
                Decorator._Width = MaxWidth;
            }

            _ContentWidth = MaxWidth;

            foreach (BTAuxiliaryNode Service in _Services)
            {
                Service._Width = MaxWidth;
            }

            X += MaxWidth + _SpanX;

            if (_InSlot != null)
            {
                _InSlot.X = _SpanX;
                _InSlot.Y = _SlotSpanY;
                _InSlot.Width = _SlotWidth;
                _InSlot.Height = SlotHeight;
            }

            if (_OutSlot != null && _MainOutSlot == null)
            {
                _OutSlot.X = X;
                _OutSlot.Y = _SlotSpanY;
                _OutSlot.Width = _SlotWidth;
                _OutSlot.Height = SlotHeight;
                X += _SlotWidth + _SpanX;
            }
            else if (_OutSlot != null && _MainOutSlot != null)
            {
                _MainOutSlot.X = X;
                _MainOutSlot.Y = _SlotSpanY;
                _MainOutSlot.Width = _SlotWidth;
                _MainOutSlot.Height = (Y - 3 * _SlotSpanY) / 3;

                _OutSlot.X = X;
                _OutSlot.Y = _MainOutSlot.Height + 2 * _SlotSpanY;
                _OutSlot.Width = _SlotWidth;
                _OutSlot.Height = (Y - 3 * _SlotSpanY) / 3 * 2;

                X += _SlotWidth + _SpanX;
            }

            Width = X;
            Height = Y;
        }

        public override void DoLayoutWithConnections()
        {
            DoLayout();
            _InSlot?.DoLayout_Connections();
            _OutSlot?.DoLayout_Connections();
            _MainOutSlot?.DoLayout_Connections();
        }

        public void DrawIndex(int ContentX, int ContentY, int ContentWidth, int ContentHeight)
        {
            int Width = _DetailFont.MeasureString_Fast(_ExecuteIndex.ToString());
            int Height = _DetailFont.GetCharHeight();

            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            GraphicsHelper.DrawString(_DetailFont, _ExecuteIndex.ToString(), Color.EDITOR_UI_GENERAL_TEXT_COLOR, ContentX + ContentWidth - Width / 2, ContentY - Height / 2, Width, Height, TextAlign.CenterCenter);
        }

        public override void Draw()
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            Color Color1 = Color.FromRGBA(0, 0, 0, 160);
            GraphicsHelper.FillRectangle(Color1, X, Y, Width, Height);

            Color Color2 = Color.FromRGB(0, 0, 0);
            if (_NodeType == BTNodeType.Composite)
            {
                Color2 = Color.FromRGB(96, 132, 91);
            }
            else if (_NodeType == BTNodeType.Task)
            {
                Color2 = Color.FromRGB(69, 110, 137);
            }
            GraphicsHelper.FillRectangle(Color2, X + _ContentX, Y + _ContentY, _ContentWidth, _ContentHeight);
            DrawContent(X + _ContentX, Y + _ContentY, _ContentWidth, _ContentHeight);

            if (_bExecuting)
            {
                GraphicsHelper.DrawRectangle(Color.FromRGB(255, 0, 0), X, Y, Width, Height);
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

            if(_InSlot != null)
            {
                _InSlot.Draw(X, Y);
            }

            if(_OutSlot != null)
            {
                _OutSlot.Draw(X, Y);
            }

            if(_MainOutSlot != null)
            {
                _MainOutSlot.Draw(X, Y);
            }

            foreach(BTAuxiliaryNode Decorator in _Decorators)
            {
                Decorator.Draw(X, Y);
            }

            foreach(BTAuxiliaryNode Service in _Services)
            {
                Service.Draw(X, Y);
            }

            DrawIndex(X + _ContentX, Y + _ContentY, _ContentWidth, _ContentHeight);
        }

        public override object HitTest(int MouseX, int MouseY)
        {
            object HitObject = null;
            if(UIManager.PointInRect(MouseX, MouseY, X, Y, Width, Height))
            {
                HitObject = _InSlot?.HitTest(MouseX, MouseY);
                if(HitObject != null)
                {
                    return HitObject;
                }

                HitObject = _OutSlot?.HitTest(MouseX, MouseY);
                if(HitObject != null)
                {
                    return HitObject;
                }

                HitObject = _MainOutSlot?.HitTest(MouseX, MouseY);
                if(HitObject != null)
                {
                    return HitObject;
                }

                foreach(BTAuxiliaryNode Decorator in _Decorators)
                {
                    HitObject = Decorator.HitTest(MouseX, MouseY);
                    if (HitObject != null)
                    {
                        return HitObject;
                    }
                }

                foreach(BTAuxiliaryNode Service in _Services)
                {
                    HitObject = Service.HitTest(MouseX, MouseY);
                    if(HitObject != null)
                    {
                        return HitObject;
                    }
                }

                HitObject = this;
            }
            return HitObject;
        }

        public ResultType StringToResultType(string String)
        {
            return Enum.Parse<ResultType>(String);
        }

        public virtual void Execute(BehaviorTreeComponent BTComponent)
        {
            foreach (BTServiceNode ServiceNode in GetServices())
            {
                BTComponent.AddServiceNode(ServiceNode);
            }

            foreach (BTDecoratorNode DecoratorNode in GetDecorators())
            {
                if(DecoratorNode.Abort == AbortMode.Self)
                {
                    BTComponent.AddDecoratorNode(DecoratorNode);
                }
            }
        }

        public virtual void Tick(BehaviorTreeComponent BTComponent, float DeltaTime)
        {
        }

        public virtual bool DoPreDecoratorExecute(BehaviorTreeComponent BTComponent)
        {
            if(GetDecorators().Count == 0 || 
                GetDecorators().FindIndex(Decorator =>
                {
                    return (Decorator as BTDecoratorNode).Abort == AbortMode.None;
                }) == -1)
            {
                // if there is no decorator on this node
                // or all decorators' abort mode is None
                // , condition should always be true.
                return true;
            }
            else
            {
                string Message;
                foreach(BTDecoratorNode Decorator in GetDecorators())
                {
                    if(Decorator.Abort == AbortMode.None)
                    {
                        bool bAllow = Decorator.CheckCondition(BTComponent);
                        if(bAllow == false)
                        {
                            Message = string.Format("Condition Failed: At {0} Node (Name:{1}, ID:{2})", Decorator._Name, Decorator._Custom, Decorator.ID);
                            ConsoleUI.GetInstance().AddLogItem(LogMessageType.Information, Message);
                            return false;
                        }
                        else
                        {
                            Message = string.Format("Condition Success: At {0} Node (Name:{1}, ID:{2})", Decorator._Name, Decorator._Custom, Decorator.ID);
                            ConsoleUI.GetInstance().AddLogItem(LogMessageType.Information, Message);
                        }
                    }
                }
                return true;
            }
        }
    }
}