using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace Editor
{
    class BTCompositeNode : BTNode
    {
        public const int _SpanX = 5;
        public const int _SpanY = 3;

        protected List<BTNode> ChildNodes
        {
            get
            {
                List<BTNode> Nodes = GetChildNodes();
                Nodes.Sort((Left, Right) =>
                {
                    return Left._ExecuteIndex.CompareTo(Right._ExecuteIndex);
                });
                return Nodes;
            }
        }

        public virtual string GetStringContent()
        {
            return "";
        }
    }
}
