using System;
using EditorUI;

namespace CrossEditor
{
    enum UnaryArithOp
    {
        Negative,
    }

    class FlowNode_UnaryArithOp : FlowNode_StringContent
    {
        UnaryArithOp _UnaryArithOp;

        public FlowNode_UnaryArithOp(UnaryArithOp UnaryArithOp = UnaryArithOp.Negative)
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
