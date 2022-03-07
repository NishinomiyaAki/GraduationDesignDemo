namespace CrossEditor
{
    internal class EditOperation_MoveNode : EditOperation
    {
        private Node _Node;
        private int _OldX;
        private int _OldY;
        private int _NewX;
        private int _NewY;

        public EditOperation_MoveNode(Node Node, int OldX, int OldY, int NewX, int NewY)
        {
            _Node = Node;
            _OldX = OldX;
            _OldY = OldY;
            _NewX = NewX;
            _NewY = NewY;
        }

        public override void Undo()
        {
            _Node.X = _OldX;
            _Node.Y = _OldY;
            _Node.DoLayoutWithConnections();

            NodeGraphUI.GetInstance().SelectNode(_Node);

            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(_Node);
            InspectorUI.InspectObject();

            NodeGraphUI.GetInstance().SetModified();
        }

        public override void Redo()
        {
            _Node.X = _NewX;
            _Node.Y = _NewY;
            _Node.DoLayoutWithConnections();

            NodeGraphUI.GetInstance().SelectNode(_Node);

            InspectorUI InspectorUI = InspectorUI.GetInstance();
            InspectorUI.SetObjectInspected(_Node);
            InspectorUI.InspectObject();

            NodeGraphUI.GetInstance().SetModified();
        }
    }
}