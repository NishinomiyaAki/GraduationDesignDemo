using EditorUI;

namespace CrossEditor
{
    public class FlowNode_Event : FlowNode_StringContent
    {
        string _EventName;

        public FlowNode_Event(string EventName = "default")
        {
            Name = "Event";
            NodeType = NodeType.Event;

            _EventName = EventName;

            AddOutSlot("Control", SlotType.ControlFlow);
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Event name.")]
        public string EventName
        {
            get { return _EventName; }
            set
            {
                _EventName = value;
            }
        }

        public string GetEventName()
        {
            return _EventName;
        }

        public override void Run()
        {
            RunOutSlot(0);
        }

        public override string GetStringContent()
        {
            return string.Format("\"{0}\"", _EventName);
        }
    }
}