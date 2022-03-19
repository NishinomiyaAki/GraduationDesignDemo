using System;
using EditorUI;

namespace CrossEditor
{
    enum Relation
    {
        EqualTo,
        InequalTo,
        LowerTo,
        LowerEqualTo,
        GreaterTo,
        GreaterEqualTo,
    }

    class FlowNode_Compare : FlowNode_StringContent
    {
        Relation _Relation;

        public FlowNode_Compare(Relation Relation = Relation.EqualTo)
        {
            Name = "Compare";
            NodeType = NodeType.Expression;
            TemplateExpression = "({0}) {1} ({2})";

            _Relation = Relation;

            AddInSlot("Value1", SlotType.DataFlow);
            AddInSlot("Value2", SlotType.DataFlow);

            AddOutSlot("Result", SlotType.DataFlow);
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Relation.")]
        public Relation Relation
        {
            get { return _Relation; }
            set
            {
                _Relation = value;
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
                switch (_Relation)
                {
                    case Relation.EqualTo:
                        return Int1 == Int2;
                    case Relation.InequalTo:
                        return Int1 != Int2;
                    case Relation.LowerTo:
                        return Int1 < Int2;
                    case Relation.LowerEqualTo:
                        return Int1 <= Int2;
                    case Relation.GreaterTo:
                        return Int1 > Int2;
                    case Relation.GreaterEqualTo:
                        return Int1 >= Int2;
                }
            }
            else if (Value1 is float && Value2 is float)
            {
                float Float1 = (float)Value1;
                float Float2 = (float)Value2;
                switch (_Relation)
                {
                    case Relation.EqualTo:
                        return Float1 == Float2;
                    case Relation.InequalTo:
                        return Float1 != Float2;
                    case Relation.LowerTo:
                        return Float1 < Float2;
                    case Relation.LowerEqualTo:
                        return Float1 <= Float2;
                    case Relation.GreaterTo:
                        return Float1 > Float2;
                    case Relation.GreaterEqualTo:
                        return Float1 >= Float2;
                }
            }
            else
            {
                CommitError("In slots type missmatch");
            }
            return null;
        }

        public string GetOpString(Relation inRelation)
        {
            string op = "<error>";
            switch (inRelation)
            {
                case Relation.EqualTo:
                    op = "==";
                    break;
                case Relation.InequalTo:
                    op = "!=";
                    break;
                case Relation.LowerTo:
                    op = "<";
                    break;
                case Relation.LowerEqualTo:
                    op = "<=";
                    break;
                case Relation.GreaterTo:
                    op = ">";
                    break;
                case Relation.GreaterEqualTo:
                    op = ">=";
                    break;
            }
            return op;
        }

        public override string GetStringContent()
        {
            return GetOpString(_Relation);
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
            return String.Format(TemplateExpression, InNode1.ToExpression(), GetOpString(_Relation), InNode2.ToExpression());
        }
    }
}
