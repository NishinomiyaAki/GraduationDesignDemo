using EditorUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CrossEditor
{
    public enum CursorPosition
    {
        Other = -1,
        LeftTop = 0,
        CenterTop,
        RightTop,
        RightCenter,
        RightBottom,
        CenterBottom,
        LeftBottom,
        LeftCenter
    }

    public class Node_Comment : Node
    {
        public List<Node> NodesInComment;

        const int Border = 4;
        int MinWidth;

        public CursorPosition CursorPosition;

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Comment Width")]
        public int CommentWidth
        {
            get
            {
                return Width;
            }
            set
            {
                Width = value >= MinWidth ? value : MinWidth;
            }
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Comment Height")]
        public int CommentHeight
        {
            get
            {
                return Height;
            }
            set
            {
                Height = value >= 50 ? value : 50;
            }
        }

        public Node_Comment()
        {
            Name = "Comment";
            NodeType = NodeType.Comment;

            MinWidth = 50;
            Width = 200;
            Height = 200;

            NodesInComment = new List<Node>();
            CursorPosition = CursorPosition.Other;
        }

        public override object HitTest(int WorldX, int WorldY)
        {
            if (UIManager.PointInRect(WorldX, WorldY, X + Border, Y + Border, Width - 2 * Border, 20 - Border))
            {
                return this;
            }
            return null;
        }

        public bool HitBorderTest(int WorldX, int WorldY)
        {
            if (UIManager.PointInRect(WorldX, WorldY, X, Y, Border, Border))
            {
                CursorPosition = CursorPosition.LeftTop;
            }
            else if (UIManager.PointInRect(WorldX, WorldY, X + Width - Border, Y + Height - Border, Border, Border))
            {
                CursorPosition = CursorPosition.RightBottom;
            }
            else if (UIManager.PointInRect(WorldX, WorldY, X + Width - Border, Y, Border, Border))
            {
                CursorPosition = CursorPosition.RightTop;
            }
            else if (UIManager.PointInRect(WorldX, WorldY, X, Y + Height - Border, Border, Border))
            {
                CursorPosition = CursorPosition.LeftBottom;
            }
            else if (UIManager.PointInRect(WorldX, WorldY, X, Y + Border, Border, Height - 2 * Border))
            {
                CursorPosition = CursorPosition.LeftCenter;
            }
            else if (UIManager.PointInRect(WorldX, WorldY, X + Width - Border, Y + Border, Border, Height - 2 * Border))
            {
                CursorPosition = CursorPosition.RightCenter;
            }
            else if (UIManager.PointInRect(WorldX, WorldY, X + Border, Y, Width - 2 * Border, Border))
            {
                CursorPosition = CursorPosition.CenterTop;
            }
            else if (UIManager.PointInRect(WorldX, WorldY, X + Border, Y + Height - Border, Width - 2 * Border, Border))
            {
                CursorPosition = CursorPosition.CenterBottom;
            }
            else
            {
                CursorPosition = CursorPosition.Other;
            }

            return CursorPosition != CursorPosition.Other;
        }

        public void Scale(int WidthBefore, int HeightBefore, int XBefore, int YBefore, int DeltaX, int DeltaY)
        {
            switch (CursorPosition)
            {
                case CursorPosition.LeftTop:
                    CommentWidth = WidthBefore - DeltaX;
                    CommentHeight = HeightBefore - DeltaY;
                    DeltaX = WidthBefore - CommentWidth;
                    DeltaY = HeightBefore - CommentHeight;
                    X = XBefore + DeltaX;
                    Y = YBefore + DeltaY;
                    break;
                case CursorPosition.CenterTop:
                    CommentHeight = HeightBefore - DeltaY;
                    DeltaY = HeightBefore - CommentHeight;
                    Y = YBefore + DeltaY;
                    break;
                case CursorPosition.RightTop:
                    CommentWidth = WidthBefore + DeltaX;
                    CommentHeight = HeightBefore - DeltaY;
                    DeltaY = HeightBefore - CommentHeight;
                    Y = YBefore + DeltaY;
                    break;
                case CursorPosition.RightCenter:
                    CommentWidth = WidthBefore + DeltaX;
                    break;
                case CursorPosition.RightBottom:
                    CommentWidth = WidthBefore + DeltaX;
                    CommentHeight = HeightBefore + DeltaY;
                    break;
                case CursorPosition.CenterBottom:
                    CommentHeight = HeightBefore + DeltaY;
                    break;
                case CursorPosition.LeftBottom:
                    CommentWidth = WidthBefore - DeltaX;
                    CommentHeight = HeightBefore + DeltaY;
                    DeltaX = WidthBefore - CommentWidth;
                    X = XBefore + DeltaX;
                    break;
                case CursorPosition.LeftCenter:
                    CommentWidth = WidthBefore - DeltaX;
                    DeltaX = WidthBefore - CommentWidth;
                    X = XBefore + DeltaX;
                    break;
            }
        }

        public override bool RectInRect(int X1, int Y1, int Width1, int Height1)
        {
            return UIManager.RectInRect(X1, Y1, Width1, Height1, X, Y, Width, 20);
        }

        public override void DoLayoutWithConnections()
        {
            string shownString = Name;
            //if (this is Anim_FlowNode)
            //{
            //    shownString += " #" + ID.ToString();
            //}
            int NameWidthSpan = 10;
            int NameWidth = GraphicsHelper.GetInstance().DefaultFont.MeasureString_Fast(shownString) + NameWidthSpan;
            MinWidth = NameWidth;
            Width = Width > NameWidth ? Width : NameWidth;
        }

        public SystemCursor GetCursorByPosition()
        {
            switch(CursorPosition)
            {
                case CursorPosition.LeftTop:
                case CursorPosition.RightBottom:
                    return SystemCursor.SizeNWSE;

                case CursorPosition.RightTop:
                case CursorPosition.LeftBottom:
                    return SystemCursor.SizeNESW;

                case CursorPosition.LeftCenter:
                case CursorPosition.RightCenter:
                    return SystemCursor.SizeWE;

                case CursorPosition.CenterTop:
                case CursorPosition.CenterBottom:
                    return SystemCursor.SizeNS;
            }

            return SystemCursor.Arrow;
        }

        public bool ContainNode(Node Node)
        {
            return (X <= Node.X) && (Y <= Node.Y) && (X + Width >= Node.X + Node.Width) && (Y + Height >= Node.Y + Node.Height);
        }
    }
}
