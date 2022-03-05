namespace CrossEditor
{
    internal class FlowNode_ForLoop : FlowNode
    {
        private int Index;

        public FlowNode_ForLoop()
        {
            Name = "ForLoop";
            NodeType = NodeType.ControlFlow;

            Index = 0;

            AddInSlot("Control", SlotType.ControlFlow);
            AddInSlot("FirstIndex", SlotType.DataFlow);
            AddInSlot("LastIndex", SlotType.DataFlow);

            AddOutSlot("Completed", SlotType.ControlFlow);
            AddOutSlot("LoopBody", SlotType.ControlFlow);
            AddOutSlot("Index", SlotType.DataFlow);
        }

        public override object Eval(int OutSlotIndex)
        {
            if (OutSlotIndex == 2)
            {
                return Index;
            }
            else
            {
                return null;
            }
        }

        public override void Run()
        {
            int Value1;
            bool bSuccess1 = GetInSlotValue_Int(1, out Value1);
            if (bSuccess1 == false)
            {
                return;
            }
            int Value2;
            bool bSuccess2 = GetInSlotValue_Int(2, out Value2);
            if (bSuccess2 == false)
            {
                return;
            }
            for (Index = Value1; Index <= Value2; Index++)
            {
                RunOutSlot(1);
            }
            RunOutSlot(0);
        }
    }
}