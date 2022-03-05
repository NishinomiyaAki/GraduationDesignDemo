using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace CrossEditor
{
    class BTAuxiliaryNode : Node
    {
        public int _X;
        public int _Y;
        public int _Width;
        public int _Height;

        public BTNodeType _NodeType;
        public string _Name;
        public string _Custom;
        public int _Index;
        public int _ExecuteIndex;
        public BTNode _ParentNode;

        public Font _TitleFont;
        public Font _DetailFont;

        public BTAuxiliaryNode()
        {
            _NodeType = BTNodeType.Unknown;
            _Name = "";
            _Custom = "";
            _ExecuteIndex = -1;
            bSelected = false;
            bError = false;

            _TitleFont = new Font(UIManager.DefaultFontFamily, 14);
            _DetailFont = new Font(UIManager.DefaultFontFamily, 8);
        }

        public new virtual void GetContentSize(ref int ContentWidth, ref int ContentHeight)
        {
            
        }

        public new virtual void DrawContent(int X, int Y, int Width, int Height)
        {

        }

        public override void DoLayoutWithConnections()
        {
            _ParentNode.DoLayoutWithConnections();
        }

        public void DrawIndex(int ContentX, int ContentY, int ContentWidth, int ContentHeight)
        {
            int Width = _DetailFont.MeasureString_Fast(_ExecuteIndex.ToString());
            int Height = _DetailFont.GetCharHeight();

            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            GraphicsHelper.DrawString(_DetailFont, _ExecuteIndex.ToString(), Color.EDITOR_UI_GENERAL_TEXT_COLOR, ContentX + ContentWidth - Width / 2, ContentY - Height / 2, Width, Height, TextAlign.CenterCenter);
        }

        public void Draw(int NodeX, int NodeY)
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            Color Color1 = Color.FromRGB(0, 0, 0);
            if (_NodeType == BTNodeType.Decorator)
            {
                Color1 = Color.FromRGB(180, 105, 20);
            }
            else if (_NodeType == BTNodeType.Service)
            {
                Color1 = Color.FromRGB(142, 20, 19);
            }
            GraphicsHelper.FillRectangle(Color1, NodeX + _X, NodeY + _Y, _Width, _Height);
            DrawContent(NodeX + _X, NodeY + _Y, _Width, _Height);

            if (bSelected && bError)
            {
                GraphicsHelper.DrawRectangle(Color.FromRGB(255, 102, 0), NodeX + _X, NodeY + _Y, _Width, _Height);
            }
            else if (bSelected)
            {
                GraphicsHelper.DrawRectangle(Color.FromRGB(255, 204, 0), NodeX + _X, NodeY + _Y, _Width, _Height);
            }
            else if (bError)
            {
                GraphicsHelper.DrawRectangle(Color.FromRGB(255, 0, 0), NodeX + _X, NodeY + _Y, _Width, _Height);
            }
            else
            {
                GraphicsHelper.DrawRectangle(Color.FromRGB(0, 0, 0), NodeX + _X, NodeY + _Y, _Width, _Height);
            }

            DrawIndex(NodeX + _X, NodeY + _Y, _Width, _Height);
        }

        public override object HitTest(int MouseX, int MouseY)
        {
            object HitObject = null;
            if (UIManager.PointInRect(MouseX, MouseY,_ParentNode.X + _X,_ParentNode.Y + _Y, _Width, _Height))
            {
                HitObject = this;
            }
            return HitObject;
        }

        public new virtual void SaveToXml(Record RecordNode)
        {
            Type Type = this.GetType();
            RecordNode.SetTypeString(Type.Name);
            RecordNode.SetInt("ID", ID);
            RecordNode.SetInt("ParrentID", _ParentNode.ID);
            RecordNode.SetInt("Index", _Index);
        }

        public new virtual void LoadFromXml(Record RecordNode)
        {
            // avoid override origin version
            DebugHelper.Assert(false);
        }

        public virtual void LoadFromXml(Record RecordNode, BehaviorTree BehaviorTree)
        {
            ID = RecordNode.GetInt("ID");
            _ParentNode = BehaviorTree.FindNodeByID(RecordNode.GetInt("ParrentID"));
            _Index = RecordNode.GetInt("Index");
            _ParentNode.AddAuxiliaryNodeDirectly(this);
        }

        public virtual string GetStringContent()
        {
            return "";
        }
    }
}
