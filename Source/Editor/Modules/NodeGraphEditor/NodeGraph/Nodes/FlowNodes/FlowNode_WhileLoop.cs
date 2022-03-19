namespace CrossEditor
{
    class FlowNode_WhileLoop : FlowNode
    {
        public FlowNode_WhileLoop()
        {
            Name = "WhileLoop";
            NodeType = NodeType.ControlFlow;

            AddInSlot("Control", SlotType.ControlFlow);
            AddInSlot("Condition", SlotType.DataFlow);

            AddOutSlot("Completed", SlotType.ControlFlow);
            AddOutSlot("LoopBody", SlotType.ControlFlow);
        }

        public override void Run()
        {
            while (true)
            {
                bool bValue;
                bool bSuccess = GetInSlotValue_Bool(1, out bValue);
                if (bSuccess == false)
                {
                    return;
                }
                if (bValue)
                {
                    RunOutSlot(1);
                }
                else
                {
                    break;
                }
            }
            RunOutSlot(0);
        }
    }
}
