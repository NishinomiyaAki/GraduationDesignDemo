using EditorUI;
using System;
using System.Collections.Generic;

namespace Editor
{
    public class NodeGraphUI : DockingUI
    {
        private static NodeGraphUI _Instance = new NodeGraphUI();

        private string Filename;
        private NodeGraph NodeGraph;
        private object SelectedObject;

        private Node CurrentNode;
        private int SavedNodeX;
        private int SavedNodeY;
        private int SavedMouseOffsetX;
        private int SavedMouseOffsetY;

        private Connection CurrentConnection;

        private bool bRightMouseDown;
        private bool bMouseMoved;
        private int SavedMouseX;
        private int SavedMouseY;

        private bool bModified;

        public static NodeGraphUI GetInstance()
        {
            return _Instance;
        }

        public bool Initialize()
        {
            NodeGraph = new NodeGraph();

            Filename = "";

            _Panel = new Panel();
            _Panel.Initialize();
            _Panel.SetAutoRefresh(true);
            _Panel.PaintEvent += OnPanelPaint;
            _Panel.LeftMouseDownEvent += OnPanelLeftMouseDown;
            _Panel.LeftMouseUpEvent += OnPanelLeftMouseUp;
            _Panel.RightMouseDownEvent += OnPanelRightMouseDown;
            _Panel.RightMouseUpEvent += OnPanelRightMouseUp;
            _Panel.MouseMoveEvent += OnPanelMouseMove;

            bModified = false;

            base.Initialize("NodeGraph");

            return true;
        }

        public bool IsFocused()
        {
            if (_DockingCard.GetDockingBlock() != null &&
                _DockingCard.GetDockingBlock().GetFocused() &&
                _DockingCard.GetActive())
            {
                return true;
            }
            InspectorUI InspectorUI = InspectorUI.GetInstance();
            object ObjectInspected = InspectorUI.GetObjectInspected();
            if (ObjectInspected != null &&
                ObjectInspected is Node)
            {
                return true;
            }
            return false;
        }

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
            if (bModified)
            {
                _DockingCard.SetText("NodeGraph*");
            }
            else
            {
                _DockingCard.SetText("NodeGraph");
            }
        }

        public void OpenVisualScript(string Filename)
        {
            this.Filename = Filename;
            NodeGraph.LoadFromXml(Filename);
            EditOperationManager.GetInstance().ClearAll();
            ClearModified();
        }

        private void OnDeviceActivate(bool bActivated)
        {
        }

        private void OnPanelPaint(Control Sender)
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            int BaseX = _Panel.GetScreenX() + NodeGraph.OffsetX;
            int BaseY = _Panel.GetScreenY() + NodeGraph.OffsetY;
            GraphicsHelper.SetBaseXAndBaseY(BaseX, BaseY);

            NodeGraph.Draw();

            if (CurrentConnection != null)
            {
                CurrentConnection.Draw();
            }
        }

        private void SetObjectSelected(object Object, bool bSelected)
        {
            if (Object is Node)
            {
                Node Node = Object as Node;
                Node.bSelected = bSelected;
            }
            else if (Object is Slot)
            {
                Slot Slot = Object as Slot;
                Slot.bSelected = bSelected;
            }
            else if (Object is Connection)
            {
                Connection Connection = Object as Connection;
                Connection.bSelected = bSelected;
            }
        }

        private int GetBaseX()
        {
            return _Panel.GetScreenX() + NodeGraph.OffsetX;
        }

        private int GetBaseY()
        {
            return _Panel.GetScreenY() + NodeGraph.OffsetY;
        }

        private void OnPanelLeftMouseDown(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            if (SelectedObject != null)
            {
                SetObjectSelected(SelectedObject, false);
                SelectedObject = null;
            }
            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(null);
            int BaseX = GetBaseX();
            int BaseY = GetBaseY();
            object HitObject = NodeGraph.HitTest(MouseX - BaseX, MouseY - BaseY);
            if (HitObject != null)
            {
                SetObjectSelected(HitObject, true);
                SelectedObject = HitObject;

                if (SelectedObject is Node)
                {
                    CurrentNode = SelectedObject as Node;
                    NodeGraph.MoveToTop(CurrentNode);
                    SavedNodeX = CurrentNode.X;
                    SavedNodeY = CurrentNode.Y;
                    SavedMouseOffsetX = MouseX - CurrentNode.X;
                    SavedMouseOffsetY = MouseY - CurrentNode.Y;
                    Sender.CaptureMouse();
                    bContinue = false;

                    InspectorUI.SetObjectInspected(CurrentNode);
                }
                else if (SelectedObject is Slot)
                {
                    Slot Slot = SelectedObject as Slot;
                    CurrentConnection = new Connection();
                    if (Slot.bOutput)
                    {
                        CurrentConnection.SetOutSlot(Slot);
                    }
                    else
                    {
                        CurrentConnection.SetInSlot(Slot);
                    }
                    CurrentConnection.TempEnd = new Vector2f(MouseX - BaseX, MouseY - BaseY);
                    CurrentConnection.DoLayout();
                    Sender.CaptureMouse();
                    bContinue = false;
                }
            }
            InspectorUI.InspectObject();
        }

        private void OnPanelLeftMouseUp(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            if (CurrentNode != null)
            {
                int OldX = SavedNodeX;
                int OldY = SavedNodeY;
                int NewX = CurrentNode.X;
                int NewY = CurrentNode.Y;
                if (NewX != OldX || NewY != OldY)
                {
                    SetModified();

                    EditOperation_MoveNode EditOperation = new EditOperation_MoveNode(CurrentNode, OldX, OldY, NewX, NewY);
                    EditOperationManager.GetInstance().AddOperation(EditOperation);
                }

                Sender.ReleaseMouse();
                CurrentNode = null;
                bContinue = false;
            }
            if (CurrentConnection != null)
            {
                int BaseX = GetBaseX();
                int BaseY = GetBaseY();
                object HitObject = NodeGraph.HitTest(MouseX - BaseX, MouseY - BaseY);
                if (HitObject != null)
                {
                    if (HitObject is Slot)
                    {
                        Slot Slot = HitObject as Slot;
                        Slot OutSlot = null;
                        Slot InSlot = null;
                        if (CurrentConnection.InSlot != null)
                        {
                            InSlot = CurrentConnection.InSlot;
                            OutSlot = Slot;
                        }
                        else if (CurrentConnection.OutSlot != null)
                        {
                            OutSlot = CurrentConnection.OutSlot;
                            InSlot = Slot;
                        }
                        else
                        {
                            DebugHelper.Assert(false);
                        }

                        if ((InSlot.bOutput ^ OutSlot.bOutput) == true)
                        {
                            ConnectionItemList ConnectionItemList_RemoveOldConnections = new ConnectionItemList(NodeGraph);
                            foreach (Connection Connection in InSlot.GetConnections())
                            {
                                if (Connection.InSlot != null && Connection.OutSlot != null)
                                {
                                    ConnectionItemList_RemoveOldConnections.AddConnection(Connection);
                                }
                            }

                            NodeGraph.RemoveConnection(CurrentConnection);
                            NodeGraph.ClearConnectionsOfSlot(InSlot);
                            CurrentConnection.SetOutSlot(OutSlot);
                            CurrentConnection.SetInSlot(InSlot);
                            NodeGraph.AddConnection(CurrentConnection);
                            CurrentConnection.DoLayout();

                            SetModified();

                            ConnectionItemList ConnectionItemList_AddNewConnection = new ConnectionItemList(NodeGraph);
                            ConnectionItemList_AddNewConnection.AddConnection(CurrentConnection);
                            EditOperation_AddConnection EditOperation = new EditOperation_AddConnection(ConnectionItemList_RemoveOldConnections, ConnectionItemList_AddNewConnection);
                            EditOperationManager.GetInstance().AddOperation(EditOperation);
                        }
                        else
                        {
                            InSlot?.RemoveConnection(CurrentConnection);
                            OutSlot?.RemoveConnection(CurrentConnection);
                        }
                    }
                }
                Sender.ReleaseMouse();
                CurrentConnection = null;
                bContinue = false;
            }
        }

        private void OnPanelRightMouseDown(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            bRightMouseDown = true;
            bMouseMoved = false;
            SavedMouseOffsetX = MouseX - NodeGraph.OffsetX;
            SavedMouseOffsetY = MouseY - NodeGraph.OffsetY;
            SavedMouseX = MouseX;
            SavedMouseY = MouseY;
            Sender.CaptureMouse();
        }

        private void OnPanelRightMouseUp(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            if (bRightMouseDown)
            {
                bRightMouseDown = false;
                Sender.ReleaseMouse();
                if (bMouseMoved == false)
                {
                    if (Sender.IsPointIn(MouseX, MouseY))
                    {
                        ShowContextMenu(MouseX, MouseY);
                    }
                }
                bContinue = false;
            }
        }

        private void OnPanelMouseMove(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            if (CurrentNode != null)
            {
                CurrentNode.X = MouseX - SavedMouseOffsetX;
                CurrentNode.Y = MouseY - SavedMouseOffsetY;
                CurrentNode.DoLayoutWithConnections();
                SetModified();
            }

            if (CurrentConnection != null)
            {
                int BaseX = GetBaseX();
                int BaseY = GetBaseY();
                CurrentConnection.TempEnd = new Vector2f(MouseX - BaseX, MouseY - BaseY);
                CurrentConnection.DoLayout();
            }

            if (bRightMouseDown)
            {
                NodeGraph.OffsetX = MouseX - SavedMouseOffsetX;
                NodeGraph.OffsetY = MouseY - SavedMouseOffsetY;
                int Distance = Math.Abs(MouseX - SavedMouseX) + Math.Abs(MouseY - SavedMouseY);
                if (Distance > 3)
                {
                    bMouseMoved = true;
                }
            }
        }

        private ConnectionItemList CollectConnectionsOfNode(Node Node)
        {
            ConnectionItemList ConnectionItemList = new ConnectionItemList(NodeGraph);
            List<Slot> InSlots = Node.GetInSlots();
            foreach (Slot InSlot in InSlots)
            {
                List<Connection> Connections = InSlot.GetConnections();
                foreach (Connection Connection in Connections)
                {
                    ConnectionItemList.AddConnection(Connection);
                }
            }
            List<Slot> OutSlots = Node.GetOutSlots();
            foreach (Slot OutSlot in OutSlots)
            {
                List<Connection> Connections = OutSlot.GetConnections();
                foreach (Connection Connection in Connections)
                {
                    ConnectionItemList.AddConnection(Connection);
                }
            }
            return ConnectionItemList;
        }

        public void DoDuplicate()
        {
            if (CurrentNode != null ||
                CurrentConnection != null)
            {
                return;
            }
            if (SelectedObject is Node)
            {
                Node Node = SelectedObject as Node;

                Record RecordNode = new Record();
                Node.SaveToXml(RecordNode);

                Node Node1 = NodeGraph.LoadNode(RecordNode);
                AddNodeTo(Node1, Node.X + 10, Node.Y + 10);

                SetModified();
            }
        }

        public void DoDeleteNode(Node Node)
        {
            ConnectionItemList ConnectionItemList = CollectConnectionsOfNode(Node);
            NodeGraph.RemoveNode(Node);

            SetModified();

            EditOperation_RemoveNode EditOperation = new EditOperation_RemoveNode(NodeGraph, Node, ConnectionItemList);
            EditOperationManager.GetInstance().AddOperation(EditOperation);
        }

        public void DoDelete()
        {
            if (CurrentNode != null ||
                CurrentConnection != null)
            {
                return;
            }
            if (SelectedObject is Node)
            {
                Node Node = SelectedObject as Node;
                DoDeleteNode(Node);
            }
            else if (SelectedObject is Slot)
            {
                Slot Slot = SelectedObject as Slot;
                DoDeleteNode(Slot.Node);
            }
            else if (SelectedObject is Connection)
            {
                Connection Connection = SelectedObject as Connection;
                NodeGraph.RemoveConnection(Connection);

                SetModified();

                ConnectionItemList ConnectionItemList = new ConnectionItemList(NodeGraph);
                ConnectionItemList.AddConnection(Connection);
                EditOperation_RemoveConnection EditOperation = new EditOperation_RemoveConnection(ConnectionItemList);
                EditOperationManager.GetInstance().AddOperation(EditOperation);
            }
            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(null);
            InspectorUI.InspectObject();
        }

        public void OnDeleteKeyDown()
        {
            DoDelete();
        }

        public void SelectNode(Node Node)
        {
            if (SelectedObject != null)
            {
                SetObjectSelected(SelectedObject, false);
                SelectedObject = null;
            }
            SetObjectSelected(Node, true);
            SelectedObject = Node;
        }

        private void AddNodeTo(Node Node, int X, int Y)
        {
            NodeGraph.AddNode(Node);
            SelectNode(Node);
            Node.X = X;
            Node.Y = Y;
            Node.DoLayoutWithConnections();

            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(Node);
            InspectorUI.InspectObject();

            EditOperation_AddNode EditOperation = new EditOperation_AddNode(NodeGraph, Node);
            EditOperationManager.GetInstance().AddOperation(EditOperation);
        }

        private void AddNodeToMouse(Node Node)
        {
            int BaseX = GetBaseX();
            int BaseY = GetBaseY();
            int X = SavedMouseX - BaseX;
            int Y = SavedMouseY - BaseY;
            AddNodeTo(Node, X, Y);

            SetModified();
        }

        private void ShowContextMenu(int MouseX, int MouseY)
        {
            Menu MenuContextMenu = new Menu();
            MenuContextMenu.Initialize();

            MenuItem MenuItem_Run = new MenuItem();
            MenuItem_Run.SetText("Compile & Run");
            MenuItem_Run.ClickedEvent += OnMenuItemCompileAndRunClicked;

            MenuItem MenuItem_Stop = new MenuItem();
            MenuItem_Stop.SetText("Stop");
            MenuItem_Stop.ClickedEvent += (Sender) => { StopRunning(); };

            MenuItem MenuItem_Save = new MenuItem();
            MenuItem_Save.SetText("Save");
            MenuItem_Save.ClickedEvent += OnMenuItemSaveClicked;

            MenuItem MenuItem_Duplicate = new MenuItem();
            MenuItem_Duplicate.SetText("Duplicate");
            MenuItem_Duplicate.ClickedEvent += OnMenuItemDuplicateClicked;

            MenuItem MenuItem_Delete = new MenuItem();
            MenuItem_Delete.SetText("Delete");
            MenuItem_Delete.ClickedEvent += OnMenuItemDeleteClicked;

            Menu Menu_Node_Event = new Menu();
            Menu_Node_Event.Initialize();
            MenuItem MenuItem_Node_Event = new MenuItem();
            MenuItem_Node_Event.SetText("Event");
            MenuItem_Node_Event.SetMenu(Menu_Node_Event);

            MenuItem MenuItem_Node_Event_Start = new MenuItem();
            MenuItem_Node_Event_Start.SetText("Start");
            MenuItem_Node_Event_Start.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_Event("Start")); };
            Menu_Node_Event.AddMenuItem(MenuItem_Node_Event_Start);

            MenuItem MenuItem_Node_Event_Update = new MenuItem();
            MenuItem_Node_Event_Update.SetText("Update");
            MenuItem_Node_Event_Update.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_Event("Update")); };
            Menu_Node_Event.AddMenuItem(MenuItem_Node_Event_Update);

            MenuItem MenuItem_Node_Event_RunBehaviorTree = new MenuItem();
            MenuItem_Node_Event_RunBehaviorTree.SetText("Run Behavior Tree");
            MenuItem_Node_Event_RunBehaviorTree.ClickedEvent += (Sender) => { AddNodeToMouse(new FlowNode_RunBehaviorTree()); };
            Menu_Node_Event.AddMenuItem(MenuItem_Node_Event_RunBehaviorTree);

            Menu Menu_Node_Branch = new Menu();
            Menu_Node_Branch.Initialize();
            MenuItem MenuItem_Node_Branch = new MenuItem();
            MenuItem_Node_Branch.SetText("Branch");
            MenuItem_Node_Branch.SetMenu(Menu_Node_Branch);

            MenuItem MenuItem_Node_Branch_If = new MenuItem();
            MenuItem_Node_Branch_If.SetText("If");
            MenuItem_Node_Branch_If.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_If()); };
            Menu_Node_Branch.AddMenuItem(MenuItem_Node_Branch_If);

            Menu Menu_Node_Loop = new Menu();
            Menu_Node_Loop.Initialize();
            MenuItem MenuItem_Node_Loop = new MenuItem();
            MenuItem_Node_Loop.SetText("Loop");
            MenuItem_Node_Loop.SetMenu(Menu_Node_Loop);

            MenuItem MenuItem_Node_Loop_WhileLoop = new MenuItem();
            MenuItem_Node_Loop_WhileLoop.SetText("WhileLoop");
            MenuItem_Node_Loop_WhileLoop.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_WhileLoop()); };
            Menu_Node_Loop.AddMenuItem(MenuItem_Node_Loop_WhileLoop);

            MenuItem MenuItem_Node_Loop_ForLoop = new MenuItem();
            MenuItem_Node_Loop_ForLoop.SetText("ForLoop");
            MenuItem_Node_Loop_ForLoop.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_ForLoop()); };
            Menu_Node_Loop.AddMenuItem(MenuItem_Node_Loop_ForLoop);

            Menu Menu_Node_LogicOp = new Menu();
            Menu_Node_LogicOp.Initialize();
            MenuItem MenuItem_Node_LogicOp = new MenuItem();
            MenuItem_Node_LogicOp.SetText("LogicOp");
            MenuItem_Node_LogicOp.SetMenu(Menu_Node_LogicOp);

            MenuItem MenuItem_Node_LogicOp_And = new MenuItem();
            MenuItem_Node_LogicOp_And.SetText("And");
            MenuItem_Node_LogicOp_And.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_BinaryLogicOp(BinaryLogicOp.And)); };
            Menu_Node_LogicOp.AddMenuItem(MenuItem_Node_LogicOp_And);

            MenuItem MenuItem_Node_LogicOp_Or = new MenuItem();
            MenuItem_Node_LogicOp_Or.SetText("Or");
            MenuItem_Node_LogicOp_Or.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_BinaryLogicOp(BinaryLogicOp.Or)); };
            Menu_Node_LogicOp.AddMenuItem(MenuItem_Node_LogicOp_Or);

            MenuItem MenuItem_Node_LogicOp_Xor = new MenuItem();
            MenuItem_Node_LogicOp_Xor.SetText("Xor");
            MenuItem_Node_LogicOp_Xor.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_BinaryLogicOp(BinaryLogicOp.Xor)); };
            Menu_Node_LogicOp.AddMenuItem(MenuItem_Node_LogicOp_Xor);

            Menu_Node_LogicOp.AddSeperator();

            MenuItem MenuItem_Node_LogicOp_Not = new MenuItem();
            MenuItem_Node_LogicOp_Not.SetText("Not");
            MenuItem_Node_LogicOp_Not.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_UnaryLogicOp(UnaryLogicOp.Not)); };
            Menu_Node_LogicOp.AddMenuItem(MenuItem_Node_LogicOp_Not);

            Menu Menu_Node_Compare = new Menu();
            Menu_Node_Compare.Initialize();
            MenuItem MenuItem_Node_Compare = new MenuItem();
            MenuItem_Node_Compare.SetText("Compare");
            MenuItem_Node_Compare.SetMenu(Menu_Node_Compare);

            MenuItem MenuItem_Node_Compare_Equal = new MenuItem();
            MenuItem_Node_Compare_Equal.SetText("==");
            MenuItem_Node_Compare_Equal.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_Compare(Relation.EqualTo)); };
            Menu_Node_Compare.AddMenuItem(MenuItem_Node_Compare_Equal);

            MenuItem MenuItem_Node_Compare_Inequal = new MenuItem();
            MenuItem_Node_Compare_Inequal.SetText("!=");
            MenuItem_Node_Compare_Inequal.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_Compare(Relation.InequalTo)); };
            Menu_Node_Compare.AddMenuItem(MenuItem_Node_Compare_Inequal);

            MenuItem MenuItem_Node_Compare_LowerTo = new MenuItem();
            MenuItem_Node_Compare_LowerTo.SetText("<");
            MenuItem_Node_Compare_LowerTo.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_Compare(Relation.LowerTo)); };
            Menu_Node_Compare.AddMenuItem(MenuItem_Node_Compare_LowerTo);

            MenuItem MenuItem_Node_Compare_LowerEqual = new MenuItem();
            MenuItem_Node_Compare_LowerEqual.SetText("<=");
            MenuItem_Node_Compare_LowerEqual.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_Compare(Relation.LowerEqualTo)); };
            Menu_Node_Compare.AddMenuItem(MenuItem_Node_Compare_LowerEqual);

            MenuItem MenuItem_Node_Compare_GreaterTo = new MenuItem();
            MenuItem_Node_Compare_GreaterTo.SetText(">");
            MenuItem_Node_Compare_GreaterTo.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_Compare(Relation.GreaterTo)); };
            Menu_Node_Compare.AddMenuItem(MenuItem_Node_Compare_GreaterTo);

            MenuItem MenuItem_Node_Compare_GreaterEqual = new MenuItem();
            MenuItem_Node_Compare_GreaterEqual.SetText(">=");
            MenuItem_Node_Compare_GreaterEqual.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_Compare(Relation.GreaterEqualTo)); };
            Menu_Node_Compare.AddMenuItem(MenuItem_Node_Compare_GreaterEqual);

            Menu Menu_Node_ArithOp = new Menu();
            Menu_Node_ArithOp.Initialize();
            MenuItem MenuItem_Node_ArithOp = new MenuItem();
            MenuItem_Node_ArithOp.SetText("ArithOp");
            MenuItem_Node_ArithOp.SetMenu(Menu_Node_ArithOp);

            MenuItem MenuItem_Node_ArithOp_Add = new MenuItem();
            MenuItem_Node_ArithOp_Add.SetText("+");
            MenuItem_Node_ArithOp_Add.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_BinaryArithOp(BinaryArithOp.Add)); };
            Menu_Node_ArithOp.AddMenuItem(MenuItem_Node_ArithOp_Add);

            MenuItem MenuItem_Node_ArithOp_Substract = new MenuItem();
            MenuItem_Node_ArithOp_Substract.SetText("- ");
            MenuItem_Node_ArithOp_Substract.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_BinaryArithOp(BinaryArithOp.Substract)); };
            Menu_Node_ArithOp.AddMenuItem(MenuItem_Node_ArithOp_Substract);

            MenuItem MenuItem_Node_ArithOp_Multiply = new MenuItem();
            MenuItem_Node_ArithOp_Multiply.SetText("*");
            MenuItem_Node_ArithOp_Multiply.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_BinaryArithOp(BinaryArithOp.Multiply)); };
            Menu_Node_ArithOp.AddMenuItem(MenuItem_Node_ArithOp_Multiply);

            MenuItem MenuItem_Node_ArithOp_Divide = new MenuItem();
            MenuItem_Node_ArithOp_Divide.SetText("/");
            MenuItem_Node_ArithOp_Divide.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_BinaryArithOp(BinaryArithOp.Divide)); };
            Menu_Node_ArithOp.AddMenuItem(MenuItem_Node_ArithOp_Divide);

            MenuItem MenuItem_Node_ArithOp_Modulo = new MenuItem();
            MenuItem_Node_ArithOp_Modulo.SetText("%");
            MenuItem_Node_ArithOp_Modulo.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_BinaryArithOp(BinaryArithOp.Modulo)); };
            Menu_Node_ArithOp.AddMenuItem(MenuItem_Node_ArithOp_Modulo);

            Menu_Node_ArithOp.AddSeperator();

            MenuItem MenuItem_Node_ArithOp_Neg = new MenuItem();
            MenuItem_Node_ArithOp_Neg.SetText("- ");
            MenuItem_Node_ArithOp_Neg.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_UnaryArithOp(UnaryArithOp.Negative)); };
            Menu_Node_ArithOp.AddMenuItem(MenuItem_Node_ArithOp_Neg);

            Menu Menu_Node_Constant = new Menu();
            Menu_Node_Constant.Initialize();
            MenuItem MenuItem_Node_Constant = new MenuItem();
            MenuItem_Node_Constant.SetText("Constant");
            MenuItem_Node_Constant.SetMenu(Menu_Node_Constant);

            MenuItem MenuItem_Node_Constant_True = new MenuItem();
            MenuItem_Node_Constant_True.SetText("True");
            MenuItem_Node_Constant_True.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_Bool(true)); };
            Menu_Node_Constant.AddMenuItem(MenuItem_Node_Constant_True);

            MenuItem MenuItem_Node_Constant_False = new MenuItem();
            MenuItem_Node_Constant_False.SetText("False");
            MenuItem_Node_Constant_False.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_Bool(false)); };
            Menu_Node_Constant.AddMenuItem(MenuItem_Node_Constant_False);

            MenuItem MenuItem_Node_Constant_Integer = new MenuItem();
            MenuItem_Node_Constant_Integer.SetText("Integer");
            MenuItem_Node_Constant_Integer.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_Integer(0)); };
            Menu_Node_Constant.AddMenuItem(MenuItem_Node_Constant_Integer);

            MenuItem MenuItem_Node_Constant_Float = new MenuItem();
            MenuItem_Node_Constant_Float.SetText("Float");
            MenuItem_Node_Constant_Float.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_Float(0.0f)); };
            Menu_Node_Constant.AddMenuItem(MenuItem_Node_Constant_Float);

            MenuItem MenuItem_Node_Constant_String = new MenuItem();
            MenuItem_Node_Constant_String.SetText("String");
            MenuItem_Node_Constant_String.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_String("")); };
            Menu_Node_Constant.AddMenuItem(MenuItem_Node_Constant_String);

            Menu Menu_Node_Convert = new Menu();
            Menu_Node_Convert.Initialize();
            MenuItem MenuItem_Node_Convert = new MenuItem();
            MenuItem_Node_Convert.SetText("Convert");
            MenuItem_Node_Convert.SetMenu(Menu_Node_Convert);

            MenuItem MenuItem_Node_Convert_ToInt = new MenuItem();
            MenuItem_Node_Convert_ToInt.SetText("ToInt");
            MenuItem_Node_Convert_ToInt.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_ToInt()); };
            Menu_Node_Convert.AddMenuItem(MenuItem_Node_Convert_ToInt);

            MenuItem MenuItem_Node_Convert_ToFloat = new MenuItem();
            MenuItem_Node_Convert_ToFloat.SetText("ToFloat");
            MenuItem_Node_Convert_ToFloat.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_ToFloat()); };
            Menu_Node_Convert.AddMenuItem(MenuItem_Node_Convert_ToFloat);

            MenuItem MenuItem_Node_Convert_ToString = new MenuItem();
            MenuItem_Node_Convert_ToString.SetText("ToString");
            MenuItem_Node_Convert_ToString.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_ToString()); };
            Menu_Node_Convert.AddMenuItem(MenuItem_Node_Convert_ToString);

            Menu Menu_Node_Function = new Menu();
            Menu_Node_Function.Initialize();
            MenuItem MenuItem_Node_Function = new MenuItem();
            MenuItem_Node_Function.SetText("Function");
            MenuItem_Node_Function.SetMenu(Menu_Node_Function);

            MenuItem MenuItem_Node_Function_PrintString = new MenuItem();
            MenuItem_Node_Function_PrintString.SetText("PrintString");
            MenuItem_Node_Function_PrintString.ClickedEvent += (MenuItem Sender) => { AddNodeToMouse(new FlowNode_PrintString()); };
            Menu_Node_Function.AddMenuItem(MenuItem_Node_Function_PrintString);

            MenuContextMenu.AddMenuItem(MenuItem_Run);
            MenuContextMenu.AddMenuItem(MenuItem_Stop);
            MenuContextMenu.AddMenuItem(MenuItem_Save);
            MenuContextMenu.AddSeperator();
            MenuContextMenu.AddMenuItem(MenuItem_Duplicate);
            MenuContextMenu.AddMenuItem(MenuItem_Delete);
            MenuContextMenu.AddSeperator();
            MenuContextMenu.AddMenuItem(MenuItem_Node_Event);
            MenuContextMenu.AddMenuItem(MenuItem_Node_Branch);
            MenuContextMenu.AddMenuItem(MenuItem_Node_Loop);
            MenuContextMenu.AddMenuItem(MenuItem_Node_LogicOp);
            MenuContextMenu.AddMenuItem(MenuItem_Node_Compare);
            MenuContextMenu.AddMenuItem(MenuItem_Node_ArithOp);
            MenuContextMenu.AddMenuItem(MenuItem_Node_Constant);
            MenuContextMenu.AddMenuItem(MenuItem_Node_Convert);
            MenuContextMenu.AddMenuItem(MenuItem_Node_Function);

            ContextMenu.GetInstance().SetForm(MainUI.GetInstance().MainWindow);
            ContextMenu.GetInstance().ShowMenu(MenuContextMenu, MouseX, MouseY);
        }

        private void OnMenuItemCompileAndRunClicked(MenuItem Sender)
        {
            OnMenuItemSaveClicked(null);
            NodeGraph.ClearError();
            ConsoleUI.GetInstance().ClearAll();
            FlowNode_Event FlowNode_Event_Start = NodeGraph.FindEventNode("Start");
            FlowNode_Event_Start.Run();
            MainUI.GetInstance().ActivateDockingCard_Console();
        }

        public override void DoSave()
        {
            StopRunning();
            if (Filename != "")
            {
                NodeGraph.SaveToXml(Filename);
            }
            ClearModified();
        }

        private void OnMenuItemSaveClicked(MenuItem Sender)
        {
            DoSave();
        }

        private void OnMenuItemDuplicateClicked(MenuItem Sender)
        {
            DoDuplicate();
        }

        private void OnMenuItemDeleteClicked(MenuItem Sender)
        {
            DoDelete();
        }

        private void StopRunning()
        {
            foreach (Node Node in NodeGraph.FindRunBehaviorTreeNode())
            {
                (Node as FlowNode_RunBehaviorTree).Stop();
            }
        }

        public void LocateToNode(int NodeID)
        {
            Node Node = NodeGraph.FindNodeByID(NodeID);
            if (Node != null)
            {
                NodeGraph.OffsetX = -(Node.X + Node.Width / 2) + _Panel.GetWidth() / 2;
                NodeGraph.OffsetY = -(Node.Y + Node.Height / 2) + _Panel.GetHeight() / 2;
                SelectNode(Node);
            }
        }
    }
}