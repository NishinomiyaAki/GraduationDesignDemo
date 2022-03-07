using EditorUI;

namespace CrossEditor
{
    internal class FlowNode_Float : FlowNode_StringContent
    {
        private float _Value;

        public FlowNode_Float(float Value)
        {
            Name = "Float";
            NodeType = NodeType.Expression;

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

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetFloat("Value", _Value);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _Value = RecordNode.GetFloat("Value");
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
    }
}