using EditorUI;
using System;

namespace CrossEditor
{
    internal enum UnaryLogicOp
    {
        Not,
    }

    internal class FlowNode_UnaryLogicOp : FlowNode_StringContent
    {
        private UnaryLogicOp _UnaryLogicOp;

        public FlowNode_UnaryLogicOp(UnaryLogicOp UnaryLogicOp)
        {
            Name = "UnaryLogicOp";
            NodeType = NodeType.Expression;

            _UnaryLogicOp = UnaryLogicOp;

            AddInSlot("Value", SlotType.DataFlow);

            AddOutSlot("Result", SlotType.DataFlow);
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Logical operator.")]
        public UnaryLogicOp UnaryLogicOp
        {
            get { return _UnaryLogicOp; }
            set
            {
                _UnaryLogicOp = value;
            }
        }

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetString("UnaryLogicOp", _UnaryLogicOp.ToString());
        }

        private UnaryLogicOp StringToUnaryLogicOp(string String)
        {
            return Enum.Parse<UnaryLogicOp>(String);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _UnaryLogicOp = StringToUnaryLogicOp(RecordNode.GetString("UnaryLogicOp"));
        }

        public override object Eval(int OutSlotIndex)
        {
            int OutSlotIndex1;
            FlowNode InNode1 = GetInputNode(0, out OutSlotIndex1) as FlowNode;
            if (InNode1 == null)
            {
                CommitInSlotError(0, "slot is not connected.");
                return 0;
            }
            object Value1 = InNode1.Eval(OutSlotIndex1);
            if (Value1 is bool)
            {
                bool Bool1 = (bool)Value1;
                switch (_UnaryLogicOp)
                {
                    case UnaryLogicOp.Not:
                        return !Bool1;

                    default:
                        DebugHelper.Assert(false);
                        break;
                }
            }
            return null;
        }

        public override string GetStringContent()
        {
            switch (_UnaryLogicOp)
            {
                case UnaryLogicOp.Not:
                    return "Not";
            }
            return "<error>";
        }
    }
}