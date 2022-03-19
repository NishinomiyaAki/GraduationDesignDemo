using System;
using EditorUI;

namespace CrossEditor
{
    enum BinaryArithOp
    {
        Add,
        Substract,
        Multiply,
        Divide,
        Modulo,
    }

    class FlowNode_BinaryArithOp : FlowNode_StringContent
    {
        BinaryArithOp _BinaryArithOp;

        public FlowNode_BinaryArithOp(BinaryArithOp BinaryArithOp = BinaryArithOp.Add)
        {
            Name = "BinaryArithOp";
            NodeType = NodeType.Expression;
            TemplateExpression = "({0}) {1} ({2})";

            _BinaryArithOp = BinaryArithOp;

            AddInSlot("Value1", SlotType.DataFlow);
            AddInSlot("Value2", SlotType.DataFlow);

            AddOutSlot("Result", SlotType.DataFlow);
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Arithmetic operator.")]
        public BinaryArithOp BinaryArithOp
        {
            get { return _BinaryArithOp; }
            set
            {
                _BinaryArithOp = value;
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
            if (Value1 is int && Value2 is int)
            {
                int Int1 = (int)Value1;
                int Int2 = (int)Value2;
                switch (_BinaryArithOp)
                {
                    case BinaryArithOp.Add:
                        return Int1 + Int2;
                    case BinaryArithOp.Substract:
                        return Int1 - Int2;
                    case BinaryArithOp.Multiply:
                        return Int1 * Int2;
                    case BinaryArithOp.Divide:
                        return Int1 / Int2;
                    case BinaryArithOp.Modulo:
                        return Int1 % Int2;
                    default:
                        DebugHelper.Assert(false);
                        break;
                }
            }
            else if (Value1 is float && Value2 is float)
            {
                float Float1 = (float)Value1;
                float Float2 = (float)Value2;
                switch (_BinaryArithOp)
                {
                    case BinaryArithOp.Add:
                        return Float1 + Float2;
                    case BinaryArithOp.Substract:
                        return Float1 - Float2;
                    case BinaryArithOp.Multiply:
                        return Float1 * Float2;
                    case BinaryArithOp.Divide:
                        return Float1 / Float2;
                    default:
                        DebugHelper.Assert(false);
                        break;
                }
            }
            else
            {
                CommitError("In slots type missmatch");
            }
            return 0;
        }

        public override string GetStringContent()
        {
            switch (_BinaryArithOp)
            {
                case BinaryArithOp.Add:
                    return "+";
                case BinaryArithOp.Substract:
                    return "-";
                case BinaryArithOp.Multiply:
                    return "*";
                case BinaryArithOp.Divide:
                    return "/";
                case BinaryArithOp.Modulo:
                    return "%";
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

            // TODO(x): Compatibility check without Eval
            // e.g : float % float is not allowed
            string op = "<error>";
            switch (_BinaryArithOp)
            {
                case BinaryArithOp.Add:
                    op = "+";
                    break;
                case BinaryArithOp.Substract:
                    op = "-";
                    break;
                case BinaryArithOp.Multiply:
                    op = "*";
                    break;
                case BinaryArithOp.Divide:
                    op = "/";
                    break;
                case BinaryArithOp.Modulo:
                    op = "%";
                    break;
            }
            return String.Format(TemplateExpression, InNode1.ToExpression(), op, InNode2.ToExpression());
        }
    }
}
