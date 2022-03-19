namespace CrossEditor
{
    class EditOperation_ResizeComment : EditOperation
    {
        NodeGraphView _View;
        Node_Comment _Comment;
        CursorPosition _Position;
        int _DeltaX;
        int _DeltaY;


        public EditOperation_ResizeComment(NodeGraphView View, Node_Comment Comment, CursorPosition Position, int DeltaX, int DeltaY)
        {
            _View = View;
            _Comment = Comment;
            _Position = Position;
            _DeltaX = DeltaX;
            _DeltaY = DeltaY;
        }

        public override void Undo()
        {
            _View.TryAddSelectedNode(_Comment);

            _Comment.CursorPosition = _Position;
            _Comment.Scale(_Comment.Width, _Comment.Height, _Comment.X, _Comment.Y, -_DeltaX, -_DeltaY);
            _Comment.CursorPosition = CursorPosition.Other;

            _View.UpdateInspect();
        }

        public override void Redo()
        {
            _View.TryAddSelectedNode(_Comment);

            _Comment.CursorPosition = _Position;
            _Comment.Scale(_Comment.Width, _Comment.Height, _Comment.X, _Comment.Y, _DeltaX, _DeltaY);
            _Comment.CursorPosition = CursorPosition.Other;

            _View.UpdateInspect();
        }
    }
}
