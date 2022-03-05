using EditorUI;

namespace CrossEditor
{
    public class FlowNode_Event : FlowNode_StringContent
    {
        private string _EventName;

        public FlowNode_Event(string EventName)
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

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetString("EventName", _EventName);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _EventName = RecordNode.GetString("EventName");
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