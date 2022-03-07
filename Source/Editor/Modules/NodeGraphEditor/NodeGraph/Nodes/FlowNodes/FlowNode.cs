using System.Collections.Generic;

namespace Editor
{
    public class FlowNode : Node
    {
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
    }
}