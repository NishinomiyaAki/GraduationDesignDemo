using EditorUI;

namespace Editor
{
    internal class FlowNode_Integer : FlowNode_StringContent
    {
        private int _Value;

        public FlowNode_Integer(int Value)
        {
            Name = "Integer";
            NodeType = NodeType.Expression;

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

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetInt("Value", _Value);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _Value = RecordNode.GetInt("Value");
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