namespace Editor
{
    internal class EditOperation_AddConnection : EditOperation
    {
        private ConnectionItemList _ConnectionItemList_RemoveOldConnections;
        private ConnectionItemList _ConnectionItemList_AddNewConnection;

        public EditOperation_AddConnection(ConnectionItemList ConnectionItemList_RemoveOldConnections, ConnectionItemList ConnectionItemList_AddNewConnection)
        {
            _ConnectionItemList_RemoveOldConnections = ConnectionItemList_RemoveOldConnections;
            _ConnectionItemList_AddNewConnection = ConnectionItemList_AddNewConnection;
        }

        public override void Undo()
        {
            _ConnectionItemList_AddNewConnection.DoRemoveOperation();
            _ConnectionItemList_RemoveOldConnections.DoAddOperation();

            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(null);
            InspectorUI.InspectObject();

            NodeGraphUI.GetInstance().SetModified();
        }

        public override void Redo()
        {
            _ConnectionItemList_RemoveOldConnections.DoRemoveOperation();
            _ConnectionItemList_AddNewConnection.DoAddOperation();

            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(null);
            InspectorUI.InspectObject();

            NodeGraphUI.GetInstance().SetModified();
        }
    }
}