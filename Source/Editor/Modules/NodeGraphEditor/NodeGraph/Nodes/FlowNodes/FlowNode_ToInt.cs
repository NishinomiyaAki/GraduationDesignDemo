using EditorUI;

namespace CrossEditor
{
    class FlowNode_ToInt : FlowNode
    {
        public FlowNode_ToInt()
        {
            Name = "ToInt";
            NodeType = NodeType.Expression;

            AddInSlot("Value", SlotType.DataFlow);

            AddOutSlot("Value", SlotType.DataFlow);
        }

        public override object Eval(int OutSlotIndex)
        {
            if (OutSlotIndex == 0)
            {
                int OutSlotIndex1;
                FlowNode InNode = GetInputNode(0, out OutSlotIndex1) as FlowNode;
                if (InNode == null)
                {
                    CommitInSlotError(0, "slot is not connected.");
                    return 0;
                }
                object Value1 = InNode.Eval(OutSlotIndex1);
                if (Value1 is int)
                {
                    int Int1 = (int)Value1;
                    return Int1;
                }
                else if (Value1 is float)
                {
                    float Float1 = (float)Value1;
                    return (int)Float1;
                }
                else if (Value1 is string)
                {
                    string String1 = (string)Value1;
                    return MathHelper.ParseInt(String1);
                }
                else
                {
                    CommitInSlotError(0, "slot can not convert to int.");
                    return 0;
                }
            }
            return 0;
        }
    }
}
