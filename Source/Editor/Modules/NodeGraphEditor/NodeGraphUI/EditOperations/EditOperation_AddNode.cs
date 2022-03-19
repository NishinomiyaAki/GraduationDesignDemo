namespace Editor
{
    internal class EditOperation_AddNode : EditOperation
    {
        private NodeGraph _NodeGraph;
        private Node _Node;

        public EditOperation_AddNode(NodeGraph NodeGraph, Node Node)
        {
            _NodeGraph = NodeGraph;
            _Node = Node;
        }

        public override void Undo()
        {
            _NodeGraph.RemoveNode(_Node);

            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(null);
            InspectorUI.InspectObject();

            NodeGraphUI.GetInstance().SetModified();
        }

        public override void Redo()
        {
            _NodeGraph.AddNodeDirectly(_Node);

            NodeGraphUI.GetInstance().SelectNode(_Node);

            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(_Node);
            InspectorUI.InspectObject();

            NodeGraphUI.GetInstance().SetModified();
        }
    }
}