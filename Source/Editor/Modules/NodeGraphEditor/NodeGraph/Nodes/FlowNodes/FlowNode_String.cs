using EditorUI;

namespace CrossEditor
{
    class FlowNode_String : FlowNode_StringContent
    {
        string _String;

        public FlowNode_String(string String = "")
        {
            Name = "String";
            NodeType = NodeType.Expression;

            _String = String;

            AddOutSlot("String", SlotType.DataFlow);
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Constant string value.")]
        public string String
        {
            get { return _String; }
            set
            {
                _String = value;
            }
        }

        public override object Eval(int OutSlotIndex)
        {
            if (OutSlotIndex == 0)
            {
                return _String;
            }
            else
            {
                return null;
            }
        }

        public override string GetStringContent()
        {
            return string.Format("\"{0}\"", _String);
        }
    }
}
