using EditorUI;
using System;

namespace CrossEditor
{
    internal enum BinaryArithOp
    {
        Add,
        Substract,
        Multiply,
        Divide,
        Modulo,
    }

    internal class FlowNode_BinaryArithOp : FlowNode_StringContent
    {
        private BinaryArithOp _BinaryArithOp;

        public FlowNode_BinaryArithOp(BinaryArithOp BinaryArithOp)
        {
            Name = "BinaryArithOp";
            NodeType = NodeType.Expression;

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

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetString("BinaryArithOp", _BinaryArithOp.ToString());
        }

        private BinaryArithOp StringToBinaryArithOp(string String)
        {
            return Enum.Parse<BinaryArithOp>(String);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _BinaryArithOp = StringToBinaryArithOp(RecordNode.GetString("BinaryArithOp"));
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
                CommitNodeError("In slots type missmatch");
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
    }
}