using EditorUI;

namespace CrossEditor
{
    class FlowNode_Bool : FlowNode_StringContent
    {
        bool _Value;

        public FlowNode_Bool(bool Value = false)
        {
            Name = "Bool";
            NodeType = NodeType.Expression;
            TemplateExpression = "{0}";

            _Value = Value;

            AddOutSlot("Value", SlotType.DataFlow);
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Constant boolean value.")]
        public bool Value
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
            return string.Format(TemplateExpression, _Value.ToString().ToLower());
        }
    }
}
