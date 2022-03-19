using EditorUI;
using System;
using System.Collections.Generic;

namespace CrossEditor
{
    class EditOperation_AddConnection : EditOperation
    {
        NodeGraphView _View;
        List<Connection> _ConnectionsToRemove;
        List<Connection> _ConnectionsToAdd;

        public EditOperation_AddConnection(NodeGraphView View, List<Connection> ConnectionsToRemove, List<Connection> ConnectionsToAdd)
        {
            _View = View;
            _ConnectionsToRemove = ConnectionsToRemove.Clone();
            _ConnectionsToAdd = ConnectionsToAdd.Clone();
        }

        public override void Undo()
        {
            for(int i = _ConnectionsToAdd.Count - 1; i >= 0; --i)
            {
                _View.GetModel().RemoveConnection(_ConnectionsToAdd[i]);
            }
            foreach(Connection Connection in _ConnectionsToRemove)
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
            foreach (Connection Connection in _ConnectionsToAdd)
            {
                _View.GetModel().AddConnection(Connection);
            }

            _View.SetModified();
        }
    }
}
