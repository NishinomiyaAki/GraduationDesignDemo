namespace CrossEditor
{
    internal class ConnectionItem
    {
        public int ConnectionID;
        public int OutNodeID;
        public string OutSlotName;
        public int InNodeID;
        public string InSlotName;

        public ConnectionItem()
        {
            ConnectionID = -1;
            OutNodeID = -1;
            OutSlotName = "";
            InNodeID = -1;
            InSlotName = "";
        }
    }
}