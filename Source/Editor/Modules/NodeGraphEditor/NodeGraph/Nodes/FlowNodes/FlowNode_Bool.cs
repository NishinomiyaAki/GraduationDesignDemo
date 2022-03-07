using EditorUI;

namespace CrossEditor
{
    internal class FlowNode_Bool : FlowNode_StringContent
    {
        private bool _Value;

        public FlowNode_Bool(bool Value)
        {
            Name = "Bool";
            NodeType = NodeType.Expression;

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

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetBool("Value", _Value);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _Value = RecordNode.GetBool("Value");
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