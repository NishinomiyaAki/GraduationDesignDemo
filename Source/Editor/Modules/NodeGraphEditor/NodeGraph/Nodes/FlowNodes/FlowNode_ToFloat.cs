using EditorUI;

namespace CrossEditor
{
    class FlowNode_ToFloat : FlowNode
    {
        public FlowNode_ToFloat()
        {
            Name = "ToFloat";
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
                    return 0.0f;
                }
                object Value1 = InNode.Eval(OutSlotIndex1);
                if (Value1 is int)
                {
                    int Int1 = (int)Value1;
                    return (float)Int1;
                }
                else if (Value1 is float)
                {
                    float Float1 = (float)Value1;
                    return Float1;
                }
                else if (Value1 is string)
                {
                    string String1 = (string)Value1;
                    return MathHelper.ParseFloat(String1);
                }
                else
                {
                    CommitInSlotError(0, "slot can not convert to float.");
                    return 0.0f;
                }
            }
            return 0.0f;
        }
    }
}
