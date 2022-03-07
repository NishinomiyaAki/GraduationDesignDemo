using EditorUI;
using System;

namespace Editor
{
    internal enum Relation
    {
        EqualTo,
        InequalTo,
        LowerTo,
        LowerEqualTo,
        GreaterTo,
        GreaterEqualTo,
    }

    internal class FlowNode_Compare : FlowNode_StringContent
    {
        private Relation _Relation;

        public FlowNode_Compare(Relation Relation)
        {
            Name = "Compare";
            NodeType = NodeType.Expression;

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

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetString("Relation", _Relation.ToString());
        }

        private Relation StringToRelation(string String)
        {
            return Enum.Parse<Relation>(String);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _Relation = StringToRelation(RecordNode.GetString("Relation"));
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
                CommitNodeError("In slots type missmatch");
            }
            return null;
        }

        public override string GetStringContent()
        {
            switch (_Relation)
            {
                case Relation.EqualTo:
                    return "==";

                case Relation.InequalTo:
                    return "!=";

                case Relation.LowerTo:
                    return "<";

                case Relation.LowerEqualTo:
                    return "<=";

                case Relation.GreaterTo:
                    return ">";

                case Relation.GreaterEqualTo:
                    return ">=";
            }
            return "<error>";
        }
    }
}