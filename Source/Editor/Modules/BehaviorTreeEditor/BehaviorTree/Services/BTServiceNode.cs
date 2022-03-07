using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace Editor
{
    class BTServiceNode : BTAuxiliaryNode
    {
        public const int _SpanX = 5;
        public const int _SpanY = 3;

        public float _ExecuteInterval;

        public string ServiceName
        {
            get
            {
                return _Custom;
            }
            set
            {
                _Custom = value;
            }
        }

        public float ExecuteInterval
        {
            get
            {
                return _ExecuteInterval;
            }
            set
            {
                _ExecuteInterval = value;
            }
        }

        public virtual void Tick(BehaviorTreeComponent BTComponent,float DeltaTime)
        {
        }

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetString("Custom", _Custom);
            RecordNode.SetFloat("ExecuteInterval", _ExecuteInterval);
        }

        public override void LoadFromXml(Record RecordNode, BehaviorTree BehaviorTree)
        {
            base.LoadFromXml(RecordNode, BehaviorTree);
            _Custom = RecordNode.GetString("Custom");
            _ExecuteInterval = RecordNode.GetFloat("ExecuteInterval");
        }
    }
}
