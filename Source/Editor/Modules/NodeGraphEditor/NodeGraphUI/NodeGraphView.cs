using EditorUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CrossEditor
{
    public struct ClipData
    {
        public List<Node> NodesToPaste;
        public List<Connection> ConnectionsToPaste;
    }

    public struct MenuBuilder
    {
        public string Text;
        public MenuItemClickedEventHandler Event;
        public bool bHasChild;
        public bool bIsSeperator;
        public List<MenuBuilder> Children;
    }

    public sealed class NodeGraphView : DockingUI
    {
        Panel Panel;
        NodeGraphModel Model;
        Panel Navigator;
        Panel Zoom;

        bool bModified;

        // For Hover
        bool bCanUpdateHover;
        object HoverObject;

        // For preview subgraph
        bool bPreviewMode;
        GraphCamera2D PreviewCamera;
        Panel PreviewPanel;
        const int PreviewSize = 128;

        // For Select Node(s)
        bool bMultipleSelect;
        List<Node> LastSelectedNodes;
        List<Node> SelectedNodes;

        // For Dragging
        bool bRightMouseDown;
        bool bMouseMoved;
        float SavedWorldX;
        float SavedWorldY;
        int RightMouseDownX;
        int RightMouseDownY;

        // For Moving
        bool bLeftMouseDown;
        int LeftMouseDownX;
        int LeftMouseDownY;
        int SavedNodeX;
        int SavedNodeY;
        List<Node> NodesMoved;
        public static int MoveStep = 10;

        // For Resize Comment
        bool bOnCommentBorder;
        int SavedCommentX;
        int SavedCommentY;
        int SavedCommentWidth;
        int SavedCommentHeight;

        // For Box Select
        bool bBoxSelect;
        int SelectionX1;
        int SelectionY1;
        int SelectionX2;
        int SelectionY2;

        // For Connection
        public Slot SlotA;
        public Slot SlotB;
        public Connection CurrentConnection;

        // For Clip
        bool bHasClipData;
        ClipData ClipData;

        // For Navigator
        const int NAV_HEIGHT = 32;
        List<NodeGraphModel> NavigatedModels;

        public delegate void MenuClickHandler();
        public event MenuClickHandler RunEvent;
        public event MenuClickHandler SaveEvent;

        public delegate void ConnectEventHandler(object Context);
        public event ConnectEventHandler BeginConnectEvent;
        public event ConnectEventHandler ConnectingEvent;

        public delegate void HoverHandler(object HoverObject);
        public event HoverHandler HoverEvent;

        public delegate void MouseEventHandler(object Context);
        public event MouseEventHandler DoubleClickEvent;

        public delegate void InspectHandler();
        public event InspectHandler InspectPropertyEvent;

        GraphCamera2D Camera
        {
            get => Model?.Camera;
        }

        public NodeGraphView()
        {
            Panel = new Panel();
            Panel.Initialize();
            Panel.PaintEvent += OnPanelPaint;
            Panel.LeftMouseDownEvent += OnPanelLeftMouseDown;
            Panel.LeftMouseUpEvent += OnPanelLeftMouseUp;
            Panel.LeftMouseDoubleClickedEvent += OnPanelLeftMouseDoubleClicked;
            Panel.RightMouseDownEvent += OnPanelRightMouseDown;
            Panel.RightMouseUpEvent += OnPanelRightMouseUp;
            Panel.MouseMoveEvent += OnPanelMouseMove;
            Panel.MouseWheelEvent += OnPanelMouseWheel;
            Panel.PositionChangedEvent += OnPanelPositionChanged;
            Panel.KeyDownEvent += OnPanelKeyDown;
            Panel.KeyUpEvent += OnPanelKeyUp;

            Navigator = new Panel();
            Navigator.SetHeight(NAV_HEIGHT);
            Navigator.SetBackgroundColor(Color.FromRGBA(0, 0, 0, 160));
            Panel.AddChild(Navigator);

            Zoom = new Panel();
            Zoom.SetHeight(NAV_HEIGHT);
            Panel.AddChild(Zoom);

            LastSelectedNodes = new List<Node>();
            SelectedNodes = new List<Node>();
            NodesMoved = new List<Node>();
            NavigatedModels = new List<NodeGraphModel>();

            base.Initialize("Node Graph", Panel);

            GetDockingCard().CloseEvent += OnDockingCardClose;

            DragDropManager DragDropManager = DragDropManager.GetInstance();
            DragDropManager.DragEndEvent += OnDragDropManagerDragEnd;
        }

        void OnDragDropManagerDragEnd(DragDropManager Sender, int MouseX, int MouseY)
        {
            if (Panel.GetVisible_Recursively() && Panel.IsPointIn_Recursively(MouseX, MouseY))
            {
                // TODO: drag drop delegate
            }
        }

        public Panel GetPanel() => Panel;

        public NodeGraphModel GetModel() => Model;

        public void BindModel(NodeGraphModel Model)
        {
            // Before bind model, make sure the panel's position has been decided.
            // Or it will draw the graph unexpectedly.

            // Clear view status
            ClearStatus();
            // Binding
            this.Model = Model;
            Model.View = this;
            // Set Camera
            Camera.Initialize(Panel.GetScreenX(), Panel.GetScreenY(), Panel.GetHeight(), Panel.GetWidth());
            // Set navgitor and zoom
            NavigatedModels.Add(Model);
            RefreshZoom();
            RefreshNavigator();
            // Set card text
            UpdateDockingCardText();
        }

        public override bool IsFocused() => _IsFocused;

        public bool GetModified() => bModified;

        public void SetModified()
        {
            bModified = true;

            UpdateDockingCardText();
        }

        public void ClearModified()
        {
            bModified = false;

            UpdateDockingCardText();
        }

        public void UpdateDockingCardText()
        {
            string text = Model.Name;
            if (bModified)
            {
                text += "*";
            }

            _DockingCard.SetText(text);
        }

        public void SetCursor(SystemCursor Cursor)
        {
            Device Device = Device.GetInstance();
            UIManager UIManager = UIManager.GetInstance();

            UIManager.SetOverridingCursor(Device.GetSystemCursor(Cursor));
        }

        public void ClearCursor()
        {
            UIManager UIManager = UIManager.GetInstance();

            UIManager.SetOverridingCursor(null);
        }

        public void CreatePreview(Node Node, int MouseX, int MouseY)
        {
            // Container
            PreviewPanel = new Panel();
            PreviewPanel.SetBackgroundColor(Color.FromRGB(0, 0, 0));
            PreviewPanel.SetPos(
                            MouseX - GetPanel().GetScreenX() + 20,
                            MouseY - GetPanel().GetScreenY() + 20
                            );
            PreviewPanel.SetSize(PreviewSize, PreviewSize);
            GetPanel().AddChild(PreviewPanel);
            // GraphPanel
            NodeGraphView Preview = new NodeGraphView();
            Panel GraphPanel = Preview.GetPanel();
            Preview.GetDockingCard().RemoveChild(GraphPanel);
            GraphPanel.SetVisible(true);
            GraphPanel.ClearChildren();
            int Span = 4;
            int GraphSize = PreviewSize - Span * 2;
            GraphPanel.SetPosition(Span, Span, GraphSize, GraphSize);
            PreviewPanel.AddChild(GraphPanel);
            // Set model to view
            Preview.Model = Node.SubGraph;
            Preview.PreviewCamera = new GraphCamera2D();
            Preview.PreviewCamera.Initialize(GraphPanel.GetScreenX(), GraphPanel.GetScreenY(), GraphPanel.GetHeight(), GraphPanel.GetWidth());
            Preview.bPreviewMode = true;
            // Zoom to show the graph entirely
            int X = 0, Y = 0, Width = 0, Height = 0;
            Node.SubGraph.GetNodesBlock(Node.SubGraph.Nodes, ref X, ref Y, ref Width, ref Height);
            Preview.PreviewCamera.WorldX = X + Width / 2 - Preview.PreviewCamera.WorldHeight * Preview.PreviewCamera.AspectRatio / 2;
            Preview.PreviewCamera.WorldY = Y + Height / 2 - Preview.PreviewCamera.WorldHeight / 2;
            int MaxSize = Math.Max(Width, Height);
            float Scale = MaxSize * 1.1f / Preview.PreviewCamera.WorldHeight;
            if (Scale > 1.0f)
            {
                Preview.PreviewCamera.Zoom(X + Width / 2, Y + Height / 2, Scale);
            }
        }

        public void ClearPreview()
        {
            if (PreviewPanel != null)
            {
                GetPanel().RemoveChild(PreviewPanel);
                PreviewPanel = null;
            }
        }

        public void SetHoverSlot(Slot Slot)
        {
            if (SlotB != null)
            {
                SlotB.bSelected = false;
            }

            SlotB = Slot;

            if (Slot == null)
            {
                return;
            }
            else
            {
                SlotB.bSelected = true;
            }
        }

        public void AddSelectedNode(Node Node, bool bInspect = true)
        {
            if (Node == null)
            {
                return;
            }

            if (SelectedNodes.Contains(Node) == false)
            {
                Node.bSelected = true;
                SelectedNodes.Add(Node);
            }

            if (bInspect)
            {
                Inspect();
            }
        }

        public void TryAddSelectedNode(Node Node)
        {
            if (SelectedNodes.Contains(Node) == false)
            {
                ClearSelectedNodes();
                AddSelectedNode(Node);
            }
        }

        public void RemoveSelectedNode(Node Node)
        {
            if (Node == null)
                return;

            if (SelectedNodes.Contains(Node) == true)
            {
                Node.bSelected = false;
                SelectedNodes.Remove(Node);
            }
            Inspect();
        }

        public void ClearSelectedNodes(bool bInspect = true)
        {
            foreach (Node Node in SelectedNodes)
            {
                Node.bSelected = false;
            }
            SelectedNodes.Clear();
            if (bInspect)
            {
                Inspect();
            }
        }

        public void Inspect()
        {
            // Always inspect the last one
            object InspectObject = null;
            if (SelectedNodes.Count > 0)
            {
                InspectObject = SelectedNodes[SelectedNodes.Count - 1];
            }

            if (InspectPropertyEvent != null && InspectObject == null)
            {
                InspectPropertyEvent?.Invoke();
            }
            else
            {
                InspectorUI InspectorUI = InspectorUI.GetInstance();
                if (InspectorUI.GetObjectInspected() != InspectObject)
                {
                    InspectorUI.SetObjectInspected(InspectObject);
                    InspectorUI.InspectObject();
                }
            }
        }

        public void UpdateInspect()
        {
            InspectorUI.GetInstance().ReadValueAndUpdateLayout();
        }

        void DrawGrid()
        {
            float size = 1.0f;
            int statePanelOffsetX = 0;
            int statePanelOffsetY = 0;
            int stateScrollWidth = Panel.GetWidth() + (int)(1000 * 2 * size);
            int stateScrollHeight = Panel.GetHeight() + (int)(1000 * 2 * size);
            Vector4f ThickColor = new Vector4f(0.0f, 0.0f, 0.0f, 0.5f);
            Vector4f ThinColor = new Vector4f(0.5f, 0.5f, 0.5f, 0.5f);
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();
            GraphicsHelper.DrawGrid(size, 10, statePanelOffsetX * 1.0f, statePanelOffsetY * 1.0f,
             stateScrollWidth * 1.0f, stateScrollHeight * 1.0f, ThickColor, ThinColor, Model.GridID);
        }

        void DrawBoxSelect()
        {
            if (bBoxSelect)
            {
                int X1 = 0, Y1 = 0, X2 = 0, Y2 = 0;
                Camera.WorldToScreen(SelectionX1, SelectionY1, ref X1, ref Y1);
                Camera.WorldToScreen(SelectionX2, SelectionY2, ref X2, ref Y2);
                int X = Math.Min(X1, X2);
                int Y = Math.Min(Y1, Y2);
                int Width = Math.Abs(X1 - X2);
                int Height = Math.Abs(Y1 - Y2);
                Color ColorBlue = new Color(0.618f, 0.618f, 1.0f, 0.382f);
                Color ColorWhite = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                Graphics2D Graphics2D = Graphics2D.GetInstance();
                Graphics2D.FillRectangle(ColorBlue, X, Y, Width, Height);
                Graphics2D.DrawRectangle(ColorWhite, X, Y, Width, Height);
            }
        }

        public void Save()
        {
            SaveEvent?.Invoke();
        }

        void OnDockingCardClose(DockingCard Sender, ref bool bNotToClose)
        {
            Save();
        }

        public void RefreshNavigator()
        {
            Navigator.ClearChildren();

            int BorderHeight = 6;
            int SpanX = 4;
            int FontSize = NAV_HEIGHT - BorderHeight * 2;
            int SeperatorWidth = 20;
            int SeperatorHeight = 20;
            int SeperatorY = (NAV_HEIGHT - SeperatorHeight) / 2;
            Font Font = UIManager.GetInstance().GetDefaultFont(FontSize);

            int X = SpanX;
            for (int i = 0; i < NavigatedModels.Count; i++)
            {
                NodeGraphModel Model = NavigatedModels[i];
                int ButtonWidth = Font.MeasureString_Fast(Model.Name) + SpanX * 2;
                Button Button = new Button();
                Button.SetText(Model.Name);
                Button.SetFontSize(FontSize);
                Button.SetTagInt1(i);
                Button.SetTagObject(Model);
                Button.SetPosition(X, BorderHeight, ButtonWidth, FontSize);
                Button.ClickedEvent += (Sender) =>
                {
                    NavigatedModels = NavigatedModels.Take(Sender.GetTagInt1()).ToList();
                    BindModel(Sender.GetTagObject() as NodeGraphModel);
                };

                Navigator.AddChild(Button);

                X += ButtonWidth + SpanX;

                if (i != NavigatedModels.Count - 1)
                {
                    Button Seperator = new Button();
                    Seperator.SetImage(UIManager.LoadUIImage("Editor/Others/ButtonNavigatorSeperator.png"));
                    Seperator.SetPosition(X, SeperatorY, SeperatorWidth, SeperatorHeight);
                    Seperator.SetEnable(false);
                    Navigator.AddChild(Seperator);

                    X += SeperatorWidth + SpanX;
                }
            }

            int NavigatorWidth = Navigator.GetWidth();
            Control LastChild = Navigator.GetChild(Navigator.GetChildCount() - 1);
            if (LastChild.GetEndX() > NavigatorWidth)
            {
                int Offset = LastChild.GetEndX() - NavigatorWidth;
                for (int i = 0; i < Navigator.GetChildCount(); ++i)
                {
                    Navigator.GetChild(i).SetX(Navigator.GetChild(i).GetX() - Offset);
                }
            }
        }

        public void RefreshZoom()
        {
            Zoom.ClearChildren();

            int BorderHeight = 6;
            int SpanX = 4;
            int FontSize = NAV_HEIGHT - BorderHeight * 2;
            Font Font = UIManager.GetInstance().GetDefaultFont(FontSize);

            int ButtonWidth = Font.MeasureString_Fast(Camera.GetZoomText()) + SpanX * 2;
            Button Button = new Button();
            Button.SetText(Camera.GetZoomText());
            Button.SetFontSize(FontSize);
            Button.SetPosition(SpanX, BorderHeight, ButtonWidth, FontSize);
            Button.SetEnable(false);
            Zoom.AddChild(Button);

            int ZoomWidth = ButtonWidth + 2 * SpanX;
            int ZoomX = Zoom.GetX() + (Zoom.GetWidth() - ZoomWidth);
            Zoom.SetPosition(ZoomX, 0, ZoomWidth, NAV_HEIGHT);
        }

        void ClearStatus()
        {
            ClearSelectedNodes();
            LastSelectedNodes.Clear();
            NodesMoved.Clear();
            HoverObject = null;
            bHasClipData = false;
        }

        #region Panel Event

        void OnPanelPaint(Control Sender)
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();
            if (bPreviewMode)
            {
                GraphicsHelper.SetCamera(PreviewCamera);
            }
            else
            {
                GraphicsHelper.SetCamera(Camera);
            }

            DrawGrid();

            Model.Draw();

            CurrentConnection?.Draw();

            DrawBoxSelect();

            Sender.PaintChildren();

            GraphicsHelper.SetCamera(null);
        }

        void OnPanelLeftMouseDown(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            bLeftMouseDown = true;
            bCanUpdateHover = false;
            bMouseMoved = false;
            LeftMouseDownX = MouseX;
            LeftMouseDownY = MouseY;
            SlotA = null;
            SlotB = null;
            bContinue = false;
            Sender.CaptureMouse();

            bMultipleSelect = Device.GetInstance().IsControlDown();
            LastSelectedNodes = bMultipleSelect ? SelectedNodes.Clone() : new List<Node>();

            int WorldX = 0, WorldY = 0;
            Camera.ScreenToWorld(MouseX, MouseY, ref WorldX, ref WorldY);

            // Start resize comment
            if (bOnCommentBorder)
            {
                Node_Comment Comment = HoverObject as Node_Comment;
                SavedCommentX = Comment.X;
                SavedCommentY = Comment.Y;
                SavedCommentWidth = Comment.CommentWidth;
                SavedCommentHeight = Comment.CommentHeight;
            }
            else if (HoverObject != null)
            {
                // Start move node(s)
                if (HoverObject is Node)
                {
                    Node HoverNode = HoverObject as Node;
                    SavedNodeX = HoverNode.X;
                    SavedNodeY = HoverNode.Y;

                    NodesMoved.Clear();
                }
                // Begin connect
                else
                {
                    if (HoverObject is Slot)
                    {
                        SlotA = HoverObject as Slot;
                        CurrentConnection = new Connection();
                        if (SlotA.bOutput)
                        {
                            CurrentConnection.OutSlot = SlotA;
                        }
                        else
                        {
                            CurrentConnection.InSlot = SlotA;
                        }
                    }
                    else
                    {
                        BeginConnectEvent?.Invoke(HoverObject);
                    }

                    if (CurrentConnection != null)
                    {
                        CurrentConnection.TempEnd = new Vector2f(WorldX, WorldY);
                        CurrentConnection.DoLayout();
                    }
                }
            }
            // Start Box Select
            else
            {
                bBoxSelect = true;
                SelectionX1 = WorldX;
                SelectionY1 = WorldY;
                SelectionX2 = WorldX;
                SelectionY2 = WorldY;
            }
        }

        void OnPanelLeftMouseUp(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            int WorldX = 0, WorldY = 0;
            Camera.ScreenToWorld(MouseX, MouseY, ref WorldX, ref WorldY);

            // End resize comment
            if (bOnCommentBorder)
            {
                bOnCommentBorder = false;
                Node_Comment Comment = HoverObject as Node_Comment;

                // Need multiple direction
                int DeltaX = (Comment.Width - SavedCommentWidth) * (MouseX >= LeftMouseDownX ? 1 : -1);
                int DeltaY = (Comment.Height - SavedCommentHeight) * (MouseY >= LeftMouseDownY ? 1 : -1);

                EditOperation_ResizeComment EditOperation = new EditOperation_ResizeComment(this, Comment, Comment.CursorPosition, DeltaX, DeltaY);
                EditOperationManager.GetInstance().AddOperation(EditOperation);

                UpdateInspect();
            }
            // Release connection
            else if (CurrentConnection != null)
            {
                if (SlotB != null)
                {
                    Model.TryConnect(SlotA, SlotB);
                }
                else
                {
                    ShowMenu(CurrentConnection, MouseX, MouseY);
                }

                CurrentConnection = null;
            }
            // Click on the node without move
            else if (!bBoxSelect && !bMouseMoved)
            {
                Node HoverNode = HoverObject as Node;
                if (bMultipleSelect)
                {
                    if (LastSelectedNodes.Contains(HoverNode))
                    {
                        RemoveSelectedNode(HoverNode);
                    }
                    else
                    {
                        AddSelectedNode(HoverNode);
                    }
                }
                else
                {
                    TryAddSelectedNode(HoverNode);
                }
            }
            // Move Node(s)
            else if (!bBoxSelect && bMouseMoved)
            {
                if (HoverObject is Node)
                {
                    Node HoverNode = HoverObject as Node;

                    EditOperation_MoveNode EditOperation = new EditOperation_MoveNode(this, NodesMoved, SelectedNodes, HoverNode.X - SavedNodeX, HoverNode.Y - SavedNodeY);
                    EditOperationManager.GetInstance().AddOperation(EditOperation);
                }

                NodesMoved.Clear();
            }
            // Click the panel without move
            else if (bBoxSelect && !bMouseMoved)
            {
                ClearSelectedNodes();
            }
            // End Box Select
            else if (bBoxSelect && bMouseMoved)
            {
                // Do nothing
            }

            bLeftMouseDown = false;
            bCanUpdateHover = true;
            bMouseMoved = false;
            bBoxSelect = false;
            bContinue = false;
            Sender.ReleaseMouse();
        }

        void OnPanelLeftMouseDoubleClicked(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            ClearPreview();
            DoubleClickEvent?.Invoke(HoverObject);
        }

        void OnPanelRightMouseDown(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            bRightMouseDown = true;
            bCanUpdateHover = false;
            bMouseMoved = false;
            SavedWorldX = Camera.WorldX;
            SavedWorldY = Camera.WorldY;
            RightMouseDownX = MouseX;
            RightMouseDownY = MouseY;
            bContinue = false;
            Sender.CaptureMouse();

            //TODO: right mouse down delegate
        }

        void OnPanelRightMouseUp(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            if (bRightMouseDown)
            {
                // Right click on something
                if (bMouseMoved == false && Sender.IsPointIn(MouseX, MouseY))
                {
                    if (HoverObject is Node)
                    {
                        Node HoverNode = HoverObject as Node;
                        TryAddSelectedNode(HoverNode);
                    }
                    else if (HoverObject is Slot)
                    {
                        Slot HoverSlot = HoverObject as Slot;
                        TryAddSelectedNode(HoverSlot.Node);
                    }
                    ShowMenu(HoverObject, MouseX, MouseY);
                }
                // Finish drag
                else
                {
                    ClearCursor();
                }

                // Only if right mouse down event is triggered by Panel
                bContinue = false;
            }

            bRightMouseDown = false;
            bCanUpdateHover = true;
            bMouseMoved = false;
            Sender.ReleaseMouse();

            //TODO: right mouse up delegate
        }

        void OnPanelMouseMove(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            int WorldX = 0, WorldY = 0;
            Camera.ScreenToWorld(MouseX, MouseY, ref WorldX, ref WorldY);

            ClearCursor();
            ClearPreview();
            SetHoverSlot(null);
            if (bCanUpdateHover)
            {
                HoverObject = Model.HitTest(WorldX, WorldY);
                bOnCommentBorder = false;

                HoverEvent?.Invoke(HoverObject);

                if (HoverObject is Node)
                {
                    // SetCursor(SystemCursor.SizeAll);
                    Node Node = HoverObject as Node;
                    if (Node.bHasSubGraph)
                    {
                        CreatePreview(Node, MouseX, MouseY);
                    }
                }
                else if (HoverObject is Slot)
                {
                    SetHoverSlot(HoverObject as Slot);
                    SetCursor(SystemCursor.Cross);
                }
                else if (HoverObject == null)
                {
                    object HitComment = Model.HitCommentBorderTest(WorldX, WorldY);

                    // Hover over the border doesnt mean hover over the comment
                    if (HitComment != null)
                    {
                        HoverObject = HitComment;
                        bOnCommentBorder = true;
                        SetCursor((HitComment as Node_Comment).GetCursorByPosition());
                    }
                }
            }

            // Dragging
            if (bRightMouseDown)
            {
                bMouseMoved = Math.Abs(MouseX - RightMouseDownX) + Math.Abs(MouseY - RightMouseDownY) > 2;
                if (bMouseMoved)
                {
                    SetCursor(SystemCursor.Dragging);
                }

                float ZoomRatio = Camera.GetZoomRatio();
                Camera.WorldX = SavedWorldX - (MouseX - RightMouseDownX) / ZoomRatio;
                Camera.WorldY = SavedWorldY - (MouseY - RightMouseDownY) / ZoomRatio;
            }

            if (bLeftMouseDown)
            {
                bMouseMoved = Math.Abs(MouseX - LeftMouseDownX) + Math.Abs(MouseY - LeftMouseDownY) > 2;

                float ZoomRatio = Camera.GetZoomRatio();
                int DeltaX = (int)((MouseX - LeftMouseDownX) / ZoomRatio);
                int DeltaY = (int)((MouseY - LeftMouseDownY) / ZoomRatio);
                DeltaX = DeltaX / MoveStep * MoveStep;
                DeltaY = DeltaY / MoveStep * MoveStep;
                // Resize Comment
                if (bOnCommentBorder)
                {
                    Node_Comment Comment = HoverObject as Node_Comment;
                    Comment.Scale(SavedCommentWidth, SavedCommentHeight, SavedCommentX, SavedCommentY, DeltaX, DeltaY);
                }
                // Box Select
                else if (bBoxSelect)
                {
                    SelectionX2 = WorldX;
                    SelectionY2 = WorldY;

                    int PanelX = Panel.GetScreenX();
                    int PanelY = Panel.GetScreenY();
                    int PanelWidth = Panel.GetWidth();
                    int PanelHeight = Panel.GetHeight();

                    if (UIManager.PointInRect(MouseX, MouseY, PanelX, PanelY, PanelWidth, PanelHeight) == false)
                    {
                        // Slow down the speed of camera moving
                        int Factor = 10;
                        if (MouseX < PanelX)
                        {
                            Camera.MoveX((MouseX - PanelX) * 1.0f / Factor);
                        }
                        if (MouseX > PanelX + PanelWidth)
                        {
                            Camera.MoveX((MouseX - PanelX - PanelWidth) * 1.0f / Factor);
                        }
                        if (MouseY < PanelY)
                        {
                            Camera.MoveY((MouseY - PanelY) * 1.0f / Factor);
                        }
                        if (MouseY > PanelY + PanelHeight)
                        {
                            Camera.MoveY((MouseY - PanelY - PanelHeight) * 1.0f / Factor);
                        }
                    }

                    int X = Math.Min(SelectionX1, SelectionX2);
                    int Y = Math.Min(SelectionY1, SelectionY2);
                    int Width = Math.Abs(SelectionX1 - SelectionX2);
                    int Height = Math.Abs(SelectionY1 - SelectionY2);
                    List<Node> BoxSelectedNodes = Model.BoxSelect(X, Y, Width, Height);
                    // Calculate difference set
                    ClearSelectedNodes(false);
                    foreach (Node Node in BoxSelectedNodes.Except(LastSelectedNodes))
                    {
                        AddSelectedNode(Node,false);
                    }
                    foreach (Node Node in LastSelectedNodes.Except(BoxSelectedNodes))
                    {
                        AddSelectedNode(Node,false);
                    }
                    Inspect();
                }
                // Connection
                else if (CurrentConnection != null)
                {
                    CurrentConnection.TempEnd = new Vector2f(WorldX, WorldY);
                    CurrentConnection.DoLayout();

                    object TempObject = Model.HitTest(WorldX, WorldY);
                    if (TempObject is Slot)
                    {
                        SetHoverSlot(TempObject as Slot);
                    }
                    else
                    {
                        if (TempObject != null)
                        {
                            ConnectingEvent?.Invoke(TempObject);
                        }
                        else
                        {
                            SetHoverSlot(null);
                        }
                    }
                }
                // Move Node(s)
                else if (HoverObject is Node)
                {
                    Node HoverNode = HoverObject as Node;
                    TryAddSelectedNode(HoverNode);

                    int NodeXBefore = HoverNode.X;
                    int NodeYBefore = HoverNode.Y;
                    int NodeXAfter = SavedNodeX + DeltaX;
                    int NodeYAfter = SavedNodeY + DeltaY;

                    if (NodesMoved.Count == 0)
                    {
                        Model.UpdateNodesInComment();

                        NodesMoved.AddRange(SelectedNodes);
                        for (int i = 0; i < NodesMoved.Count; ++i)
                        {
                            if (NodesMoved[i] is Node_Comment)
                            {
                                (NodesMoved[i] as Node_Comment).NodesInComment.ForEach(NodeInComment =>
                                {
                                    if (NodesMoved.Contains(NodeInComment) == false)
                                    {
                                        NodesMoved.Add(NodeInComment);
                                    }
                                });
                            }
                        }
                    }

                    NodesMoved.ForEach(Item =>
                    {
                        Item.X += (NodeXAfter - NodeXBefore);
                        Item.Y += (NodeYAfter - NodeYBefore);
                    });
                }
            }
        }

        void OnPanelMouseWheel(Control Sender, int MouseX, int MouseY, int MouseDeltaZ, int MouseDeltaW, ref bool bContinue)
        {
            if (Panel.IsPointIn(MouseX, MouseY))
            {
                int WorldX = 0, WorldY = 0;
                Camera.ScreenToWorld(MouseX, MouseY, ref WorldX, ref WorldY);

                if (MouseDeltaZ > 0) // bigger
                {
                    Camera.ZoomBigger(WorldX, WorldY);
                }
                else if (MouseDeltaZ < 0) // smaller
                {
                    Camera.ZoomSmaller(WorldX, WorldY);
                }
                RefreshZoom();
            }
        }

        void OnPanelPositionChanged(Control Sender, bool bPositionChanged, bool bSizeChanged)
        {
            if (Camera != null)
            {
                Camera.ScreenX = Panel.GetScreenX();
                Camera.ScreenY = Panel.GetScreenY();
                Camera.ScreenHeight = Panel.GetHeight();
                Camera.AspectRatio = Panel.GetWidth() * 1.0f / Panel.GetHeight();
            }

            Zoom.SetX(Panel.GetWidth() - Zoom.GetWidth());
            Navigator.SetWidth(Panel.GetWidth());
        }

        void OnPanelKeyDown(Control Sender, Key Key, ref bool bContinue)
        {
            bContinue = false;
            Device Device = Device.GetInstance();
            bool bControl = Device.IsControlDown();
            bool bShift = Device.IsShiftDown();
            bool bAlt = Device.IsAltDown();
            bool bControlOnly = bControl && !bShift && !bAlt;

            if (bControlOnly)
            {
                switch (Key)
                {
                    case Key.X:
                        Cut();
                        break;
                    case Key.C:
                        Copy();
                        break;
                    case Key.V:
                        int WorldX = 0, WorldY = 0;
                        Camera.ScreenToWorld(Device.GetMouseX(), Device.GetMouseY(), ref WorldX, ref WorldY);
                        PasteAt(WorldX, WorldY);
                        break;
                    case Key.D:
                        Duplicate();
                        break;
                }
            }
        }

        void OnPanelKeyUp(Control Sender, Key Key, ref bool bContinue)
        {

        }

        #endregion

        #region Menu

        void ShowMenu(object Object, int MouseX, int MouseY)
        {
            Menu Menu = new Menu();

            BuildMenu(Menu, Model.BuildMenu(Object));

            ContextMenu.GetInstance().ShowMenu(Menu, MouseX, MouseY);
        }

        void BuildMenu(Menu Menu, List<MenuBuilder> MenuBuilders)
        {
            foreach (MenuBuilder Builder in MenuBuilders)
            {
                if (Builder.bIsSeperator)
                {
                    Menu.AddSeperator();
                }
                else
                {
                    MenuItem MenuItem = new MenuItem();
                    MenuItem.SetText(Builder.Text);

                    if (Builder.bHasChild)
                    {
                        Menu SubMenu = new Menu();
                        MenuItem.SetMenu(SubMenu);

                        if (Builder.Children.Count > 0)
                        {
                            BuildMenu(SubMenu, Builder.Children);
                        }
                        else
                        {
                            MenuItem Empty = new MenuItem();
                            Empty.SetText("Empty.");
                            SubMenu.AddMenuItem(Empty);
                        }
                    }
                    else
                    {
                        MenuItem.ClickedEvent += Builder.Event;
                    }

                    Menu.AddMenuItem(MenuItem);
                }
            }
        }

        public List<MenuBuilder> BuildContextActionMenu()
        {
            List<MenuBuilder> MenuBuilders = new List<MenuBuilder>();

            MenuBuilders.Add(new MenuBuilder { Text = "Compile & Run", Event = (Sender) => { RunEvent?.Invoke(); } });
            MenuBuilders.Add(new MenuBuilder { Text = "Save", Event = (Sender) => { SaveEvent?.Invoke(); } });
            MenuBuilders.Add(new MenuBuilder { Text = "Back to center", Event = (Sender) => { Model.MoveCameraToCenter(); } });

            if (bHasClipData)
            {
                MenuBuilders.Add(new MenuBuilder
                {
                    Text = "Paste Here",
                    Event = (Sender) =>
                    {
                        int WorldX = 0, WorldY = 0;
                        Camera.ScreenToWorld(RightMouseDownX, RightMouseDownY, ref WorldX, ref WorldY);
                        PasteAt(WorldX, WorldY);
                    }
                });
            }

            return MenuBuilders;
        }

        public List<MenuBuilder> BuildNodeActionMenu()
        {
            List<MenuBuilder> MenuBuilders = new List<MenuBuilder>();

            MenuBuilders.Add(new MenuBuilder { Text = "Delete", Event = (Sender) => { Delete(); } });
            MenuBuilders.Add(new MenuBuilder { Text = "Cut", Event = (Sender) => { Cut(); } });
            MenuBuilders.Add(new MenuBuilder { Text = "Copy", Event = (Sender) => { Copy(); } });
            MenuBuilders.Add(new MenuBuilder { Text = "Duplicate", Event = (Sender) => { Duplicate(); } });
            MenuBuilders.Add(new MenuBuilder
            {
                Text = "Alignment",
                bHasChild = true,
                Children = new List<MenuBuilder>
                {
                    new MenuBuilder{ Text = "Align Top", Event = (Sender) => { AlignNodes("Top"); }},
                    new MenuBuilder{ Text = "Align Middle", Event = (Sender) => { AlignNodes("Middle"); }},
                    new MenuBuilder{ Text = "Align Bottom", Event = (Sender) => { AlignNodes("Bottom"); }},
                    new MenuBuilder{ Text = "Align Left", Event = (Sender) => { AlignNodes("Left"); }},
                    new MenuBuilder{ Text = "Align Center", Event = (Sender) => { AlignNodes("Center"); }},
                    new MenuBuilder{ Text = "Align Right", Event = (Sender) => { AlignNodes("Right"); }},
                    new MenuBuilder{ bIsSeperator = true },
                    new MenuBuilder{ Text = "Distribute Horizontally", Event = (Sender) => { AlignNodes("Horizontally"); }},
                    new MenuBuilder{ Text = "Distribute Vertically", Event = (Sender) => { AlignNodes("Vertically"); }},
                },
            });

            return MenuBuilders;
        }

        public List<MenuBuilder> BuildSlotActionMenu(Slot Slot)
        {
            List<MenuBuilder> MenuBuilders = new List<MenuBuilder>();

            MenuBuilders.Add(new MenuBuilder
            {
                Text = string.Format("Select All {0} Nodes", Slot.bOutput ? "Output" : "Input"),
                Event = (Sender) =>
                {
                    foreach (Connection Connection in Slot.GetConnections())
                    {
                        if (Slot.bOutput)
                        {
                            AddSelectedNode(Connection.InSlot.Node);
                        }
                        else
                        {
                            AddSelectedNode(Connection.OutSlot.Node);
                        }
                    }
                }
            });
            MenuBuilders.Add(new MenuBuilder { Text = "Break All Connections", Event = (Sender) => { RemoveConnections(Slot.GetConnections()); } });
            MenuBuilders.Add(new MenuBuilder
            {
                Text = "Break Connection To",
                bHasChild = true,
                Children = Slot.GetConnections().ConvertAll(Item =>
                {
                    Slot TargetSlot = Slot.bOutput ? Item.InSlot : Item.OutSlot;
                    return new MenuBuilder
                    {
                        Text = string.Format("{0} ({1})", TargetSlot.Node.Name, TargetSlot.Name),
                        Event = (Sender) => { RemoveConnections(new List<Connection> { Item }); }
                    };
                })
            });

            return MenuBuilders;
        }

        #endregion

        #region Node Actions

        public void ImportNode(Node Node)
        {
            if (Node != null)
            {
                // Import and add to this graph
                Model.ImportNode(Node);

                int WorldX = 0, WorldY = 0;
                Camera.ScreenToWorld(RightMouseDownX, RightMouseDownY, ref WorldX, ref WorldY);
                Node.X = WorldX / MoveStep * MoveStep;
                Node.Y = WorldY / MoveStep * MoveStep;

                TryAddSelectedNode(Node);

                EditOperation_AddNode EditOperation = new EditOperation_AddNode(this, new List<Node>() { Node }, new List<Connection>(), new List<Connection>());
                EditOperationManager.GetInstance().AddOperation(EditOperation);
            }
        }

        public void ImportNodeWithConnection(Node Node, Connection ConnectionToAdd, List<Connection> ConnectionsToRemove)
        {
            if (Node != null)
            {
                Model.ImportNode(Node);

                TryAddSelectedNode(Node);

                if (ConnectionsToRemove == null)
                {
                    ConnectionsToRemove = new List<Connection>();
                }
                for (int i = ConnectionsToRemove.Count - 1; i >= 0; --i)
                {
                    Model.RemoveConnection(ConnectionsToRemove[i]);
                }

                List<Connection> ConnectionsToAdd = new List<Connection>();
                if (ConnectionToAdd != null)
                {
                    Model.AddConnection(ConnectionToAdd);
                    ConnectionsToAdd.Add(ConnectionToAdd);
                }

                EditOperation_AddNode EditOperation = new EditOperation_AddNode(this, new List<Node> { Node }, ConnectionsToAdd, ConnectionsToRemove);
                EditOperationManager.GetInstance().AddOperation(EditOperation);
            }
        }

        public void Delete()
        {
            List<Node> NodesToDelete = new List<Node>();
            SelectedNodes.ForEach(Item =>
            {
                if (Item.bOperable)
                    NodesToDelete.Add(Item);
            });

            if (NodesToDelete.Count == 0)
            {
                return;
            }

            List<Connection> ConnectionsToDelete = Model.GetConnectionsRelated(NodesToDelete);

            EditOperation_RemoveNode EditOperation = new EditOperation_RemoveNode(this, NodesToDelete, ConnectionsToDelete);
            EditOperationManager.GetInstance().AddOperation(EditOperation);

            EditOperation.Redo();

            // TODO: delete nodes delegate
        }

        public void Cut()
        {
            List<Node> NodesToCut = new List<Node>();
            SelectedNodes.ForEach(Item =>
            {
                if (Item.bOperable)
                    NodesToCut.Add(Item);
            });

            if (NodesToCut.Count == 0)
            {
                return;
            }

            bHasClipData = true;

            List<Connection> ConnectionsToCut = Model.GetConnectionsInside(NodesToCut);
            List<Connection> ConnectionsToDelete = Model.GetConnectionsRelated(NodesToCut);

            ClipData = new ClipData();
            ClipData.NodesToPaste = NodesToCut;
            ClipData.ConnectionsToPaste = ConnectionsToCut;

            EditOperation_RemoveNode EditOperation = new EditOperation_RemoveNode(this, NodesToCut, ConnectionsToDelete);
            EditOperationManager.GetInstance().AddOperation(EditOperation);

            EditOperation.Redo();

            // TODO: cut nodes delegate
        }

        public void Copy()
        {
            List<Node> NodesToCopy = new List<Node>();
            SelectedNodes.ForEach(Item =>
            {
                if (Item.bOperable)
                    NodesToCopy.Add(Item);
            });

            if (NodesToCopy.Count == 0)
            {
                return;
            }

            bHasClipData = true;

            List<Connection> ConnectionsToCopy = Model.GetConnectionsInside(NodesToCopy);

            ClipData = new ClipData();
            ClipData.NodesToPaste = NodesToCopy;
            ClipData.ConnectionsToPaste = ConnectionsToCopy;
        }

        public void Duplicate()
        {
            List<Node> NodesToDuplicate = new List<Node>();
            SelectedNodes.ForEach(Item =>
            {
                if (Item.bOperable)
                    NodesToDuplicate.Add(Item);
            });

            if (NodesToDuplicate.Count == 0)
            {
                return;
            }

            bHasClipData = true;

            List<Connection> ConnectionsToDuplicate = Model.GetConnectionsInside(NodesToDuplicate);

            ClipData = new ClipData();
            ClipData.NodesToPaste = NodesToDuplicate;
            ClipData.ConnectionsToPaste = ConnectionsToDuplicate;
            // Generate new nodes and connections
            ClipData ActualClipData = Model.GetActualClipData(ClipData);
            // Give a bias to avoid overlap
            foreach (Node Node in ActualClipData.NodesToPaste)
            {
                Node.X += 20;
                Node.Y += 20;
            }

            EditOperation_AddNode EditOperation = new EditOperation_AddNode(this, ActualClipData.NodesToPaste, ActualClipData.ConnectionsToPaste, new List<Connection>());
            EditOperationManager.GetInstance().AddOperation(EditOperation);

            EditOperation.Redo();
        }

        public void PasteAt(int WorldX, int WorldY)
        {
            ClipData ActualClipData = Model.GetActualClipData(ClipData);

            int X = 0, Y = 0, Width = 0, Height = 0;
            Model.GetNodesBlock(ActualClipData.NodesToPaste, ref X, ref Y, ref Width, ref Height);

            int AnchorX = X + Width / 5;
            int AnchorY = Y + Width / 5;
            int DeltaX = (WorldX - AnchorX) / MoveStep * MoveStep;
            int DeltaY = (WorldY - AnchorY) / MoveStep * MoveStep;

            foreach (Node Node in ActualClipData.NodesToPaste)
            {
                Node.X += DeltaX;
                Node.Y += DeltaY;
            }

            EditOperation_AddNode EditOperation = new EditOperation_AddNode(this, ActualClipData.NodesToPaste, ActualClipData.ConnectionsToPaste, new List<Connection>());
            EditOperationManager.GetInstance().AddOperation(EditOperation);

            EditOperation.Redo();
        }

        public void AlignNodes(string Direction)
        {
            int X = 0, Y = 0, Width = 0, Height = 0;
            Model.GetNodesBlock(SelectedNodes, ref X, ref Y, ref Width, ref Height);

            switch (Direction)
            {
                case "Top":
                    SelectedNodes.ForEach(Node => Node.Y = Y);
                    break;
                case "Middle":
                    SelectedNodes.ForEach(Node => Node.Y = Y + Height / 2 - Node.Height / 2);
                    break;
                case "Bottom":
                    SelectedNodes.ForEach(Node => Node.Y = Y + Height - Node.Height);
                    break;
                case "Left":
                    SelectedNodes.ForEach(Node => Node.X = X);
                    break;
                case "Center":
                    SelectedNodes.ForEach(Node => Node.X = X + Width / 2 - Node.Width / 2);
                    break;
                case "Right":
                    SelectedNodes.ForEach(Node => Node.X = X + Width - Node.Width);
                    break;
                case "Horizontally":
                    if (SelectedNodes.Count > 2)
                    {
                        SelectedNodes.Sort((Left, Right) => Left.X.CompareTo(Right.X));
                        int LastNodeIndex = SelectedNodes.FindIndex(Node => Node.X + Node.Width == X + Width);
                        Node LastNode = SelectedNodes[LastNodeIndex];
                        Node FirstNode = SelectedNodes[0];
                        int BaseLine = FirstNode.X + FirstNode.Width / 2;
                        int EndLine = LastNode.X + LastNode.Width / 2;
                        int Interval = (EndLine - BaseLine) / (SelectedNodes.Count - 1);
                        for (int i = 1; i < SelectedNodes.Count; ++i)
                        {
                            if (i < LastNodeIndex)
                            {
                                SelectedNodes[i].X = BaseLine + Interval * i - SelectedNodes[i].Width / 2;
                            }
                            else if (i > LastNodeIndex)
                            {
                                SelectedNodes[i].X = BaseLine + Interval * (i - 1) - SelectedNodes[i].Width / 2;
                            }
                        }
                    }
                    break;
                case "Vertically":
                    if (SelectedNodes.Count > 2)
                    {
                        SelectedNodes.Sort((Left, Right) => Left.Y.CompareTo(Right.Y));
                        int LastNodeIndex = SelectedNodes.FindIndex(Node => Node.Y + Node.Height == Y + Height);
                        Node LastNode = SelectedNodes[LastNodeIndex];
                        Node FirstNode = SelectedNodes[0];
                        int BaseLine = FirstNode.Y + FirstNode.Height / 2;
                        int EndLine = LastNode.Y + LastNode.Height / 2;
                        int Interval = (EndLine - BaseLine) / (SelectedNodes.Count - 1);
                        for (int i = 1; i < SelectedNodes.Count; ++i)
                        {
                            if (i < LastNodeIndex)
                            {
                                SelectedNodes[i].Y = BaseLine + Interval * i - SelectedNodes[i].Height / 2;
                            }
                            else if (i > LastNodeIndex)
                            {
                                SelectedNodes[i].Y = BaseLine + Interval * (i - 1) - SelectedNodes[i].Height / 2;
                            }
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Connection Actions

        public void ImportConnection(Connection ConnectionToAdd, Connection ConnectionToRemove = null)
        {
            if (ConnectionToAdd != null)
            {
                List<Connection> ConnectionsToRemove = new List<Connection>();
                if (ConnectionToRemove == null)
                {
                    ConnectionsToRemove.Add(ConnectionToRemove);
                }

                EditOperation_AddConnection EditOperation = new EditOperation_AddConnection(this, ConnectionsToRemove, new List<Connection> { ConnectionToAdd });
                EditOperationManager.GetInstance().AddOperation(EditOperation);

                EditOperation.Redo();
            }
        }

        public void AddConnections(List<Connection> ConnectionsToAdd, List<Connection> ConnectionsToRemove = null)
        {
            if (ConnectionsToAdd != null)
            {
                if (ConnectionsToRemove == null)
                {
                    ConnectionsToRemove = new List<Connection>();
                }

                EditOperation_AddConnection EditOperation = new EditOperation_AddConnection(this, ConnectionsToRemove, ConnectionsToAdd);
                EditOperationManager.GetInstance().AddOperation(EditOperation);

                EditOperation.Redo();
            }
        }

        public void RemoveConnections(List<Connection> Connections)
        {
            if (Connections.Count != 0)
            {
                EditOperation_RemoveConnection EditOperation = new EditOperation_RemoveConnection(this, Connections);
                EditOperationManager.GetInstance().AddOperation(EditOperation);

                EditOperation.Redo();
            }
        }

        #endregion
    }
}
