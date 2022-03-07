using EditorUI;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrossEditor
{
    internal class BehaviorTreeUI : DockingUI
    {
        private static BehaviorTreeUI _Instance = new BehaviorTreeUI();

        private string _FileName;
        private BehaviorTree _BehaviorTree;
        private object _SelectedObject;

        private BTNode _CurrentNode;
        private int _SavedNodeX;
        private int _SavedNodeY;
        private int _SavedMouseOffsetX;
        private int _SavedMouseOffsetY;

        private Connection _CurrentConnection;

        private bool _bRightMouseDown;
        private bool _bMouseMoved;
        private int _SavedMouseX;
        private int _SavedMouseY;

        private bool _bModified;

        public static BehaviorTreeUI GetInstance()
        {
            return _Instance;
        }

        public bool Initialize()
        {
            _BehaviorTree = new BehaviorTree();

            _FileName = "";

            _Panel = new Panel();
            _Panel.Initialize();
            _Panel.SetAutoRefresh(true);
            _Panel.PaintEvent += OnPanelPaint;
            _Panel.LeftMouseDownEvent += OnPanelLeftMouseDown;
            _Panel.LeftMouseUpEvent += OnPanelLeftMouseUp;
            _Panel.RightMouseDownEvent += OnPanelRightMouseDown;
            _Panel.RightMouseUpEvent += OnPanelRightMouseUp;
            _Panel.MouseMoveEvent += OnPanelMouseMove;

            _bModified = false;

            base.Initialize("BehaviorTree");

            return true;
        }

        public void SetModified()
        {
            _bModified = true;
            UpdateDockingCardText();
        }

        public void ClearModified()
        {
            _bModified = false;
            UpdateDockingCardText();
        }

        public void UpdateDockingCardText()
        {
            if (_bModified)
            {
                _DockingCard.SetText("BehaviorTree*");
            }
            else
            {
                _DockingCard.SetText("BehaviorTree");
            }
        }

        public void OpenBehaviorTree(string FileName)
        {
            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(null);
            InspectorUI.InspectObject();
            if (_FileName != "")
            {
                DoSave();
            }
            _FileName = FileName;
            FileInfo FileInfo = new FileInfo(FileName);
            _BehaviorTree = BehaviorTreeManager.GetInstance().GetBehaviorTreeByName(FileInfo.Name);
            _BehaviorTree?.DoLayout();
            ClearModified();
        }

        private void OnPanelPaint(Control Sender)
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            int BaseX = _Panel.GetScreenX() + _BehaviorTree.OffsetX;
            int BaseY = _Panel.GetScreenY() + _BehaviorTree.OffsetY;
            GraphicsHelper.SetBaseXAndBaseY(BaseX, BaseY);

            _BehaviorTree.Draw();

            if (_CurrentConnection != null)
            {
                _CurrentConnection.Draw();
            }
        }

        private void SetObjectSelected(object Object, bool bSelected)
        {
            if (Object is BTNode)
            {
                BTNode BTNode = Object as BTNode;
                BTNode.bSelected = bSelected;
            }
            else if (Object is BTAuxiliaryNode)
            {
                BTAuxiliaryNode Node = Object as BTAuxiliaryNode;
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
            return _Panel.GetScreenX() + _BehaviorTree.OffsetX;
        }

        private int GetBaseY()
        {
            return _Panel.GetScreenY() + _BehaviorTree.OffsetY;
        }

        private void OnPanelLeftMouseDown(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            if (_SelectedObject != null)
            {
                SetObjectSelected(_SelectedObject, false);
                _SelectedObject = null;
            }
            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(null);
            int BaseX = GetBaseX();
            int BaseY = GetBaseY();
            object HitObject = _BehaviorTree.HitTest(MouseX - BaseX, MouseY - BaseY);
            if (HitObject != null)
            {
                SetObjectSelected(HitObject, true);
                _SelectedObject = HitObject;

                if (_SelectedObject is BTNode)
                {
                    _CurrentNode = _SelectedObject as BTNode;
                    _BehaviorTree.MoveToTop(_CurrentNode);
                    _SavedNodeX = _CurrentNode.X;
                    _SavedNodeY = _CurrentNode.Y;
                    _SavedMouseOffsetX = MouseX - _CurrentNode.X;
                    _SavedMouseOffsetY = MouseY - _CurrentNode.Y;
                    Sender.CaptureMouse();
                    bContinue = false;

                    InspectorUI.SetObjectInspected(_CurrentNode);
                }
                else if (_SelectedObject is BTAuxiliaryNode)
                {
                    InspectorUI.SetObjectInspected(_SelectedObject);
                }
                else if (_SelectedObject is Slot)
                {
                    Slot Slot = _SelectedObject as Slot;
                    _CurrentConnection = new Connection();
                    if (Slot.bOutput)
                    {
                        _CurrentConnection.SetOutSlot(Slot);
                    }
                    else
                    {
                        _CurrentConnection.SetInSlot(Slot);
                    }
                    _CurrentConnection.TempEnd = new Vector2f(MouseX - BaseX, MouseY - BaseY);
                    _CurrentConnection.DoLayout();
                    Sender.CaptureMouse();
                    bContinue = false;
                }
            }
            InspectorUI.InspectObject();
        }

        private void OnPanelLeftMouseUp(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            if (_CurrentNode != null)
            {
                int OldX = _SavedNodeX;
                int OldY = _SavedNodeY;
                int NewX = _CurrentNode.X;
                int NewY = _CurrentNode.Y;
                if (NewX != OldX || NewY != OldY)
                {
                    SetModified();
                }

                Sender.ReleaseMouse();
                _CurrentNode = null;
                bContinue = false;
            }
            if (_CurrentConnection != null)
            {
                int BaseX = GetBaseX();
                int BaseY = GetBaseY();
                object HitObject = _BehaviorTree.HitTest(MouseX - BaseX, MouseY - BaseY);
                if (HitObject != null)
                {
                    if (HitObject is Slot)
                    {
                        Slot Slot = HitObject as Slot;
                        Slot OutSlot = null;
                        Slot InSlot = null;
                        if (_CurrentConnection.InSlot != null)
                        {
                            InSlot = _CurrentConnection.InSlot;
                            OutSlot = Slot;
                        }
                        else if (_CurrentConnection.OutSlot != null)
                        {
                            OutSlot = _CurrentConnection.OutSlot;
                            InSlot = Slot;
                        }
                        else
                        {
                            DebugHelper.Assert(false);
                        }

                        if ((InSlot.bOutput ^ OutSlot.bOutput) == true &&
                            LoopTest(InSlot, OutSlot))
                        {
                            _BehaviorTree.RemoveConnection(_CurrentConnection);

                            // only one chlid node can be connected to the slot of root node
                            // as well as the out slot of parallel node
                            if (OutSlot.Node == _BehaviorTree.RootNode ||
                                (OutSlot.Node as BTNode) is BTComposite_Parallel)
                            {
                                _BehaviorTree.ClearConnectionsOfSlot(OutSlot);
                            }

                            // the main out slot of parallel node can only connect with task node
                            if (!((OutSlot.Node as BTNode)._MainOutSlot == OutSlot && !(InSlot.Node is BTTaskNode)))
                            {
                                _BehaviorTree.ClearConnectionsOfSlot(InSlot);
                                _CurrentConnection.SetOutSlot(OutSlot);
                                _CurrentConnection.SetInSlot(InSlot);
                                _BehaviorTree.AddConnection(_CurrentConnection);
                                _CurrentConnection.DoLayout();
                            }

                            SetModified();
                        }
                        else
                        {
                            InSlot?.RemoveConnection(_CurrentConnection);
                            OutSlot?.RemoveConnection(_CurrentConnection);
                        }
                    }
                }
                Sender.ReleaseMouse();
                _CurrentConnection = null;
                bContinue = false;
            }
        }

        private bool LoopTest(Slot InSlot, Slot OutSlot)
        {
            List<Node> Nodes = new List<Node>();
            Nodes.Add(OutSlot.Node);

            Stack<Node> Stack = new Stack<Node>();
            Node CurrentNode = InSlot.Node;
            Stack.Push(CurrentNode);
            while (Stack.Count != 0)
            {
                CurrentNode = Stack.Pop() as BTNode;
                if (Nodes.Contains(CurrentNode))
                {
                    return false;
                }
                else
                {
                    Nodes.Add(CurrentNode);
                }
                foreach (Node ChildNode in (CurrentNode as BTNode).GetChildNodes())
                {
                    Stack.Push(ChildNode);
                }
            }
            return true;
        }

        private void OnPanelRightMouseDown(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            _bRightMouseDown = true;
            _bMouseMoved = false;
            _SavedMouseOffsetX = MouseX - _BehaviorTree.OffsetX;
            _SavedMouseOffsetY = MouseY - _BehaviorTree.OffsetY;
            _SavedMouseX = MouseX;
            _SavedMouseY = MouseY;
            Sender.CaptureMouse();
        }

        private void OnPanelRightMouseUp(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            if (_bRightMouseDown)
            {
                _bRightMouseDown = false;
                Sender.ReleaseMouse();
                if (_bMouseMoved == false)
                {
                    int BaseX = GetBaseX();
                    int BaseY = GetBaseY();
                    object HitObject = _BehaviorTree.HitTest(MouseX - BaseX, MouseY - BaseY);
                    if (HitObject != null && HitObject is BTNode)
                    {
                        SelectObject(HitObject);
                        _BehaviorTree.MoveToTop(HitObject as BTNode);
                        if (HitObject as BTNode != _BehaviorTree.RootNode)
                        {
                            ShowBTNodeMenu(MouseX, MouseY);
                        }
                    }
                    else if (HitObject != null && HitObject is BTAuxiliaryNode)
                    {
                        SelectObject(HitObject);
                        _BehaviorTree.MoveToTop((HitObject as BTAuxiliaryNode)._ParentNode);
                        ShowBTAuxiliaryMenu(MouseX, MouseY);
                    }
                    else if (HitObject != null && HitObject is Connection)
                    {
                        SelectObject(HitObject);
                        ShowConnectionMenu(MouseX, MouseY);
                    }
                    else if (Sender.IsPointIn(MouseX, MouseY))
                    {
                        ShowContextMenu(MouseX, MouseY);
                    }
                }
                bContinue = false;
            }
        }

        private void ShowContextMenu(int MouseX, int MouseY)
        {
            Menu MenuContextMenu = new Menu();
            MenuContextMenu.Initialize();

            MenuItem MenuItem_Save = new MenuItem();
            MenuItem_Save.SetText("Save");
            MenuItem_Save.ClickedEvent += (Sender) =>
            {
                DoSave();
            };

            #region Composite

            Menu Menu_Node_Composite = new Menu();
            Menu_Node_Composite.Initialize();

            MenuItem MenuItem_Node_Composite_Selector = new MenuItem();
            MenuItem_Node_Composite_Selector.SetText("Selector");
            MenuItem_Node_Composite_Selector.ClickedEvent += (Sender) =>
            {
                AddNodeToMouse(new BTComposite_Selector());
            };
            Menu_Node_Composite.AddMenuItem(MenuItem_Node_Composite_Selector);

            MenuItem MenuItem_Node_Composite_Sequence = new MenuItem();
            MenuItem_Node_Composite_Sequence.SetText("Sequence");
            MenuItem_Node_Composite_Sequence.ClickedEvent += (Sender) =>
            {
                AddNodeToMouse(new BTComposite_Sequence());
            };
            Menu_Node_Composite.AddMenuItem(MenuItem_Node_Composite_Sequence);

            MenuItem MenuItem_Node_Composite_Parallel = new MenuItem();
            MenuItem_Node_Composite_Parallel.SetText("Simple Parallel");
            MenuItem_Node_Composite_Parallel.ClickedEvent += (Sender) =>
            {
                AddNodeToMouse(new BTComposite_Parallel());
            };
            Menu_Node_Composite.AddMenuItem(MenuItem_Node_Composite_Parallel);

            #endregion Composite

            MenuItem MenuItem_Node_Composite = new MenuItem();
            MenuItem_Node_Composite.SetText("Composites");
            MenuItem_Node_Composite.SetMenu(Menu_Node_Composite);

            #region Task

            Menu Menu_Node_Task = new Menu();
            Menu_Node_Task.Initialize();

            MenuItem MenuItem_Node_Task_FinishWithResult = new MenuItem();
            MenuItem_Node_Task_FinishWithResult.SetText("Finish With Result");
            MenuItem_Node_Task_FinishWithResult.ClickedEvent += (Sender) =>
            {
                AddNodeToMouse(new BTTask_FinishWithResult());
            };
            Menu_Node_Task.AddMenuItem(MenuItem_Node_Task_FinishWithResult);

            MenuItem MenuItem_Node_Task_Wait = new MenuItem();
            MenuItem_Node_Task_Wait.SetText("Wait");
            MenuItem_Node_Task_Wait.ClickedEvent += (Sender) =>
            {
                AddNodeToMouse(new BTTask_Wait());
            };
            Menu_Node_Task.AddMenuItem(MenuItem_Node_Task_Wait);

            MenuItem MenuItem_Node_Task_MoveTo = new MenuItem();
            MenuItem_Node_Task_MoveTo.SetText("Move To");
            MenuItem_Node_Task_MoveTo.ClickedEvent += (Sender) =>
            {
                AddNodeToMouse(new BTTask_MoveTo());
            };
            Menu_Node_Task.AddMenuItem(MenuItem_Node_Task_MoveTo);

            MenuItem MenuItem_Node_Task_FindRandomPosition = new MenuItem();
            MenuItem_Node_Task_FindRandomPosition.SetText("Find Random Position");
            MenuItem_Node_Task_FindRandomPosition.ClickedEvent += (Sender) =>
            {
                AddNodeToMouse(new BTTask_FindRandomPosition());
            };
            Menu_Node_Task.AddMenuItem(MenuItem_Node_Task_FindRandomPosition);

            MenuItem MenuItem_Node_Task_ChasePlayer = new MenuItem();
            MenuItem_Node_Task_ChasePlayer.SetText("Chase Player");
            MenuItem_Node_Task_ChasePlayer.ClickedEvent += (Sender) =>
            {
                AddNodeToMouse(new BTTask_ChasePlayer());
            };
            Menu_Node_Task.AddMenuItem(MenuItem_Node_Task_ChasePlayer);

            #endregion Task

            MenuItem MenuItem_Node_Task = new MenuItem();
            MenuItem_Node_Task.SetText("Tasks");
            MenuItem_Node_Task.SetMenu(Menu_Node_Task);

            MenuContextMenu.AddMenuItem(MenuItem_Save);
            MenuContextMenu.AddMenuItem(MenuItem_Node_Composite);
            MenuContextMenu.AddMenuItem(MenuItem_Node_Task);

            ContextMenu.GetInstance().SetForm(MainUI.GetInstance().MainWindow);
            ContextMenu.GetInstance().ShowMenu(MenuContextMenu, MouseX, MouseY);
        }

        private void ShowBTNodeMenu(int MouseX, int MouseY)
        {
            Menu MenuContextMenu = new Menu();
            MenuContextMenu.Initialize();

            MenuItem MenuItem_Save = new MenuItem();
            MenuItem_Save.SetText("Save");

            MenuItem MenuItem_Duplicate = new MenuItem();
            MenuItem_Duplicate.SetText("Duplicate");

            MenuItem MenuItem_Delete = new MenuItem();
            MenuItem_Delete.SetText("Delete");
            MenuItem_Delete.ClickedEvent += (Sender) =>
            {
                DoDelete();
            };

            #region Decorator

            Menu Menu_Node_Decorator = new Menu();
            Menu_Node_Decorator.Initialize();

            MenuItem Decorator_BlackboardBasedCondition = new MenuItem();
            Decorator_BlackboardBasedCondition.SetText("Blackboard Based Condition");
            Decorator_BlackboardBasedCondition.ClickedEvent += (MenuItem Sender) =>
            {
                AddAuxiliaryNodeTo(new BTDecorator_BlackboardBased(), _SelectedObject as BTNode);
            };
            Menu_Node_Decorator.AddMenuItem(Decorator_BlackboardBasedCondition);

            #endregion Decorator

            MenuItem MenuItem_Node_Decorator = new MenuItem();
            MenuItem_Node_Decorator.SetText("Decorators");
            MenuItem_Node_Decorator.SetMenu(Menu_Node_Decorator);

            #region Service

            Menu Menu_Node_Service = new Menu();
            Menu_Node_Service.Initialize();

            MenuItem MenuItem_Node_Service_SetDefaultFocus = new MenuItem();
            MenuItem_Node_Service_SetDefaultFocus.SetText("Set Default Focus");
            MenuItem_Node_Service_SetDefaultFocus.ClickedEvent += (Sender) =>
            {
                AddAuxiliaryNodeTo(new BTService_SetDefaultFocus(), _SelectedObject as BTNode);
            };
            Menu_Node_Service.AddMenuItem(MenuItem_Node_Service_SetDefaultFocus);

            MenuItem MenuItem_Node_Service_FindPlayer = new MenuItem();
            MenuItem_Node_Service_FindPlayer.SetText("Find Player");
            MenuItem_Node_Service_FindPlayer.ClickedEvent += (Sender) =>
            {
                AddAuxiliaryNodeTo(new BTService_FindPlayer(), _SelectedObject as BTNode);
            };
            Menu_Node_Service.AddMenuItem(MenuItem_Node_Service_FindPlayer);

            #endregion Service

            MenuItem MenuItem_Node_Service = new MenuItem();
            MenuItem_Node_Service.SetText("Services");
            MenuItem_Node_Service.SetMenu(Menu_Node_Service);

            MenuContextMenu.AddMenuItem(MenuItem_Save);
            MenuContextMenu.AddMenuItem(MenuItem_Duplicate);
            MenuContextMenu.AddMenuItem(MenuItem_Delete);
            MenuContextMenu.AddMenuItem(MenuItem_Node_Decorator);
            MenuContextMenu.AddMenuItem(MenuItem_Node_Service);

            ContextMenu.GetInstance().SetForm(MainUI.GetInstance().MainWindow);
            ContextMenu.GetInstance().ShowMenu(MenuContextMenu, MouseX, MouseY);
        }

        private void ShowBTAuxiliaryMenu(int MouseX, int MouseY)
        {
            Menu MenuContextMenu = new Menu();
            MenuContextMenu.Initialize();

            MenuItem MenuItem_Delete = new MenuItem();
            MenuItem_Delete.SetText("Delete");
            MenuItem_Delete.ClickedEvent += (MenuItem Sender) =>
            {
                RemoveAuxiliaryNodeFrom(_SelectedObject as BTAuxiliaryNode);
            };

            MenuContextMenu.AddMenuItem(MenuItem_Delete);

            ContextMenu.GetInstance().SetForm(MainUI.GetInstance().MainWindow);
            ContextMenu.GetInstance().ShowMenu(MenuContextMenu, MouseX, MouseY);
        }

        private void ShowConnectionMenu(int MouseX, int MouseY)
        {
            Menu MenuContextMenu = new Menu();
            MenuContextMenu.Initialize();

            MenuItem MenuItem_Delete = new MenuItem();
            MenuItem_Delete.SetText("Delete");
            MenuItem_Delete.ClickedEvent += (MenuItem Sender) =>
            {
                DoDelete();
            };

            MenuContextMenu.AddMenuItem(MenuItem_Delete);

            ContextMenu.GetInstance().SetForm(MainUI.GetInstance().MainWindow);
            ContextMenu.GetInstance().ShowMenu(MenuContextMenu, MouseX, MouseY);
        }

        private void OnPanelMouseMove(Control Sender, int MouseX, int MouseY, ref bool bContinue)
        {
            if (_CurrentNode != null)
            {
                _CurrentNode.X = MouseX - _SavedMouseOffsetX;
                _CurrentNode.Y = MouseY - _SavedMouseOffsetY;
                _CurrentNode.DoLayoutWithConnections();
                SetModified();
            }

            if (_CurrentConnection != null)
            {
                int BaseX = GetBaseX();
                int BaseY = GetBaseY();
                _CurrentConnection.TempEnd = new Vector2f(MouseX - BaseX, MouseY - BaseY);
                _CurrentConnection.DoLayout();
            }

            if (_bRightMouseDown)
            {
                _BehaviorTree.OffsetX = MouseX - _SavedMouseOffsetX;
                _BehaviorTree.OffsetY = MouseY - _SavedMouseOffsetY;
                int Distance = Math.Abs(MouseX - _SavedMouseX) + Math.Abs(MouseY - _SavedMouseY);
                if (Distance > 3)
                {
                    _bMouseMoved = true;
                }
            }
        }

        public void DoDuplicate()
        {
            if (_CurrentNode != null ||
                _CurrentConnection != null)
            {
                return;
            }
            if (_SelectedObject is BTNode)
            {
                BTNode BTNode = _SelectedObject as BTNode;

                Record RecordNode = new Record();
                BTNode.SaveToXml(RecordNode);

                BTNode BTNode1 = _BehaviorTree.LoadNode(RecordNode) as BTNode;
                AddNodeTo(BTNode1, BTNode.X + 10, BTNode.Y + 10);

                SetModified();
            }
        }

        public void DoDeleteNode(Node Node)
        {
            _BehaviorTree.RemoveNode(Node as BTNode);

            SetModified();
        }

        public void DoDelete()
        {
            if (_CurrentNode != null ||
                _CurrentConnection != null)
            {
                return;
            }
            if (_SelectedObject is BTNode)
            {
                BTNode Node = _SelectedObject as BTNode;
                DoDeleteNode(Node);
            }
            else if (_SelectedObject is Slot)
            {
                Slot Slot = _SelectedObject as Slot;
                DoDeleteNode(Slot.Node);
            }
            else if (_SelectedObject is Connection)
            {
                Connection Connection = _SelectedObject as Connection;
                _BehaviorTree.RemoveConnection(Connection);

                SetModified();
            }
            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(null);
            InspectorUI.InspectObject();
        }

        public void OnDeleteKeyDown()
        {
            DoDelete();
        }

        public void SelectObject(object Object)
        {
            if (_SelectedObject != null)
            {
                SetObjectSelected(_SelectedObject, false);
                _SelectedObject = null;
            }
            SetObjectSelected(Object, true);
            _SelectedObject = Object;
        }

        private void AddNodeTo(BTNode BTNode, int X, int Y)
        {
            _BehaviorTree.AddNode(BTNode);
            SelectObject(BTNode);
            BTNode.X = X;
            BTNode.Y = Y;
            BTNode.DoLayoutWithConnections();

            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(BTNode);
            InspectorUI.InspectObject();
        }

        private void AddNodeToMouse(BTNode Node)
        {
            int BaseX = GetBaseX();
            int BaseY = GetBaseY();
            int X = _SavedMouseX - BaseX;
            int Y = _SavedMouseY - BaseY;
            AddNodeTo(Node, X, Y);

            SetModified();
        }

        private void AddAuxiliaryNodeTo(BTAuxiliaryNode Node, BTNode BTNode)
        {
            _BehaviorTree.AddAuxiliaryNode(Node, BTNode);
            SelectObject(Node);
            BTNode.DoLayoutWithConnections();

            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(Node);
            InspectorUI.InspectObject();

            SetModified();
        }

        private void RemoveAuxiliaryNodeFrom(BTAuxiliaryNode Node)
        {
            BTNode BTNode = Node._ParentNode;
            _BehaviorTree.RemoveAuxiliaryNode(Node, BTNode);
            SelectObject(BTNode);
            BTNode.DoLayoutWithConnections();

            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(BTNode);
            InspectorUI.InspectObject();

            SetModified();
        }

        public override void DoSave()
        {
            if (_FileName != "")
            {
                _BehaviorTree.SaveToXml(_FileName);
            }
            ClearModified();
        }

        public void LocateToNode(int NodeID)
        {
            Node Node = _BehaviorTree.FindNodeByID(NodeID);
            if (Node != null)
            {
                _BehaviorTree.OffsetX = -(Node.X + Node.Width / 2) + _Panel.GetWidth() / 2;
                _BehaviorTree.OffsetY = -(Node.Y + Node.Height / 2) + _Panel.GetHeight() / 2;
                SelectObject(Node);
            }
        }
    }
}