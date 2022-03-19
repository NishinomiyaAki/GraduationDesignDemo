namespace CrossEditor
{
    class FlowNode_ToString : FlowNode
    {
        public FlowNode_ToString()
        {
            Name = "ToString";
            NodeType = NodeType.Expression;

            AddInSlot("Value", SlotType.DataFlow);

            AddOutSlot("String", SlotType.DataFlow);
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
                    return "";
                }
                object Value1 = InNode.Eval(OutSlotIndex1);
                return Value1.ToString();
            }
            return "";
        }
    }
}
