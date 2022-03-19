using System;
using EditorUI;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace CrossEditor
{
    public class NodeComparer:IComparer<Node>
    {
        public int Compare(Node Left, Node Right)
        {
            return Left.ID.CompareTo(Right.ID);
        }
    }

    public enum ConnectCondition
    {
        Allow,
        NotAllow,
        NeedBreakInSlot,
        NeedBreakOutSlot,
        NeedBreakBothSlots,
        NeedConversion
    }

    public struct SlotConnectionResponse
    {
        public string Message;
        public ConnectCondition Condition;
        public SlotConnectionResponse(string Message, ConnectCondition Condition)
        {
            this.Message = Message;
            this.Condition = Condition;
        }
    }

    public class NodeGraphModel
    {
        public NodeGraphView View;
        public Node Owner;

        public string Name;

        public List<Node> Nodes;
        public List<Connection> Connections;

        public int NodeID;
        public int ConnectionID;

        public int CommentCount;
        public int GridID;

        public static Random Random = new Random();
        public static NodeComparer Comparer = new NodeComparer();

        public GraphCamera2D Camera;

        public NodeGraphType _Type;
        public NodeGraphType Type
        {
            get => _Type;
            set => _Type = value;
        }

        public NodeGraphView GetView()
        {
            return View;
        }

        public Node GetOwner()
        {
            return Owner;
        }

        public NodeGraphModel()
        {
            Name = "Name";
            Nodes = new List<Node>();
            Connections = new List<Connection>();
            NodeID = 10000;
            ConnectionID = 20000;
            CommentCount = 0;
            GridID = GraphicsHelper.GetInstance().QueryGridID();
            Camera = new GraphCamera2D();
        }

        public virtual void Draw()
        {
            foreach (Node Node in Nodes)
            {
                Node.DoLayoutWithConnections();
            }
            for (int i = 0; i < CommentCount; ++i)
            {
                Nodes[i].Draw();
            }
            foreach (Connection Connection in Connections)
            {
                Connection.Draw();
            }
            for (int i = CommentCount; i < Nodes.Count; ++i)
            {
                Nodes[i].Draw();
            }
        }

        public virtual object HitTest(int WorldX, int WorldY)
        {
            for (int i = Nodes.Count - 1; i >= 0; --i)
            {
                Node Node = Nodes[i];
                object HitObject = Node.HitTest(WorldX, WorldY);

                if (HitObject != null)
                {
                    return HitObject;
                }
            }

            return null;
        }

        public object HitCommentBorderTest(int WorldX, int WorldY)
        {
            for (int i = CommentCount - 1; i >= 0; --i)
            {
                Node_Comment Comment = Nodes[i] as Node_Comment;
                if (Comment.HitBorderTest(WorldX, WorldY))
                {
                    return Comment;
                }
            }

            return null;
        }

        public virtual List<Node> BoxSelect(int X, int Y, int Width, int Height)
        {
            List<Node> SelectedNodes = new List<Node>();

            foreach (Node Node in Nodes)
            {
                if (Node.RectInRect(X, Y, Width, Height))
                {
                    SelectedNodes.Add(Node);
                }
            }

            return SelectedNodes;
        }

        #region Node

        public int GenerateNodeID()
        {
            int ID = NodeID;
            NodeID += Random.Next(1, 20);
            return ID;
        }

        public Node CreateNode(string Type)
        {
            Node NewNode = (Node)NodeRegister.GetInstance().CreateNode(Type);

            return NewNode;
        }

        void AddNodeOrdered(Node Node)
        {
            if (Nodes.Contains(Node))
            {
                return;
            }

            if (Node is Node_Comment)
            {
                Nodes.Insert(CommentCount, Node);
                CommentCount++;
                Nodes.Sort(0, CommentCount, Comparer);
            }
            else
            {
                Nodes.Add(Node);
                Nodes.Sort(CommentCount, Nodes.Count - CommentCount, Comparer);
            }
        }

        public void AddNode(Node Node)
        {
            AddNodeOrdered(Node);
        }

        /// <summary>
        /// Generate ID for node
        /// </summary>
        /// <param name="Node"></param>
        public void ImportNode(Node Node)
        {
            Node.ID = GenerateNodeID();
            Node.SetOwner(this);
            AddNode(Node);
        }

        public void RemoveNode(Node Node)
        {
            if (Nodes.Contains(Node))
            {
                foreach (Slot InSlot in Node.GetInSlots())
                {
                    ClearConnectionsOfSlot(InSlot);
                }
                foreach (Slot OutSlot in Node.GetOutSlots())
                {
                    ClearConnectionsOfSlot(OutSlot);
                }
                if (Node is Node_Comment)
                {
                    CommentCount--;
                }
                Nodes.Remove(Node);
            }
        }

        public Node FindNodeByID(int ID)
        {
            foreach (Node Node in Nodes)
            {
                if (Node.ID == ID)
                {
                    return Node;
                }
            }

            return null;
        }

        public List<T> GetNodesOf<T>() where T : Node
        {
            List<T> Result = new List<T>();

            foreach(Node Node in Nodes)
            {
                if (Node is T)
                {
                    Result.Add(Node as T);
                }
            }

            return Result;
        }

        #endregion

        #region Connection

        public int GenerateConnectionID()
        {
            int ID = ConnectionID;
            ConnectionID += Random.Next(1, 20);
            return ID;
        }

        public virtual SlotConnectionResponse CanCreateConnection(Slot SlotA, Slot SlotB)
        {
            if (SlotA.bOutput ^ SlotB.bOutput)
            {
                if (SlotA.Node == SlotB.Node)
                {
                    return new SlotConnectionResponse("Both are on the same Node", ConnectCondition.NotAllow);
                }

                if (SlotA.SlotType != SlotB.SlotType || SlotA.SlotSubType != SlotB.SlotSubType)
                {
                    return new SlotConnectionResponse("Slot types are not compatible", ConnectCondition.NotAllow);
                }

                if (HasConnection(SlotA, SlotB))
                {
                    return new SlotConnectionResponse("Already has connected", ConnectCondition.NotAllow);
                }

                return new SlotConnectionResponse("Connect success", ConnectCondition.Allow);
            }
            else
            {
                return new SlotConnectionResponse("Directions are not compatible", ConnectCondition.NotAllow);
            }
        }

        public virtual void TryConnect(Slot SlotA, Slot SlotB)
        {
            SlotConnectionResponse Response = CanCreateConnection(SlotA, SlotB);

            if (Response.Condition == ConnectCondition.Allow)
            {
                Connection NewConnection = CreateConnection(SlotA, SlotB);
                View.ImportConnection(NewConnection);
            }
            else
            {
                ConsoleUI.GetInstance().AddLogItem(LogMessageType.Information, Response.Message);
            }
        }

        public bool HasConnection(Slot SlotA, Slot SlotB)
        {
            Slot InSlot = SlotA.bOutput ? SlotB : SlotA;
            Slot OutSlot = SlotA.bOutput ? SlotA : SlotB;

            foreach (Connection Connection in Connections)
            {
                if (Connection.InSlot == InSlot && Connection.OutSlot == OutSlot)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual Connection CreateConnection(Slot SlotA, Slot SlotB)
        {
            Connection NewConnection = new Connection();

            NewConnection.ID = GenerateConnectionID();
            NewConnection.BindInSlot(SlotA.bOutput ? SlotB : SlotA);
            NewConnection.BindOutSlot(SlotA.bOutput ? SlotA : SlotB);

            return NewConnection;
        }

        public void AddConnection(Connection Connection)
        {
            if (Connection == null || Connections.Contains(Connection))
            {
                return;
            }

            Connection.BindInSlot(Connection.InSlot);
            Connection.BindOutSlot(Connection.OutSlot);

            Connections.Add(Connection);
        }

        public void RemoveConnection(Connection Connection)
        {
            if (Connections.Contains(Connection))
            {
                Connection.InSlot?.RemoveConnection(Connection);
                Connection.OutSlot?.RemoveConnection(Connection);

                Connections.Remove(Connection);
            }
        }

        public Connection FindConnectionByID(int ID)
        {
            foreach (Connection Connection in Connections)
            {
                if (Connection.ID == ID)
                {
                    return Connection;
                }
            }

            return null;
        }

        public void ClearConnectionsOfSlot(Slot Slot)
        {
            List<Connection> Connections = Slot.GetConnections();
            for (int i = Connections.Count - 1; i >= 0; --i)
            {
                RemoveConnection(Connections[i]);
            }
        }

        public List<Connection> GetConnectionsRelated(List<Node> Nodes)
        {
            List<Connection> Connections = new List<Connection>();
            foreach (Node Node in Nodes)
            {
                foreach (Slot Slot in Node.GetInSlots())
                {
                    foreach (Connection Connection in Slot.GetConnections())
                    {
                        if (!Connections.Contains(Connection) && 
                            Nodes.Contains(Connection.InSlot.Node) | Nodes.Contains(Connection.OutSlot.Node))
                        {
                            Connections.Add(Connection);
                        }
                    }
                }

                foreach (Slot Slot in Node.GetOutSlots())
                {
                    foreach (Connection Connection in Slot.GetConnections())
                    {
                        if (!Connections.Contains(Connection) && 
                            Nodes.Contains(Connection.InSlot.Node) | Nodes.Contains(Connection.OutSlot.Node))
                        {
                            Connections.Add(Connection);
                        }
                    }
                }
            }
            return Connections;
        }

        public List<Connection> GetConnectionsOutSide(List<Node> Nodes)
        {
            List<Connection> Connections = new List<Connection>();
            foreach(Node Node in Nodes)
            {
                foreach(Slot Slot in Node.GetInSlots())
                {
                    foreach(Connection Connection in Slot.GetConnections())
                    {
                        if (Nodes.Contains(Connection.InSlot.Node) ^ Nodes.Contains(Connection.OutSlot.Node))
                        {
                            Connections.Add(Connection);
                        }
                    }
                }

                foreach (Slot Slot in Node.GetOutSlots())
                {
                    foreach (Connection Connection in Slot.GetConnections())
                    {
                        if (Nodes.Contains(Connection.InSlot.Node) ^ Nodes.Contains(Connection.OutSlot.Node))
                        {
                            Connections.Add(Connection);
                        }
                    }
                }
            }
            return Connections;
        }

        public List<Connection> GetConnectionsInside(List<Node> Nodes)
        {
            List<Connection> Connections = new List<Connection>();
            foreach (Node Node in Nodes)
            {
                foreach (Slot Slot in Node.GetInSlots())
                {
                    foreach (Connection Connection in Slot.GetConnections())
                    {
                        if (Nodes.Contains(Connection.InSlot.Node) & Nodes.Contains(Connection.OutSlot.Node))
                        {
                            Connections.Add(Connection);
                        }
                    }
                }
            }
            return Connections;
        }

        #endregion

        public ClipData GetActualClipData(ClipData Data)
        {
            if (Data.NodesToPaste.Count == 0 ||
                Nodes.Contains(Data.NodesToPaste[0]) == false)
            {
                return Data;
            }

            ClipData ActualClipData = new ClipData();

            List<Node> ActualNodes = new List<Node>();
            foreach(Node Node in Data.NodesToPaste)
            {
                Node ActualNode = CreateNode(Node.GetType().Name);
                Node.CloneTo(ref ActualNode);
                ActualNode.ID = GenerateNodeID();
                ActualNode.SetOwner(this);
                ActualNodes.Add(ActualNode);
            }
            ActualClipData.NodesToPaste = ActualNodes;

            List<Connection> ActualConnections = new List<Connection>();
            for (int i = 0; i < Data.ConnectionsToPaste.Count; ++i)
            {
                Connection Connection = Data.ConnectionsToPaste[i];
                int InSlotNodeIndex = Data.NodesToPaste.IndexOf(Connection.InSlot.Node);
                int OutSlotNodeIndex = Data.NodesToPaste.IndexOf(Connection.OutSlot.Node);
                int InSlotIndex = Connection.InSlot.Index;
                int OutSlotIndex = Connection.OutSlot.Index;

                Connection NewConnection = CreateConnection(
                    ActualNodes[InSlotNodeIndex].GetInSlot(InSlotIndex),
                    ActualNodes[OutSlotNodeIndex].GetOutSlot(OutSlotIndex)
                    );
                Connection.CloneTo(ref NewConnection);
                ActualConnections.Add(NewConnection);
            }
            ActualClipData.ConnectionsToPaste = ActualConnections;

            return ActualClipData;
        }

        public void GetNodesBlock(List<Node> Nodes, ref int X, ref int Y, ref int Width, ref int Height)
        {
            foreach (Node Node in Nodes)
            {
                Node.DoLayout();
            }

            int XMin = int.MaxValue, XMax = int.MinValue, YMin = int.MaxValue, YMax = int.MinValue;
            foreach(Node Node in Nodes)
            {
                XMin = Math.Min(Node.X, XMin);
                XMax = Math.Max(Node.X + Node.Width, XMax);
                YMin = Math.Min(Node.Y, YMin);
                YMax = Math.Max(Node.Y + Node.Height, YMax);
            }
            X = XMin;
            Y = YMin;
            Width = XMax - XMin;
            Height = YMax - YMin;
        }

        public void UpdateNodesInComment()
        {
            for (int i = 0; i < CommentCount; ++i)
            {
                Node_Comment Comment = Nodes[i] as Node_Comment;
                Comment.NodesInComment.Clear();

                for (int j = 0; j < Nodes.Count; ++j)
                {
                    if (i != j)
                    {
                        if (Comment.ContainNode(Nodes[j]))
                        {
                            Comment.NodesInComment.Add(Nodes[j]);
                        }
                    }    
                }
            }
        }

        public virtual void Run()
        {

        }

        #region Menu

        public virtual List<MenuBuilder> BuildMenu(object Context)
        {
            if (Context is Node)
            {
                return View.BuildNodeActionMenu();
            }
            else if (Context is Slot)
            {
                return View.BuildSlotActionMenu(Context as Slot);
            }
            else if (Context is Connection)
            {
                return BuildNodeMenu(Node =>
                {
                    Connection Connection = Context as Connection;
                    int MoveStep = NodeGraphView.MoveStep;
                    Node.X = Convert.ToInt32(Connection.TempEnd.X) / MoveStep * MoveStep;
                    Node.Y = Convert.ToInt32(Connection.TempEnd.Y) / MoveStep * MoveStep;

                    Slot SlotB;
                    Predicate<Slot> Matching = Slot => Slot.SlotType == View.SlotA.SlotType && Slot.SlotSubType == View.SlotA.SlotSubType;
                    if (View.SlotA.bOutput)
                    {
                        SlotB = Node.GetInSlots().Find(Matching);
                    }
                    else
                    {
                        SlotB = Node.GetOutSlots().Find(Matching);
                    }

                    Connection NewConnection = null;
                    if (SlotB != null)
                    {
                        SlotConnectionResponse Response = CanCreateConnection(View.SlotA, SlotB);
                        if (Response.Condition == ConnectCondition.Allow)
                        {
                            NewConnection = CreateConnection(View.SlotA, SlotB);
                        }
                    }

                    View.ImportNodeWithConnection(Node, NewConnection, null);
                });
            }
            else
            {
                List<MenuBuilder> MenuBuilders = new List<MenuBuilder>();
                MenuBuilders.AddRange(View.BuildContextActionMenu());
                MenuBuilders.Add(new MenuBuilder { bIsSeperator = true });
                MenuBuilders.AddRange(BuildNodeMenu(View.ImportNode));
                return MenuBuilders;
            }
        }

        public virtual List<MenuBuilder> BuildNodeMenu(Action<Node> ProcessNode)
        {
            return new List<MenuBuilder> { new MenuBuilder { Text = "Use without implement." } };
        }

        #endregion

        public virtual void CloneTo(ref NodeGraphModel Target)
        {
            Target.Name = Name;
            Target.NodeID = NodeID;
            Target.ConnectionID = ConnectionID;

            Target.Nodes = new List<Node>();
            foreach(Node Node in Nodes)
            {
                Node CloneNode = Target.CreateNode(Node.GetType().Name);
                CloneNode.SetOwner(Target);
                Node.CloneTo(ref CloneNode);
                Target.AddNode(CloneNode);
            }

            Target.Connections = new List<Connection>();
            foreach(Connection Connection in Connections)
            {
                Connection CloneConnection = Activator.CreateInstance(Connection.GetType()) as Connection;
                CloneConnection.ID = Connection.ID;
                CloneConnection.InSlot = Target.FindNodeByID(Connection.InSlot.Node.ID).GetInSlot(Connection.InSlot.Index);
                CloneConnection.OutSlot = Target.FindNodeByID(Connection.OutSlot.Node.ID).GetOutSlot(Connection.OutSlot.Index);
                Connection.CloneTo(ref CloneConnection);
                Target.AddConnection(CloneConnection);
            }
        }

        public void MoveCameraToCenter()
        {
            if (Nodes.Count == 0)
            {
                Camera.WorldX = -Camera.WorldHeight * Camera.AspectRatio / 2;
                Camera.WorldY = -Camera.WorldHeight / 2;
                return;
            }

            int X = 0, Y = 0, Width = 0, Height = 0;
            GetNodesBlock(Nodes, ref X, ref Y, ref Width, ref Height);

            Camera.WorldX = X + Width / 2 - Camera.WorldHeight * Camera.AspectRatio / 2;
            Camera.WorldY = Y + Height / 2 - Camera.WorldHeight / 2;
        }

        #region XML Save and Load

        public virtual void SaveToRecord(Record RecordNodeGraph)
        {
            RecordNodeGraph.SetTypeString("NodeGraph");

            RecordNodeGraph.SetString("Name", Name);
            RecordNodeGraph.SetInt("NodeID", NodeID);
            RecordNodeGraph.SetInt("ConnectionID", ConnectionID);

            Record RecordCamera = RecordNodeGraph.AddChild();
            Camera.SaveToXml(RecordCamera);

            foreach (Node Node in Nodes)
            {
                Record RecordNode = RecordNodeGraph.AddChild();
                SaveNodeToRecord(Node, RecordNode);
            }
            foreach (Connection Connection in Connections)
            {
                Record RecordConnection = RecordNodeGraph.AddChild();
                SaveConnectionToRecord(Connection, RecordConnection);
            }
        }

        public virtual void SaveNodeToRecord(Node NodeToSave, Record RecordNode)
        {
            Type Type = NodeToSave.GetType();
            RecordNode.SetTypeString(Type.Name);

            PropertyInfo[] Properties = Type.GetProperties();
            foreach (PropertyInfo Info in Properties)
            {
                PropertyInfoAttribute PropertyInfoAttribute = PropertyInfoAttribute.GetPropertyInfoAttribute(Info);
                if (PropertyInfoAttribute.bHide == false)
                {
                    RecordSet(RecordNode, Info.PropertyType, Info.Name, Info.GetValue(NodeToSave));
                }
            }

            RecordNode.SetInt("ID", NodeToSave.ID);
            RecordNode.SetInt("X", NodeToSave.X);
            RecordNode.SetInt("Y", NodeToSave.Y);

            if (NodeToSave.bHasSubGraph)
            {
                Record RecordSubGraph = RecordNode.AddChild();
                NodeToSave.SubGraph.SaveToRecord(RecordSubGraph);
            }
        }

        public virtual void SaveConnectionToRecord(Connection ConnectionToSave, Record RecordConnection)
        {
            RecordConnection.SetTypeString("Connection");
            RecordConnection.SetInt("ID", ConnectionToSave.ID);
            RecordConnection.SetInt("OutSlotNodeID", ConnectionToSave.OutSlot.Node.ID);
            RecordConnection.SetInt("OutSlotIndex", ConnectionToSave.OutSlot.Index);
            RecordConnection.SetInt("InSlotNodeID", ConnectionToSave.InSlot.Node.ID);
            RecordConnection.SetInt("InSlotIndex", ConnectionToSave.InSlot.Index);
        }

        public void RecordSet(Record Record, Type TargetType, string Name, object Value)
        {
            if (TargetType == typeof(bool))
            {
                Record.SetBool(Name, (bool)Value);
            }
            else if (TargetType == typeof(float))
            {
                Record.SetFloat(Name, (float)Value);
            }
            else if (TargetType == typeof(int))
            {
                Record.SetFloat(Name, (int)Value);
            }
            else if (TargetType == typeof(long))
            {
                Record.SetLongLong(Name, (long)Value);
            }
            else if (TargetType == typeof(string))
            {
                Record.SetString(Name, (string)Value);
            }
            else if (TargetType == typeof(uint))
            {
                Record.SetUnsignedInt(Name, (uint)Value);
            }
            else if (TargetType == typeof(SyncParams))
            {
                (Value as SyncParams).SaveToXml(Record.AddChild());
            }
            else if (TargetType.IsEnum)
            {
                Record.SetString(Name, Value.ToString());
            }
        }

        public virtual void LoadFromRecord(Record RecordNodeGraph)
        {
            Nodes.Clear();

            Name = RecordNodeGraph.GetString("Name");
            NodeID = RecordNodeGraph.GetInt("NodeID");
            ConnectionID = RecordNodeGraph.GetInt("ConnectionID");

            int Count = RecordNodeGraph.GetChildCount();
            for (int i = 0; i < Count; i++)
            {
                Record RecordChild = RecordNodeGraph.GetChild(i);
                string TypeString = RecordChild.GetTypeString();
                if (TypeString == "Connection")
                {
                    Record RecordConnection = RecordChild;
                    LoadConnectionFromRecord(RecordConnection);
                }
                else if (TypeString == "Camera")
                {
                    Record RecordCamera = RecordChild;
                    Camera.LoadFromXml(RecordCamera);
                }
                else
                {
                    Record RecordNode = RecordChild;
                    LoadNodeFromRecord(RecordNode);
                }
            }
        }

        public virtual Node LoadNodeFromRecord(Record RecordNode)
        {
            string TypeString = RecordNode.GetTypeString();
            Node Node = CreateNode(TypeString);
            if (Node == null)
            {
                DebugHelper.Assert(false);
            }
            else
            {
                Type NodeType = Node.GetType();
                PropertyInfo[] Properties = NodeType.GetProperties();
                foreach (PropertyInfo Info in Properties)
                {
                    PropertyInfoAttribute PropertyInfoAttribute = PropertyInfoAttribute.GetPropertyInfoAttribute(Info);
                    if (PropertyInfoAttribute.bHide == false)
                    {
                        object Value = RecordLoad(RecordNode, Info.PropertyType, Info.Name);
                        if (Value != null)
                        {
                            Info.SetValue(Node, Value);
                        }
                    }
                }
            }

            Node.ID = RecordNode.GetInt("ID");
            Node.X = RecordNode.GetInt("X");
            Node.Y = RecordNode.GetInt("Y");
            Node.SetOwner(this);

            if (RecordNode.GetChildCount() > 0 && Node.bHasSubGraph)
            {
                Record RecordSubGraph = RecordNode.FindByTypeString("NodeGraph");
                if (RecordSubGraph != null)
                {
                    Node.SubGraph.LoadFromRecord(RecordSubGraph);
                }
            }

            AddNode(Node);
            return Node;
        }

        public virtual void LoadConnectionFromRecord(Record RecordConnection)
        {
            Connection Connection = new Connection();

            Connection.ID = RecordConnection.GetInt("ID");

            int OutSlotNodeID = RecordConnection.GetInt("OutSlotNodeID");
            int OutSlotIndex = RecordConnection.GetInt("OutSlotIndex");
            Slot OutSlot = FindNodeByID(OutSlotNodeID).GetOutSlot(OutSlotIndex);
            Connection.BindOutSlot(OutSlot);

            int InSlotNodeID = RecordConnection.GetInt("InSlotNodeID");
            int InSlotIndex = RecordConnection.GetInt("InSlotIndex");
            Slot InSlot = FindNodeByID(InSlotNodeID).GetInSlot(InSlotIndex);
            Connection.BindInSlot(InSlot);

            AddConnection(Connection);
        }

        public object RecordLoad(Record Record, Type TargetType, string Name)
        {
            if (TargetType == typeof(bool))
            {
                return Record.GetBool(Name);
            }
            else if (TargetType == typeof(float))
            {
                return Record.GetFloat(Name);
            }
            else if (TargetType == typeof(int))
            {
                return Record.GetInt(Name);
            }
            else if (TargetType == typeof(long))
            {
                return Record.GetLongLong(Name);
            }
            else if (TargetType == typeof(string))
            {
                return Record.GetString(Name);
            }
            else if (TargetType == typeof(uint))
            {
                return Record.GetUnsignedInt(Name);
            }
            else if (TargetType == typeof(SyncParams))
            {
                SyncParams Params = new SyncParams();
                Record RecordParams = Record.FindByTypeString("SyncParams");
                if (RecordParams != null)
                {
                    Params.LoadFromXml(RecordParams);
                }
                return Params;
            }
            else if (TargetType.IsEnum)
            {
                return Enum.Parse(TargetType, Record.GetString(Name));
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
