namespace CrossEditor
{
    internal class InspectorUI
    {
        private static InspectorUI Instance = new InspectorUI();
        private object _InspectedObject;

        public static InspectorUI GetInstance()
        {
            return Instance;
        }

        public void SetObjectInspected(object Object)
        {
            _InspectedObject = Object;
        }

        public void InspectObject()
        {
            InspectorManager InspectorManager = InspectorManager.GetInstance();
            InspectorManager.ClearInspector();
            int Y = 0;
            if (_InspectedObject != null)
            {
                Panel Container = InspectorManager.GetPanel();
                if (_InspectedObject is BTComposite_Root)
                {
                    Inspector_Blackboard Inspector_Blackboard = new Inspector_Blackboard();
                    Inspector_Blackboard.Inspect(Container, _InspectedObject, "BlackboardName", "Behavior Tree");
                    Inspector_Blackboard.UpdateLayout(Container.Width, ref Y);
                }
                else if (_InspectedObject is Node)
                {
                    Inspector_Node Inspector_Node = new Inspector_Node();
                    Inspector_Node.Inspect(Container, _InspectedObject, null, null);
                    Inspector_Node.UpdateLayout(Container.Width, ref Y);

                    if(_InspectedObject is BTDecorator_BlackboardBased)
                    {
                        Inspector_BlackboardBased Inspector_BlackboardBased = new Inspector_BlackboardBased();
                        Inspector_BlackboardBased.Inspect(Container, _InspectedObject, null, null);
                        Inspector_BlackboardBased.UpdateLayout(Container.Width, ref Y);
                    }
                    else if (_InspectedObject is BTTask_MoveTo || 
                        _InspectedObject is BTTask_FindRandomPosition ||
                        _InspectedObject is BTTask_ChasePlayer)
                    {
                        Inspector_EntrySelector Inspector_EntrySelector = new Inspector_EntrySelector();
                        Inspector_EntrySelector.Inspect(Container, _InspectedObject, typeof(Vector2f), null);
                        Inspector_EntrySelector.UpdateLayout(Container.Width, ref Y);
                    }
                    else if (_InspectedObject is FlowNode_RunBehaviorTree)
                    {
                        Inspector_BehaviorTree Inspector_BehaviorTree = new Inspector_BehaviorTree();
                        Inspector_BehaviorTree.Inspect(Container, _InspectedObject, "BehaviorTreeName", "Behavior Tree Asset");
                        Inspector_BehaviorTree.UpdateLayout(Container.Width, ref Y);
                    }
                    else if (_InspectedObject is BTService_FindPlayer)
                    {
                        Inspector_EntrySelector Inspector_EntrySelector = new Inspector_EntrySelector();
                        Inspector_EntrySelector.Inspect(Container, _InspectedObject, typeof(float), null);
                        Inspector_EntrySelector.UpdateLayout(Container.Width, ref Y);
                    }
                }
                else if(_InspectedObject is BlackboardEntry)
                {
                    BlackboardManager BlackboardManager = BlackboardManager.GetInstance();
                    BlackboardEntry Entry = _InspectedObject as BlackboardEntry;
                    bool Editable = BlackboardUI.GetInstance().CheckBlackboard(BlackboardManager.GetBlackboardByEntry(Entry));

                    Inspector_BlackboardEntry Inspector_BlackboardEntry = new Inspector_BlackboardEntry(Editable);
                    Inspector_BlackboardEntry.Inspect(Container, _InspectedObject, null, null);
                    Inspector_BlackboardEntry.UpdateLayout(Container.Width, ref Y);

                    if (Editable)
                    {
                        Inspector_Blackboard Inspector_Blackboard = new Inspector_Blackboard();
                        Inspector_Blackboard.Inspect(Container, BlackboardManager.GetBlackboardByEntry(Entry), "ParentName", "Parent");
                        Inspector_Blackboard.UpdateLayout(Container.Width, ref Y);
                    }
                }
                Container.Refresh();
            }
        }

        public object GetObjectInspected()
        {
            return _InspectedObject;
        }
    }
}