using System.Collections.Generic;

namespace Editor
{
    internal class ConnectionItemList
    {
        private NodeGraph _NodeGraph;
        private List<ConnectionItem> _ConnectionItemList;

        public ConnectionItemList(NodeGraph NodeGraph)
        {
            _NodeGraph = NodeGraph;
            _ConnectionItemList = new List<ConnectionItem>();
        }

        public void AddConnection(Connection Connection)
        {
            ConnectionItem ConnectionItem = new ConnectionItem();
            ConnectionItem.ConnectionID = Connection.ID;
            ConnectionItem.OutNodeID = Connection.OutSlot.Node.ID;
            ConnectionItem.OutSlotName = Connection.OutSlot.Name;
            ConnectionItem.InNodeID = Connection.InSlot.Node.ID;
            ConnectionItem.InSlotName = Connection.InSlot.Name;
            _ConnectionItemList.Add(ConnectionItem);
        }

        public void DoAddOperation()
        {
            foreach (ConnectionItem ConnectionItem in _ConnectionItemList)
            {
                Node OutNode = _NodeGraph.FindNodeByID(ConnectionItem.OutNodeID);
                Slot OutSlot = OutNode.FindOutSlot(ConnectionItem.OutSlotName);
                Node InNode = _NodeGraph.FindNodeByID(ConnectionItem.InNodeID);
                Slot InSlot = InNode.FindInSlot(ConnectionItem.InSlotName);

                Connection Connection = new Connection();
                Connection.ID = ConnectionItem.ConnectionID;
                Connection.SetOutSlot(OutSlot);
                Connection.SetInSlot(InSlot);
                _NodeGraph.AddConnectionDirectly(Connection);
                Connection.DoLayout();
            }
        }

        public void DoRemoveOperation()
        {
            foreach (ConnectionItem ConnectionItem in _ConnectionItemList)
            {
                Connection Connection = _NodeGraph.FindConnectionByID(ConnectionItem.ConnectionID);
                _NodeGraph.RemoveConnection(Connection);
            }
        }
    }
}