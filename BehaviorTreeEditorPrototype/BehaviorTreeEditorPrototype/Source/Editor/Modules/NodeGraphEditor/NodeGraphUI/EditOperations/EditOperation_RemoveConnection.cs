namespace CrossEditor
{
    internal class EditOperation_RemoveConnection : EditOperation
    {
        private ConnectionItemList _ConnectionItemList;

        public EditOperation_RemoveConnection(ConnectionItemList ConnectionItemList)
        {
            _ConnectionItemList = ConnectionItemList;
        }

        public override void Undo()
        {
            _ConnectionItemList.DoAddOperation();

            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(null);
            InspectorUI.InspectObject();

            NodeGraphUI.GetInstance().SetModified();
        }

        public override void Redo()
        {
            _ConnectionItemList.DoRemoveOperation();

            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(null);
            InspectorUI.InspectObject();

            NodeGraphUI.GetInstance().SetModified();
        }
    }
}