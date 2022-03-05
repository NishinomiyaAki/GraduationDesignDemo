using EditorUI;
using System;

namespace CrossEditor
{
    internal enum UnaryArithOp
    {
        Negative,
    }

    internal class FlowNode_UnaryArithOp : FlowNode_StringContent
    {
        private UnaryArithOp _UnaryArithOp;

        public FlowNode_UnaryArithOp(UnaryArithOp UnaryArithOp)
        {
            Name = "BinaryArithOp";
            NodeType = NodeType.Expression;

            _UnaryArithOp = UnaryArithOp;

            AddInSlot("Value1", SlotType.DataFlow);
            AddInSlot("Value2", SlotType.DataFlow);

            AddOutSlot("Result", SlotType.DataFlow);
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Arithmetic operator.")]
        public UnaryArithOp UnaryArithOp
        {
            get { return _UnaryArithOp; }
            set
            {
                _UnaryArithOp = value;
            }
        }

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetString("UnaryArithOp", _UnaryArithOp.ToString());
        }

        private UnaryArithOp StringToUnaryArithOp(string String)
        {
            return Enum.Parse<UnaryArithOp>(String);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _UnaryArithOp = StringToUnaryArithOp(RecordNode.GetString("UnaryArithOp"));
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
            if (Value1 is int)
            {
                int Int1 = (int)Value1;
                switch (_UnaryArithOp)
                {
                    case UnaryArithOp.Negative:
                        return -Int1;

                    default:
                        DebugHelper.Assert(false);
                        break;
                }
            }
            return 0;
        }

        public override string GetStringContent()
        {
            switch (_UnaryArithOp)
            {
                case UnaryArithOp.Negative:
                    return "-";
            }
            return "<error>";
        }
    }
}