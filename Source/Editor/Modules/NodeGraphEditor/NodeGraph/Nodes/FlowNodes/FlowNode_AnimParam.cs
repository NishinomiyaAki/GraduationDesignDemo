using EditorUI;

namespace CrossEditor
{
    class FlowNode_AnimParam : FlowNode_StringContent
    {
        string _Parameter;

        public FlowNode_AnimParam(string String = "")
        {
            Name = "Parameter";
            NodeType = NodeType.Expression;

            _Parameter = String;

            AddOutSlot("Param", SlotType.DataFlow);
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Parameter string.", bReadOnly = true)]
        public string Parameter
        {
            get { return _Parameter; }
            set
            {
                _Parameter = value;
            }
        }

        public override object Eval(int OutSlotIndex)
        {
            if (OutSlotIndex == 0)
            {
                return _Parameter;
            }
            else
            {
                return null;
            }
        }

        public override string GetStringContent()
        {
            return string.Format("\"{0}\"", _Parameter);
        }
        public override string ToExpression()
        {
            return _Parameter;
        }
    }
}
