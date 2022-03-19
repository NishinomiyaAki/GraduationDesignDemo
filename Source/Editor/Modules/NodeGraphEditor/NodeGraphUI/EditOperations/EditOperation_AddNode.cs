using EditorUI;
using System.Collections.Generic;

namespace CrossEditor
{
    class EditOperation_AddNode : EditOperation
    {
        NodeGraphView _View;
        List<Node> _Nodes;
        List<Connection> _ConnectionsToAdd;
        List<Connection> _ConnectionsToRemove;

        public EditOperation_AddNode(NodeGraphView View, List<Node> Nodes, List<Connection> ConnectionsToAdd, List<Connection> ConnectionsToRemove)
        {
            _View = View;
            _Nodes = Nodes.Clone();
            _ConnectionsToAdd = ConnectionsToAdd.Clone();
            _ConnectionsToRemove = ConnectionsToRemove.Clone();
        }

        public override void Undo()
        {
            foreach (Connection Connection in _ConnectionsToAdd)
            {
                _View.GetModel().RemoveConnection(Connection);
            }

            foreach (Connection Connection in _ConnectionsToRemove)
            {
                _View.GetModel().AddConnection(Connection);
            }

            _View.ClearSelectedNodes();
            foreach (Node Node in _Nodes)
            {
                _View.GetModel().RemoveNode(Node);
            }

            _View.SetModified();
        }

        public override void Redo()
        {
            _View.ClearSelectedNodes();
            foreach (Node Node in _Nodes)
            {
                _View.GetModel().AddNode(Node);
                _View.AddSelectedNode(Node);
            }

            foreach (Connection Connection in _ConnectionsToRemove)
            {
                _View.GetModel().RemoveConnection(Connection);
            }

            foreach (Connection Connection in _ConnectionsToAdd)
            {
                _View.GetModel().AddConnection(Connection);
            }

            _View.SetModified();
        }
    }
}
