namespace Editor
{
    internal class FlowNode_If : FlowNode
    {
        public FlowNode_If()
        {
            Name = "If";
            NodeType = NodeType.ControlFlow;

            AddInSlot("Control", SlotType.ControlFlow);
            AddInSlot("Condition", SlotType.DataFlow);

            AddOutSlot("BranchTrue", SlotType.ControlFlow);
            AddOutSlot("BranchFalse", SlotType.ControlFlow);
        }

        public override void Run()
        {
            bool bValue;
            bool bSuccess = GetInSlotValue_Bool(1, out bValue);
            if (bSuccess)
            {
                if (bValue)
                {
                    RunOutSlot(0);
                }
                else
                {
                    RunOutSlot(1);
                }
            }
        }
    }
}