using EditorUI;
using System;
using System.Collections.Generic;
using System.IO;

namespace Editor
{
    public class NodeGraph
    {
        public int OffsetX;
        public int OffsetY;

        public int NodeID;
        public int ConnectionID;

        private List<Node> Nodes;
        private List<Connection> Connections;

        private static Random Random = new Random();

        public NodeGraph()
        {
            OffsetX = 0;
            OffsetY = 0;
            NodeID = 10000;
            ConnectionID = 20000;
            Nodes = new List<Node>();
            Connections = new List<Connection>();
        }

        public void ClearError()
        {
            foreach (Node Node in Nodes)
            {
                Node.ClearError();
            }
        }

        public void AddNode(Node Node)
        {
            if (Nodes.Contains(Node) == false)
            {
                Node.ID = NodeID;
                Nodes.Add(Node);
                NodeID += Random.Next(1, 20);
            }
        }

        public void AddNodeDirectly(Node Node)
        {
            if (Nodes.Contains(Node) == false)
            {
                Nodes.Add(Node);
            }
        }

        public void RemoveNode(Node Node)
        {
            foreach (Slot InSlot in Node.GetInSlots())
            {
                ClearConnectionsOfSlot(InSlot);
            }
            foreach (Slot OutSlot in Node.GetOutSlots())
            {
                ClearConnectionsOfSlot(OutSlot);
            }
            if (Nodes.Contains(Node))
            {
                Nodes.Remove(Node);
            }
        }

        public void MoveToTop(Node Node)
        {
            Nodes.Remove(Node);
            Nodes.Add(Node);
        }

        public Node FindNodeByID(int NodeID)
        {
            foreach (Node Node in Nodes)
            {
                if (Node.ID == NodeID)
                {
                    return Node;
                }
            }
            return null;
        }

        public Slot FindInSlotByName(int InSlotNodeID, string InSlotName)
        {
            Node Node = FindNodeByID(InSlotNodeID);
            if (Node != null)
            {
                return Node.FindInSlot(InSlotName);
            }
            return null;
        }

        public Slot FindOutSlotByName(int OutSlotNodeID, string OutSlotName)
        {
            Node Node = FindNodeByID(OutSlotNodeID);
            if (Node != null)
            {
                return Node.FindOutSlot(OutSlotName);
            }
            return null;
        }

        public List<T> Clone<T>(List<T> List)
        {
            using (System.IO.Stream ObjectStream = new MemoryStream())
            {
                System.Runtime.Serialization.IFormatter Formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                Formatter.Serialize(ObjectStream, List);
                ObjectStream.Seek(0, SeekOrigin.Begin);
                return Formatter.Deserialize(ObjectStream) as List<T>;
            }
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

        public FlowNode_Event FindEventNode(string EventName)
        {
            foreach (Node Node in Nodes)
            {
                if (Node is FlowNode_Event)
                {
                    FlowNode_Event FlowNode_Event = Node as FlowNode_Event;
                    if (FlowNode_Event.GetEventName() == EventName)
                    {
                        return FlowNode_Event;
                    }
                }
            }
            return null;
        }

        public List<Node> FindRunBehaviorTreeNode()
        {
            List<Node> Result = new List<Node>();
            foreach (Node Node in Nodes)
            {
                if(Node is FlowNode_RunBehaviorTree)
                {
                    Result.Add(Node);
                }
            }
            return Result;
        }

        public void DoLayout()
        {
            foreach (Node Node in Nodes)
            {
                Node.DoLayout();
            }
            foreach (Connection Connection in Connections)
            {
                Connection.DoLayout();
            }
        }

        public void Draw()
        {
            foreach (Connection Connection in Connections)
            {
                Connection.Draw();
            }
            foreach (Node Node in Nodes)
            {
                Node.Draw();
            }
        }

        public object HitTest(int MouseX, int MouseY)
        {
            int Count = Nodes.Count;
            for (int i = Count - 1; i >= 0; i--)
            {
                Node Node = Nodes[i];
                object HitObject = Node.HitTest(MouseX, MouseY);
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
            Nodes.Sort((Node Node1, Node Node2) =>
            {
                return Node1.ID.CompareTo(Node2.ID);
            });

            Connections.Sort((Connection Connection1, Connection Connection2) =>
            {
                return Connection1.ID.CompareTo(Connection2.ID);
            });

            XmlScript Xml = new XmlScript();
            Record RootRecord = Xml.GetRootRecord();

            Record RecordNodeGraph = RootRecord.AddChild();
            RecordNodeGraph.SetTypeString("NodeGraph");

            RecordNodeGraph.SetInt("OffsetX", OffsetX);
            RecordNodeGraph.SetInt("OffsetY", OffsetY);
            RecordNodeGraph.SetInt("NodeID", NodeID);
            RecordNodeGraph.SetInt("ConnectionID", ConnectionID);

            foreach (Node Node in Nodes)
            {
                Record RecordNode = RecordNodeGraph.AddChild();
                Node.SaveToXml(RecordNode);
            }
            foreach (Connection Connection in Connections)
            {
                Record RecordConnection = RecordNodeGraph.AddChild();
                Connection.SaveToXml(RecordConnection);
            }
            Xml.Save(Filename);
        }

        public Node LoadNode(Record RecordNode)
        {
            string TypeString = RecordNode.GetTypeString();
            Node Node = null;
            if (TypeString == "FlowNode_Bool")
            {
                Node = new FlowNode_Bool(false);
            }
            else if (TypeString == "FlowNode_Integer")
            {
                Node = new FlowNode_Integer(0);
            }
            else if (TypeString == "FlowNode_Float")
            {
                Node = new FlowNode_Float(0.0f);
            }
            else if (TypeString == "FlowNode_String")
            {
                Node = new FlowNode_String("");
            }
            else if (TypeString == "FlowNode_UnaryArithOp")
            {
                Node = new FlowNode_UnaryArithOp(UnaryArithOp.Negative);
            }
            else if (TypeString == "FlowNode_UnaryLogicOp")
            {
                Node = new FlowNode_UnaryLogicOp(UnaryLogicOp.Not);
            }
            else if (TypeString == "FlowNode_BinaryArithOp")
            {
                Node = new FlowNode_BinaryArithOp(BinaryArithOp.Add);
            }
            else if (TypeString == "FlowNode_BinaryLogicOp")
            {
                Node = new FlowNode_BinaryLogicOp(BinaryLogicOp.And);
            }
            else if (TypeString == "FlowNode_Compare")
            {
                Node = new FlowNode_Compare(Relation.EqualTo);
            }
            else if (TypeString == "FlowNode_ToInt")
            {
                Node = new FlowNode_ToInt();
            }
            else if (TypeString == "FlowNode_ToFloat")
            {
                Node = new FlowNode_ToFloat();
            }
            else if (TypeString == "FlowNode_ToString")
            {
                Node = new FlowNode_ToString();
            }
            else if (TypeString == "FlowNode_If")
            {
                Node = new FlowNode_If();
            }
            else if (TypeString == "FlowNode_WhileLoop")
            {
                Node = new FlowNode_WhileLoop();
            }
            else if (TypeString == "FlowNode_ForLoop")
            {
                Node = new FlowNode_ForLoop();
            }
            else if (TypeString == "FlowNode_PrintString")
            {
                Node = new FlowNode_PrintString();
            }
            else if (TypeString == "FlowNode_Event")
            {
                Node = new FlowNode_Event("");
            }
            else if (TypeString == "FlowNode_RunBehaviorTree")
            {
                Node = new FlowNode_RunBehaviorTree();
            }
            else
            {
                DebugHelper.Assert(false);
            }
            Node.LoadFromXml(RecordNode);
            return Node;
        }

        public void LoadFromXml(string Filename)
        {
            Nodes.Clear();
            Connections.Clear();

            if (File.Exists(Filename) == false)
            {
                return;
            }

            string Content = FileHelper.ReadTextFile(Filename);
            if (Content == "")
            {
                return;
            }

            XmlScript Xml = new XmlScript();
            Xml.Open(Filename);
            Record RootRecord = Xml.GetRootRecord();

            Record RecordNodeGraph = RootRecord.FindByTypeString("NodeGraph");
            if (RecordNodeGraph != null)
            {
                OffsetX = RecordNodeGraph.GetInt("OffsetX");
                OffsetY = RecordNodeGraph.GetInt("OffsetY");
                NodeID = RecordNodeGraph.GetInt("NodeID");
                ConnectionID = RecordNodeGraph.GetInt("ConnectionID");

                int Count = RecordNodeGraph.GetChildCount();
                for (int i = 0; i < Count; i++)
                {
                    Record RecordChild = RecordNodeGraph.GetChild(i);
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
                        Node Node = LoadNode(RecordNode);
                        Nodes.Add(Node);
                    }
                }
            }
            DoLayout();
        }
    }
}