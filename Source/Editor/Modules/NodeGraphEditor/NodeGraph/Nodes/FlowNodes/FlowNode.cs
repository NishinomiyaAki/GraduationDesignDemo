using EditorUI;
using System.Collections.Generic;

namespace CrossEditor
{
    public class FlowNode : Node
    {
        public string TemplateExpression = "";
        public virtual object Eval(int OutSlotIndex)
        {
            return null;
        }

        public virtual void Run()
        {

        }

        public bool GetInSlotValue_Bool(int InSlotIndex, out bool bValue)
        {
            int OutSlotIndex;
            FlowNode InNode = GetInputNode(InSlotIndex, out OutSlotIndex) as FlowNode;
            if (InNode != null)
            {
                object Value1 = InNode.Eval(OutSlotIndex);
                if (Value1 != null && Value1 is bool)
                {
                    bValue = (bool)Value1;
                    return true;
                }
                else
                {
                    CommitInSlotError(InSlotIndex, "slot is not connected with a bool value.");
                    bValue = false;
                    return false;
                }
            }
            CommitInSlotError(InSlotIndex, "slot is not connected.");
            bValue = false;
            return false;
        }

        public bool GetInSlotValue_Int(int InSlotIndex, out int Value)
        {
            int OutSlotIndex;
            FlowNode InNode = GetInputNode(InSlotIndex, out OutSlotIndex) as FlowNode;
            if (InNode != null)
            {
                object Value1 = InNode.Eval(OutSlotIndex);
                if (Value1 != null && Value1 is int)
                {
                    Value = (int)Value1;
                    return true;
                }
                else
                {
                    CommitInSlotError(InSlotIndex, "slot is not connected with a int value.");
                    Value = 0;
                    return false;
                }
            }
            CommitInSlotError(InSlotIndex, "slot is not connected.");
            Value = 0;
            return false;
        }

        public bool GetInSlotValue_String(int InSlotIndex, out string String)
        {
            int OutSlotIndex;
            FlowNode InNode = GetInputNode(InSlotIndex, out OutSlotIndex) as FlowNode;
            if (InNode != null)
            {
                object Value1 = InNode.Eval(OutSlotIndex);
                if (Value1 != null && Value1 is string)
                {
                    String = (string)Value1;
                    return true;
                }
                else
                {
                    CommitInSlotError(InSlotIndex, "slot is not connected with a string value.");
                    String = "";
                    return false;
                }
            }
            CommitInSlotError(InSlotIndex, "slot is not connected.");
            String = "";
            return false;
        }        

        public void RunOutSlot(int OutSlotIndex)
        {
            List<Node> OutputNodes = GetOutputNodes(OutSlotIndex);
            foreach (Node OutputNode in OutputNodes)
            {
                FlowNode FlowNode = OutputNode as FlowNode;
                FlowNode.Run();
            }
        }

        public virtual string ToExpression()
        {
            return TemplateExpression;
        }

        public List<Node> GetOutputNodes(int Index)
        {
            Slot Slot = GetOutSlot(Index);
            List<Connection> Connections = Slot.GetConnections();
            List<Node> Nodes = new List<Node>();
            foreach (Connection Connection in Connections)
            {
                Nodes.Add(Connection.InSlot.Node);
            }
            return Nodes;
        }

        public Node GetInputNode(int Index, out int OutSlotIndex)
        {
            Slot Slot = GetInSlot(Index);
            List<Connection> Connections = Slot.GetConnections();
            int ConnectionCount = Connections.Count;
            if (ConnectionCount > 0)
            {
                Connection Connection = Connections[0];
                OutSlotIndex = Connection.OutSlot.Index;
                return Connection.OutSlot.Node;
            }
            else
            {
                OutSlotIndex = -1;
                return null;
            }
        }

        public virtual void CommitInSlotError(int InSlotIndex, string ErrorInformation)
        {
            SetError();
            Slot Slot = GetInSlot(InSlotIndex);
            Slot.SetError();
            string ErrorMessage = string.Format("Error: Graph Slot {0}({1})-{2}({3}): {4}", Name, ID, Slot.Name, InSlotIndex, ErrorInformation);
            ConsoleUI.GetInstance().AddLogItem(LogMessageType.Error, ErrorMessage);
            MainUI.GetInstance().ActivateDockingCard_Console();
        }
    }
}
