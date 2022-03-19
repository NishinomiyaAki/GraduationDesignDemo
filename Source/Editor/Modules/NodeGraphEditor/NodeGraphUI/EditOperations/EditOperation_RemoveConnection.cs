using EditorUI;
using System.Collections.Generic;

namespace CrossEditor
{
    class EditOperation_RemoveConnection : EditOperation
    {
        NodeGraphView _View;
        List<Connection> _ConnectionsToRemove;

        public EditOperation_RemoveConnection(NodeGraphView View, List<Connection> ConnectionsToRemove)
        {
            _View = View;
            _ConnectionsToRemove = ConnectionsToRemove.Clone();
        }

        public override void Undo()
        {
            foreach (Connection Connection in _ConnectionsToRemove)
            {
                _View.GetModel().AddConnection(Connection);
            }

            _View.SetModified();
        }

        public override void Redo()
        {
            for (int i = _ConnectionsToRemove.Count - 1; i >= 0; --i)
            {
                _View.GetModel().RemoveConnection(_ConnectionsToRemove[i]);
            }

            _View.SetModified();
        }
    }
}
