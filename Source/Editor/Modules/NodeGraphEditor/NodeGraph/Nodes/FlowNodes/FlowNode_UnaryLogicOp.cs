using System;
using EditorUI;

namespace CrossEditor
{
    enum UnaryLogicOp
    {
        Not,
    }

    class FlowNode_UnaryLogicOp : FlowNode_StringContent
    {
        UnaryLogicOp _UnaryLogicOp;

        public FlowNode_UnaryLogicOp(UnaryLogicOp UnaryLogicOp = UnaryLogicOp.Not)
        {

            TemplateExpression = "({0} ({1}))";

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

        public override string ToExpression()
        {
            int OutSlotIndex1;
            FlowNode InNode1 = GetInputNode(0, out OutSlotIndex1) as FlowNode;

            if (InNode1 == null)
            {
                CommitInSlotError(0, "slot is not connected.");
                return "";
            }
            string op = "<error>";
            switch (_UnaryLogicOp)
            {
                case UnaryLogicOp.Not:
                    {
                        op = "not";
                        break;
                    }
                default:
                    {
                        CommitError("op type missmatch" + _UnaryLogicOp.ToString());
                        break;
                    }
            }

            return String.Format(TemplateExpression, op, InNode1.ToExpression());
        }
    }
}
