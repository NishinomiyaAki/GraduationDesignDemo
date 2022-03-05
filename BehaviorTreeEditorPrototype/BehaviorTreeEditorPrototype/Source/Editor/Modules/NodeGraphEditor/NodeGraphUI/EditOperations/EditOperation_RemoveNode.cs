namespace CrossEditor
{
    internal class EditOperation_RemoveNode : EditOperation
    {
        private NodeGraph _NodeGraph;
        private Node _Node;
        private ConnectionItemList _ConnectionItemList;

        public EditOperation_RemoveNode(NodeGraph NodeGraph, Node Node, ConnectionItemList ConnectionItemList)
        {
            _NodeGraph = NodeGraph;
            _Node = Node;
            _ConnectionItemList = ConnectionItemList;
        }

        public override void Undo()
        {
            _NodeGraph.AddNodeDirectly(_Node);
            _ConnectionItemList.DoAddOperation();

            NodeGraphUI.GetInstance().SelectNode(_Node);

            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(_Node);
            InspectorUI.InspectObject();

            NodeGraphUI.GetInstance().SetModified();
        }

        public override void Redo()
        {
            _NodeGraph.RemoveNode(_Node);

            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(null);
            InspectorUI.InspectObject();

            NodeGraphUI.GetInstance().SetModified();
        }
    }
}