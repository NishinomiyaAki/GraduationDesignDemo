namespace Editor
{
    internal class FlowNode_PrintString : FlowNode
    {
        public FlowNode_PrintString()
        {
            Name = "PrintString";
            NodeType = NodeType.Statement;

            AddInSlot("Control", SlotType.ControlFlow);
            AddInSlot("String", SlotType.DataFlow);

            AddOutSlot("Control", SlotType.ControlFlow);
        }

        public override void Run()
        {
            string String1;
            bool bSuccess1 = GetInSlotValue_String(1, out String1);
            if (bSuccess1 == false)
            {
                return;
            }
            ConsoleUI.GetInstance().AddLogItem(LogMessageType.Information, String1);
            RunOutSlot(0);
        }
    }
}