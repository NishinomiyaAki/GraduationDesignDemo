using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace Editor
{
    enum AbortMode
    {
        None,
        Self
    }

    class BTDecoratorNode : BTAuxiliaryNode
    {
        public const int _SpanX = 5;
        public const int _SpanY = 3;

        public AbortMode _Abort;

        public AbortMode Abort
        {
            get
            {
                return _Abort;
            }
            set
            {
                _Abort = value;
            }
        }

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetString("Abort", _Abort.ToString());
        }

        public override void LoadFromXml(Record RecordNode, BehaviorTree BehaviorTree)
        {
            base.LoadFromXml(RecordNode, BehaviorTree);
            _Abort = Enum.Parse<AbortMode>(RecordNode.GetString("Abort"));
        }

        public virtual bool CheckCondition(BehaviorTreeComponent BTComponent)
        {
            return false;
        }
    }
}
