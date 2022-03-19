using EditorUI;

namespace CrossEditor
{
    class FlowNode_Integer : FlowNode_StringContent
    {
        int _Value;

        public FlowNode_Integer(int Value = 0)
        {
            Name = "Integer";
            NodeType = NodeType.Expression;
            TemplateExpression = "{0}";

            _Value = Value;

            AddOutSlot("Value", SlotType.DataFlow);
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Constant integer value.")]
        public int Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
            }
        }

        public override object Eval(int OutSlotIndex)
        {
            if (OutSlotIndex == 0)
            {
                return _Value;
            }
            else
            {
                return null;
            }
        }

        public override string GetStringContent()
        {
            return _Value.ToString();
        }
        public override string ToExpression()
        {
            return string.Format(TemplateExpression, _Value.ToString());
        }
    }
}
