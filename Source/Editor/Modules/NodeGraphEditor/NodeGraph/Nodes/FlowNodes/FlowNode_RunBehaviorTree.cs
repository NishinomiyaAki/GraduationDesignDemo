using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace Editor
{
    class FlowNode_RunBehaviorTree : FlowNode_StringContent
    {
        public BehaviorTree _BehaviorTree;
        public BehaviorTreeComponent _BehaviorTreeComponent;
        public ExecuteMode _Mode;

        [PropertyInfo(ToolTips = "Behavior Tree Asset")]
        private string BehaviorTreeName
        {
            get
            {
                if(_BehaviorTree == null)
                {
                    return "None";
                }
                else
                {
                    return BehaviorTreeManager.GetInstance().GetName(_BehaviorTree);
                }
            }
            set
            {
                if(value == "None")
                {
                    _BehaviorTree = null;
                }
                else
                {
                    _BehaviorTree = BehaviorTreeManager.GetInstance().GetBehaviorTreeByName(value);
                }
                DoLayoutWithConnections();
            }
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Behavior Execute Mode")]
        public ExecuteMode Mode
        {
            get
            {
                return _Mode;
            }
            set
            {
                _Mode = value;
            }
        }

        public FlowNode_RunBehaviorTree()
        {
            Name = "Run Behavior Tree";
            NodeType = NodeType.Event;
            _BehaviorTree = null;
            _BehaviorTreeComponent = null;

            AddInSlot("Control", SlotType.ControlFlow);
            AddInSlot("Target", SlotType.DataFlow);

            AddOutSlot("Completed", SlotType.ControlFlow);
            AddOutSlot("Return Value", SlotType.DataFlow);
        }

        public override object Eval(int OutSlotIndex)
        {
            if(OutSlotIndex == 1)
            {
                return true;
            }
            else
            {
                return null;
            }
        }

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetString("BehaviorTree", BehaviorTreeName);
            RecordNode.SetString("ExecuteMode", _Mode.ToString());
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            BehaviorTreeName = RecordNode.GetString("BehaviorTree");
            _Mode = Enum.Parse<ExecuteMode>(RecordNode.GetString("ExecuteMode"));
        }

        public override void Run()
        {
            Stop();
            _BehaviorTreeComponent = new BehaviorTreeComponent(_BehaviorTree);
            _BehaviorTreeComponent.SetExecuteMode(_Mode);
            _BehaviorTreeComponent.Initialize();
            _BehaviorTreeComponent.StartTree();

            RunOutSlot(0);
        }

        public void Stop()
        {
            if(_BehaviorTreeComponent != null)
            {
                _BehaviorTreeComponent.StopTree();
            }
        }

        public override string GetStringContent()
        {
            return BehaviorTreeName;
        }
    }
}
