using System;
using EditorUI;

namespace CrossEditor
{
    enum BinaryLogicOp
    {
        And,
        Or,
        Xor,
    }

    class FlowNode_BinaryLogicOp : FlowNode_StringContent
    {
        BinaryLogicOp _BinaryLogicOp;

        public FlowNode_BinaryLogicOp(BinaryLogicOp BinaryLogicOp = BinaryLogicOp.And)
        {
            Name = "BinaryLogicOp";
            TemplateExpression = "({0}) {1} ({2})";
            NodeType = NodeType.Expression;

            _BinaryLogicOp = BinaryLogicOp;

            AddInSlot("Value1", SlotType.DataFlow);
            AddInSlot("Value2", SlotType.DataFlow);

            AddOutSlot("Result", SlotType.DataFlow);
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Logical operator.")]
        public BinaryLogicOp BinaryLogicOp
        {
            get { return _BinaryLogicOp; }
            set
            {
                _BinaryLogicOp = value;
            }
        }

        public override object Eval(int OutSlotIndex)
        {
            int OutSlotIndex1;
            int OutSlotIndex2;
            FlowNode InNode1 = GetInputNode(0, out OutSlotIndex1) as FlowNode;
            FlowNode InNode2 = GetInputNode(1, out OutSlotIndex2) as FlowNode;
            if (InNode1 == null)
            {
                CommitInSlotError(0, "slot is not connected.");
                return 0;
            }
            if (InNode2 == null)
            {
                CommitInSlotError(1, "slot is not connected.");
                return 0;
            }
            object Value1 = InNode1.Eval(OutSlotIndex1);
            object Value2 = InNode2.Eval(OutSlotIndex2);
            if (Value1 is bool && Value2 is bool)
            {
                bool Bool1 = (bool)Value1;
                bool Bool2 = (bool)Value2;
                switch (_BinaryLogicOp)
                {
                    case BinaryLogicOp.And:
                        return Bool1 & Bool2;
                    case BinaryLogicOp.Or:
                        return Bool1 | Bool2;
                    case BinaryLogicOp.Xor:
                        return Bool1 ^ Bool2;
                    default:
                        DebugHelper.Assert(false);
                        break;
                }
            }
            else
            {
                CommitError("In slots type missmatch");
            }
            return null;
        }

        public override string GetStringContent()
        {
            switch (_BinaryLogicOp)
            {
                case BinaryLogicOp.And:
                    return "And";
                case BinaryLogicOp.Or:
                    return "Or";
                case BinaryLogicOp.Xor:
                    return "Xor";
            }
            return "<error>";
        }

        public override string ToExpression()
        {
            int OutSlotIndex1;
            int OutSlotIndex2;
            FlowNode InNode1 = GetInputNode(0, out OutSlotIndex1) as FlowNode;
            FlowNode InNode2 = GetInputNode(1, out OutSlotIndex2) as FlowNode;
            if (InNode1 == null)
            {
                CommitInSlotError(0, "slot is not connected.");
                return "";
            }
            if (InNode2 == null)
            {
                CommitInSlotError(1, "slot is not connected.");
                return "";
            }

            string op = "<error>";
            switch (_BinaryLogicOp)
            {
                case BinaryLogicOp.And:
                    op = "and";
                    break;
                case BinaryLogicOp.Or:
                    op = "or";
                    break;
                case BinaryLogicOp.Xor:
                    op = "xor";
                    break;
            }
            return String.Format(TemplateExpression, InNode1.ToExpression(), op, InNode2.ToExpression());
        }
    }
}
