using EditorUI;
using System.Collections.Generic;

namespace CrossEditor
{
    class EditOperation_MoveNode : EditOperation
    {
        NodeGraphView _View;
        List<Node> _NodesToMove;
        List<Node> _NodesToSelect;
        int _DeltaX;
        int _DeltaY;

        public EditOperation_MoveNode(NodeGraphView View, List<Node> NodesToMove, List<Node> NodesToSelect, int DeltaX, int DeltaY)
        {
            _View = View;
            _NodesToMove = NodesToMove.Clone();
            _NodesToSelect = NodesToSelect.Clone();
            _DeltaX = DeltaX;
            _DeltaY = DeltaY;
        }

        public override void Undo()
        {
            foreach (Node Node in _NodesToMove)
            {
                Node.X -= _DeltaX;
                Node.Y -= _DeltaY;
            }

            _View.ClearSelectedNodes();
            foreach (Node Node in _NodesToSelect)
            {
                _View.AddSelectedNode(Node);
            }

            _View.SetModified();
        }

        public override void Redo()
        {
            foreach (Node Node in _NodesToMove)
            {
                Node.X += _DeltaX;
                Node.Y += _DeltaY;
            }

            _View.ClearSelectedNodes();
            foreach (Node Node in _NodesToSelect)
            {
                _View.AddSelectedNode(Node);
            }

            _View.SetModified();
        }
    }
}
