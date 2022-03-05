using EditorUI;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrossEditor
{
    internal class BehaviorTree
    {
        public int OffsetX;
        public int OffsetY;

        public int NodeID;
        public int ConnectionID;
        public int AuxiliaryID;

        private List<BTNode> BTNodes;
        private List<BTAuxiliaryNode> BTAuxiliaryNodes;
        private List<Connection> Connections;

        public BTNode RootNode;

        private static Random Random = new Random();

        public BlackboardData Blackboard
        {
            get
            {
                if(RootNode == null)
                {
                    return null;
                }
                else
                {
                    return (RootNode as BTComposite_Root)._Blackboard;
                }
            }
        }

        public BehaviorTree()
        {
            OffsetX = 0;
            OffsetY = 0;
            NodeID = 10000;
            ConnectionID = 20000;
            AuxiliaryID = 30000;
            BTNodes = new List<BTNode>();
            BTAuxiliaryNodes = new List<BTAuxiliaryNode>();
            Connections = new List<Connection>();
            RootNode = null;
        }

        public bool HasBTNode(BTNode BTNode)
        {
            return BTNodes.Contains(BTNode);
        }

        public bool HasBTAuxiliaryNode(BTAuxiliaryNode BTAuxiliaryNode)
        {
            return BTAuxiliaryNodes.Contains(BTAuxiliaryNode);
        }

        public void ClearError()
        {
            foreach (BTNode BTNode in BTNodes)
            {
                BTNode.ClearError();
            }
            foreach (BTAuxiliaryNode BTAuxiliaryNode in BTAuxiliaryNodes)
            {
                BTAuxiliaryNode.ClearError();
            }
        }

        public void ClearExecute()
        {
            foreach (BTNode BTNode in BTNodes)
            {
                BTNode._bExecuting = false;
            }
        }

        public void AddNode(BTNode BTNode)
        {
            if (BTNodes.Contains(BTNode) == false)
            {
                BTNode.ID = NodeID;
                BTNodes.Add(BTNode);
                NodeID += Random.Next(1, 20);
            }
        }

        public void AddNodeDirectly(BTNode BTNode)
        {
            if (BTNodes.Contains(BTNode) == false)
            {
                BTNodes.Add(BTNode);
            }
        }

        public void AddAuxiliaryNode(BTAuxiliaryNode Node, BTNode BTNode)
        {
            if(BTAuxiliaryNodes.Contains(Node) == false)
            {
                Node.ID = AuxiliaryID;
                BTNode.AddAuxiliaryNode(Node);
                BTAuxiliaryNodes.Add(Node);
                AuxiliaryID += Random.Next(1, 20);
            }
        }

        public void RemoveAuxiliaryNode(BTAuxiliaryNode Node, BTNode BTNode)
        {
            BTNode.RemoveAuxiliaryNode(Node);
            if (BTAuxiliaryNodes.Contains(Node))
            {
                BTAuxiliaryNodes.Remove(Node);
            }
        }

        public void RemoveNode(BTNode BTNode)
        {
            if(BTNode.GetInSlot() != null)
            {
                ClearConnectionsOfSlot(BTNode.GetInSlot());
            }
            if(BTNode.GetOutSlot() != null)
            {
                ClearConnectionsOfSlot(BTNode.GetOutSlot());
            }
            if(BTNode.GetMainOutSlot() != null)
            {
                ClearConnectionsOfSlot(BTNode.GetMainOutSlot());
            }

            List<BTAuxiliaryNode> Decorators = BTNode.GetDecorators();
            for (int i = Decorators.Count - 1; i >= 0; i--)
            {
                RemoveAuxiliaryNode(Decorators[i], BTNode);
            }

            List<BTAuxiliaryNode> Services = BTNode.GetServices();
            for (int i = Services.Count - 1; i >= 0; i--)
            {
                RemoveAuxiliaryNode(Services[i], BTNode);
            }

            if (BTNodes.Contains(BTNode))
            {
                BTNodes.Remove(BTNode);
            }
        }

        public void MoveToTop(BTNode BTNode)
        {
            BTNodes.Remove(BTNode);
            BTNodes.Add(BTNode);
        }

        public BTNode FindNodeByID(int NodeID)
        {
            foreach (BTNode BTNode in BTNodes)
            {
                if (BTNode.ID == NodeID)
                {
                    return BTNode;
                }
            }
            return null;
        }

        public void ClearConnectionsOfSlot(Slot Slot)
        {
            List<Connection> Connections = Slot.GetConnections();
            for (int i = Connections.Count - 1; i >= 0; i--)
            {
                RemoveConnection(Connections[i]);
            }
        }

        public void RemoveConnection(Connection Connection)
        {
            if (Connection.InSlot != null)
            {
                Connection.InSlot.RemoveConnection(Connection);
            }
            if (Connection.OutSlot != null)
            {
                Connection.OutSlot.RemoveConnection(Connection);
            }
            Connections.Remove(Connection);
        }

        public void AddConnection(Connection Connection)
        {
            DebugHelper.Assert(Connection != null);
            DebugHelper.Assert(Connection.InSlot != null);
            DebugHelper.Assert(Connection.OutSlot != null);
            if (Connections.Contains(Connection) == false)
            {
                Connection.ID = ConnectionID;
                Connections.Add(Connection);
                ConnectionID++;
            }
        }

        public void AddConnectionDirectly(Connection Connection)
        {
            DebugHelper.Assert(Connection != null);
            DebugHelper.Assert(Connection.InSlot != null);
            DebugHelper.Assert(Connection.OutSlot != null);
            if (Connections.Contains(Connection) == false)
            {
                Connections.Add(Connection);
            }
        }

        public Connection FindConnectionByID(int ConnectionID)
        {
            foreach (Connection Connection in Connections)
            {
                if (Connection.ID == ConnectionID)
                {
                    return Connection;
                }
            }
            return null;
        }

        public void DoLayout()
        {
            foreach (BTNode BTNode in BTNodes)
            {
                BTNode.DoLayout();
            }
            foreach (Connection Connection in Connections)
            {
                Connection.DoLayout();
            }
        }

        public int CalcExecuteIndex(BTNode BTNode, int ExecuteIndex)
        {
            foreach(BTAuxiliaryNode Node in BTNode.GetDecorators())
            {
                Node._ExecuteIndex = ExecuteIndex;
                ExecuteIndex++;
            }

            BTNode._ExecuteIndex = ExecuteIndex;
            ExecuteIndex++;

            foreach(BTAuxiliaryNode Node in BTNode.GetServices())
            {
                Node._ExecuteIndex = ExecuteIndex;
                ExecuteIndex++;
            }

            if(BTNode is BTComposite_Parallel)
            {
                foreach(BTNode MainChildNode in BTNode.GetMainChildNodes())
                {
                    ExecuteIndex = CalcExecuteIndex(MainChildNode, ExecuteIndex);
                }
            }

            List<BTNode> ChildNodes = BTNode.GetChildNodes();
            ChildNodes.Sort((left, right) =>
            {
                if (left.Y != right.Y)
                {
                    return left.Y.CompareTo(right.Y);
                }
                else
                {
                    if (left.X != right.X)
                    {
                        return left.X.CompareTo(right.X);
                    }
                    else
                    {
                        return left.ID.CompareTo(right.ID);
                    }
                }
            });
            for (int i = 0; i < ChildNodes.Count; i++)
            {
                ExecuteIndex = CalcExecuteIndex(ChildNodes[i] as BTNode, ExecuteIndex);
            }

            return ExecuteIndex;
        }

        public void ResetExecuteIndex()
        {
            foreach (BTNode BTNode in BTNodes)
            {
                foreach (BTAuxiliaryNode Node in BTNode.GetDecorators())
                {
                    Node._ExecuteIndex = -1;
                }

                BTNode._ExecuteIndex = -1;

                foreach (BTAuxiliaryNode Node in BTNode.GetServices())
                {
                    Node._ExecuteIndex = -1;
                }
            }
        }

        public void Draw()
        {
            ResetExecuteIndex();
            if(RootNode != null)
            {
                CalcExecuteIndex(RootNode, 0);
            }
            foreach (Connection Connection in Connections)
            {
                Connection.Draw();
            }
            foreach (BTNode BTNode in BTNodes)
            {
                BTNode.Draw();
            }
        }

        public object HitTest(int MouseX, int MouseY)
        {
            int Count = BTNodes.Count;
            for (int i = Count - 1; i >= 0; i--)
            {
                BTNode BTNode = BTNodes[i];
                object HitObject = BTNode.HitTest(MouseX, MouseY);
                if (HitObject != null)
                {
                    return HitObject;
                }
            }
            Count = Connections.Count;
            for (int i = Count - 1; i >= 0; i--)
            {
                Connection Connection = Connections[i];
                object HitObject = Connection.HitTest(MouseX, MouseY);
                if (HitObject != null)
                {
                    return HitObject;
                }
            }
            return null;
        }

        public void SaveToXml(string Filename)
        {
            BTNodes.Sort((BTNode BTNode1, BTNode BTNode2) =>
            {
                return BTNode1.ID.CompareTo(BTNode2.ID);
            });

            Connections.Sort((Connection Connection1, Connection Connection2) =>
            {
                return Connection1.ID.CompareTo(Connection2.ID);
            });

            BTAuxiliaryNodes.Sort((Node1, Node2) =>
            {
                return Node1.ID.CompareTo(Node2.ID);
            });

            XmlScript Xml = new XmlScript();
            Record RootRecord = Xml.GetRootRecord();

            Record RecordBehaviorTree = RootRecord.AddChild();
            RecordBehaviorTree.SetTypeString("BehaviorTree");

            RecordBehaviorTree.SetInt("OffsetX", OffsetX);
            RecordBehaviorTree.SetInt("OffsetY", OffsetY);
            RecordBehaviorTree.SetInt("NodeID", NodeID);
            RecordBehaviorTree.SetInt("ConnectionID", ConnectionID);
            RecordBehaviorTree.SetInt("AuxiliaryID", AuxiliaryID);

            foreach (BTNode BTNode in BTNodes)
            {
                Record RecordBTNode = RecordBehaviorTree.AddChild();
                BTNode.SaveToXml(RecordBTNode);
            }
            foreach (Connection Connection in Connections)
            {
                Record RecordConnection = RecordBehaviorTree.AddChild();
                Connection.SaveToXml(RecordConnection);
            }
            foreach (BTAuxiliaryNode Node in BTAuxiliaryNodes)
            {
                Record RecordNode = RecordBehaviorTree.AddChild();
                Node.SaveToXml(RecordNode);
            }
            Xml.Save(Filename);
        }

        public object LoadNode(Record RecordNode)
        {
            string TypeString = RecordNode.GetTypeString();
            BTNode BTNode = null;
            BTAuxiliaryNode AuxiliaryNode = null;
            if(TypeString == "BTComposite_Root")
            {
                BTNode = new BTComposite_Root();
                RootNode = BTNode;
            }
            else if (TypeString == "BTComposite_Selector")
            {
                BTNode = new BTComposite_Selector();
            }
            else if (TypeString == "BTComposite_Sequence")
            {
                BTNode = new BTComposite_Sequence();
            }
            else if (TypeString == "BTComposite_Parallel")
            {
                BTNode = new BTComposite_Parallel();
            }
            else if (TypeString == "BTTask_FinishWithResult")
            {
                BTNode = new BTTask_FinishWithResult();
            }
            else if (TypeString == "BTTask_Wait")
            {
                BTNode = new BTTask_Wait();
            }
            else if (TypeString == "BTTask_MoveTo")
            {
                BTNode = new BTTask_MoveTo();
            }
            else if (TypeString == "BTTask_FindRandomPosition")
            {
                BTNode = new BTTask_FindRandomPosition();
            }
            else if (TypeString == "BTTask_ChasePlayer")
            {
                BTNode = new BTTask_ChasePlayer();
            }
            else if (TypeString == "BTDecorator_BlackboardBased")
            {
                AuxiliaryNode = new BTDecorator_BlackboardBased();
            }
            else if (TypeString == "BTService_SetDefaultFocus")
            {
                AuxiliaryNode = new BTService_SetDefaultFocus();
            }
            else if (TypeString == "BTService_FindPlayer")
            {
                AuxiliaryNode = new BTService_FindPlayer();
            }

            BTNode?.LoadFromXml(RecordNode);
            AuxiliaryNode?.LoadFromXml(RecordNode, this);
            return (object)BTNode ?? AuxiliaryNode;
        }

        public void LoadFromXml(string Filename)
        {
            BTNodes.Clear();
            Connections.Clear();
            BTAuxiliaryNodes.Clear();

            if (File.Exists(Filename) == false)
            {
                return;
            }

            string Content = FileHelper.ReadTextFile(Filename);
            if (Content == "")
            {
                // when file is empty, add root node at once
                BTNode RootNode = new BTComposite_Root();
                RootNode.X = 500;
                RootNode.Y = 500;
                RootNode.DoLayoutWithConnections();
                AddNode(RootNode);
                this.RootNode = RootNode;
            }
            else
            {
                XmlScript Xml = new XmlScript();
                Xml.Open(Filename);
                Record RootRecord = Xml.GetRootRecord();

                Record RecordBehaviorTree = RootRecord.FindByTypeString("BehaviorTree");
                if (RecordBehaviorTree != null)
                {
                    OffsetX = RecordBehaviorTree.GetInt("OffsetX");
                    OffsetY = RecordBehaviorTree.GetInt("OffsetY");
                    NodeID = RecordBehaviorTree.GetInt("NodeID");
                    ConnectionID = RecordBehaviorTree.GetInt("ConnectionID");
                    AuxiliaryID = RecordBehaviorTree.GetInt("AuxiliaryID");

                    int Count = RecordBehaviorTree.GetChildCount();
                    for (int i = 0; i < Count; i++)
                    {
                        Record RecordChild = RecordBehaviorTree.GetChild(i);
                        string TypeString = RecordChild.GetTypeString();
                        if (TypeString == "Connection")
                        {
                            Connection Connection = new Connection();
                            Connection.LoadFromXml(RecordChild, this);
                            if (Connection.OutSlot != null && Connection.InSlot != null)
                            {
                                Connections.Add(Connection);
                            }
                        }
                        else
                        {
                            Record RecordNode = RecordChild;
                            object Node = LoadNode(RecordNode);

                            if (Node is BTNode)
                            {
                                BTNodes.Add(Node as BTNode);
                            }
                            if (Node is BTAuxiliaryNode)
                            {
                                BTAuxiliaryNodes.Add(Node as BTAuxiliaryNode);
                            }
                        }
                    }
                }
            }
        }

        public Slot FindSlotByName(int NodeID, string SlotName)
        {
            BTNode BTNode = FindNodeByID(NodeID);
            if(BTNode != null)
            {
                return BTNode.FindSlot(SlotName);
            }
            return null;
        }
    }
}