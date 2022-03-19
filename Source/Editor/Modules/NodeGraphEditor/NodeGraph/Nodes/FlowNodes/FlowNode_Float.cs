using EditorUI;

namespace CrossEditor
{
    class FlowNode_Float : FlowNode_StringContent
    {
        float _Value;

        public FlowNode_Float(float Value = 0.0f)
        {
            Name = "Float";
            NodeType = NodeType.Expression;
            TemplateExpression = "{0}";

            _Value = Value;

            AddOutSlot("Value", SlotType.DataFlow);
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Constant float value.")]
        public float Value
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
