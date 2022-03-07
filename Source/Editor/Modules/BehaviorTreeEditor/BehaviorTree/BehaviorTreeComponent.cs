using System;
using System.Collections.Generic;
//using System.Threading;
//using System.Windows.Forms;
using System.Timers;

namespace CrossEditor
{
    struct ExecuteInfo
    {
        public int ExecuteIndex;
        public List<ResultType> ChildResults;
        public List<ResultType> MainChildResults;
    }

    enum ExecuteMode
    {
        Single,
        Looped
    }

    class BehaviorTreeComponent
    {
        BehaviorTree _BehaviorTree;
        BlackboardComponent _BlackboardComponent;

        BTNode _RootNode;
        BTNode _ExecuteNode;
        
        List<BTNode> _ParallelNodes;
        List<BTNode> _ActiveNodes;
        List<BTTaskNode> _ParallelTaskNodes;
        List<BTServiceNode> _ServiceNodes;
        List<BTServiceNode> _ExecuteServiceNodes;
        List<BTDecoratorNode> _ExecuteDecoratorNodes;
        // Memory
        Dictionary<BTCompositeNode, ExecuteInfo> _ExecuteInfos;
        Dictionary<BTTaskNode, object> _TaskMemory;
        Dictionary<BTServiceNode, object> _ServiceMemory;

        Timer _Timer;
        ExecuteMode _ExecuteMode;

        public BehaviorTreeComponent(BehaviorTree BehaviorTree)
        {
            _BehaviorTree = BehaviorTree;
            _RootNode = _BehaviorTree.RootNode;
            _ExecuteMode = ExecuteMode.Single;
            _Timer = new Timer(100);
            _Timer.Elapsed += Tick;
            _Timer.AutoReset = true;
            // synchronize with edit window
            _Timer.SynchronizingObject = MainUI.GetInstance().EditWindow;
        }

        public void SetExecuteMode(ExecuteMode Mode)
        {
            _ExecuteMode = Mode;
        }

        private void Tick(object sender, ElapsedEventArgs e)
        {
            float DeltaTime = (float)(sender as Timer).Interval / 1000;
            if (_ExecuteNode != null)
            {
                // avoid conflict
                lock (_ExecuteNode)
                {
                    // search and determine execute stream
                    _ExecuteNode.Execute(this);
                    if (_ExecuteNode == null)
                    {
                        return;
                    }

                    // tick service node on current execute stream
                    for (int i = 0; i < _ExecuteServiceNodes.Count; i++)
                    {
                        _ExecuteServiceNodes[i].Tick(this, DeltaTime);
                    }

                    // tick on execute task node
                    _ExecuteNode.Tick(this, DeltaTime);

                    // check condition every tick
                    DoCheckCondition();

                    for (int i = 0; i < _ParallelTaskNodes.Count; i++)
                    {
                        _ParallelTaskNodes[i].Tick(this, DeltaTime);
                    }
                }
            }
        }

        public void Initialize()
        {
            _ExecuteNode = null;
            _ParallelNodes = new List<BTNode>();
            _ActiveNodes = new List<BTNode>();
            _ParallelTaskNodes = new List<BTTaskNode>();
            _ServiceNodes = new List<BTServiceNode>();
            _ExecuteServiceNodes = new List<BTServiceNode>();
            _ExecuteDecoratorNodes = new List<BTDecoratorNode>();
            _ExecuteInfos = new Dictionary<BTCompositeNode, ExecuteInfo>();
            _TaskMemory = new Dictionary<BTTaskNode, object>();
            _ServiceMemory = new Dictionary<BTServiceNode, object>();

            _BlackboardComponent = new BlackboardComponent();
            _BlackboardComponent.UseBlackboard((_RootNode as BTComposite_Root)._Blackboard);

            InitExecuteNodes();
            foreach (BTNode Node in _ActiveNodes)
            {
                if (Node is BTCompositeNode)
                {
                    ExecuteInfo ExecuteInfo = new ExecuteInfo();
                    ExecuteInfo.ExecuteIndex = 0;
                    ExecuteInfo.ChildResults = new List<ResultType>();
                    ExecuteInfo.MainChildResults = new List<ResultType>();
                    _ExecuteInfos.Add(Node as BTCompositeNode, ExecuteInfo);
                }
                else if (Node is BTTaskNode)
                {
                    _TaskMemory.Add(Node as BTTaskNode, null);
                }
            }
            foreach (BTServiceNode ServiceNode in _ServiceNodes)
            {
                _ServiceMemory.Add(ServiceNode, null);
            }
        }

        public BlackboardComponent GetBlackboardComponent()
        {
            return _BlackboardComponent;
        }

        public void InitExecuteNodes()
        {
            _BehaviorTree.ResetExecuteIndex();
            _BehaviorTree.CalcExecuteIndex(_RootNode, 0);
            Stack<BTNode> Stack = new Stack<BTNode>();
            Stack.Push(_RootNode);
            while (Stack.Count > 0)
            {
                BTNode Node = Stack.Pop();
                _ActiveNodes.Add(Node);
                foreach (Node MainChildNode in Node.GetMainChildNodes())
                {
                    Stack.Push(MainChildNode as BTNode);
                }
                foreach (Node ChildNode in Node.GetChildNodes())
                {
                    Stack.Push(ChildNode as BTNode);
                }
                foreach (BTServiceNode ServiceNode in Node.GetServices())
                {
                    _ServiceNodes.Add(ServiceNode);
                }
            }
        }

        public void StartTree()
        {
            _ExecuteNode = _RootNode;
            _Timer.Start();
        }

        public void StopTree()
        {
            _BehaviorTree.ClearExecute();
            _Timer.Stop();

            Initialize();
        }

        public void ResetTreeMemory()
        {
            foreach (BTNode Node in _ActiveNodes)
            {
                if (Node is BTCompositeNode)
                {
                    ExecuteInfo ExecuteInfo = new ExecuteInfo();
                    ExecuteInfo.ExecuteIndex = 0;
                    ExecuteInfo.ChildResults = new List<ResultType>();
                    ExecuteInfo.MainChildResults = new List<ResultType>();
                    _ExecuteInfos[Node as BTCompositeNode] = ExecuteInfo;
                }
                else if (Node is BTTaskNode)
                {
                    _TaskMemory[Node as BTTaskNode] = null;
                }
            }
            foreach (BTServiceNode ServiceNode in _ServiceNodes)
            {
                _ServiceMemory[ServiceNode] = null;
            }
        }

        public void SetExecuteNode(BTNode BTNode)
        {
            _ExecuteNode = BTNode;
            BTNode._bExecuting = true;
        }

        public ExecuteInfo GetExecuteInfo(BTCompositeNode BTCompositeNode)
        {
            ExecuteInfo ExecuteInfo;
            _ExecuteInfos.TryGetValue(BTCompositeNode, out ExecuteInfo);
            return ExecuteInfo;
        }

        public object GetTaskMemory(BTTaskNode BTTaskNode)
        {
            object Memory;
            _TaskMemory.TryGetValue(BTTaskNode, out Memory);
            return Memory;
        }

        public void SetTaskMemory(BTTaskNode BTTaskNode, object Memory)
        {
            if (_TaskMemory.ContainsKey(BTTaskNode))
            {
                _TaskMemory[BTTaskNode] = Memory;
            }
        }

        public object GetServiceMemory(BTServiceNode BTServiceNode)
        {
            object Memory;
            _ServiceMemory.TryGetValue(BTServiceNode, out Memory);
            return Memory;
        }

        public void SetServiceMemory(BTServiceNode BTServiceNode, object Memory)
        {
            if (_ServiceMemory.ContainsKey(BTServiceNode))
            {
                _ServiceMemory[BTServiceNode] = Memory;
            }
        }

        public void OnTaskFinished(BTNode BTNode, ResultType TaskResult)
        {
            string Message = string.Format("Execute Finish: {0} Node (Name:{1}, ID:{2}) return with {3}", BTNode._Name, BTNode._Custom, BTNode.ID, TaskResult.ToString());
            ConsoleUI.GetInstance().AddLogItem(LogMessageType.Information, Message);

            BTNode._bExecuting = false;
            // remove service node
            for (int i = 0; i < BTNode.GetServices().Count; i++)
            {
                BTServiceNode Service = BTNode.GetServices()[i] as BTServiceNode;
                RemoveServiceNode(Service);
            }
            // remove decorator node
            for (int i = 0; i < BTNode.GetDecorators().Count; i++)
            {
                BTDecoratorNode Decorator = BTNode.GetDecorators()[i] as BTDecoratorNode;
                if (Decorator.Abort == AbortMode.Self)
                {
                    RemoveDecoratorNode(Decorator);
                }
            }

            BTCompositeNode ParentNode = BTNode.GetParentNode() as BTCompositeNode;

            // when root node finish executing, stop tree at first
            if (ParentNode == null)
            {
                if (_ExecuteMode == ExecuteMode.Looped)
                {
                    _Timer.Stop();
                    ResetTreeMemory();
                    //ConsoleUI.GetInstance().ClearAll();
                    StartTree();
                }
                else
                {
                    StopTree();
                }
                return;
            }
            ExecuteInfo ParentInfo = _ExecuteInfos[ParentNode];

            // change parent execute info
            if (ParentNode is BTComposite_Parallel)
            {
                BTComposite_Parallel ParentParallelNode = ParentNode as BTComposite_Parallel;
                if (ParentParallelNode.IsMainChild(BTNode))
                {
                    ParentInfo.MainChildResults.Add(TaskResult);
                }
                else
                {
                    ParentInfo.ChildResults.Add(TaskResult);
                }
            }
            else
            {
                ParentInfo.ChildResults.Add(TaskResult);
            }

            ParentInfo.ExecuteIndex += 1;
            _ExecuteInfos[ParentNode] = ParentInfo;

            if (IsOnParallel(ParentNode))
            {
                BTNode Backup = _ExecuteNode;
                ParentNode.Execute(this);
                AddParallelTaskNode(_ExecuteNode);
                if (BTNode is BTTaskNode && _ParallelTaskNodes.Contains(BTNode as BTTaskNode))
                {
                    _ParallelTaskNodes.Remove(BTNode as BTTaskNode);
                }
                _ExecuteNode = Backup;
            }
            else
            {
                // back to parent node and research execute stream
                ParentNode.Execute(this);
            }
        }

        public bool IsOnParallel(BTNode BTNode)
        {
            if (_ParallelNodes.Count == 0)
            {
                return false;
            }

            BTNode Temp = BTNode;
            while (Temp != null)
            {
                if (_ParallelNodes.Contains(Temp))
                {
                    return true;
                }
                Temp = Temp.GetParentNode();
            }

            return false;
        }

        public void AddParallelNode(BTNode BTNode)
        {
            if (_ParallelNodes.Contains(BTNode) == false)
            {
                _ParallelNodes.Add(BTNode);
                BTNode Backup = _ExecuteNode;
                BTNode.Execute(this);
                AddParallelTaskNode(_ExecuteNode);
                _ExecuteNode = Backup;
            }
        }

        public void AddParallelTaskNode(BTNode BTNode)
        {
            if (BTNode is BTTaskNode)
            {
                if (_ParallelTaskNodes.Contains(BTNode as BTTaskNode) == false)
                {
                    _ParallelTaskNodes.Add(BTNode as BTTaskNode);
                }
            }
        }

        public void RemoveParallelNode(BTNode BTNode)
        {
            if (BTNode == null)
            {
                return;
            }

            Stack<BTNode> Stack = new Stack<BTNode>();
            Stack.Push(BTNode);
            while (Stack.Count > 0)
            {
                BTNode Node = Stack.Pop();
                Node._bExecuting = false;
                foreach (BTNode Child in Node.GetChildNodes())
                {
                    Stack.Push(Child);
                }
                foreach (BTNode Child in Node.GetMainChildNodes())
                {
                    Stack.Push(Child);
                }
                if (_ParallelNodes.Contains(Node))
                {
                    _ParallelNodes.Remove(Node);
                }
                if (Node is BTTaskNode && _ParallelTaskNodes.Contains(Node as BTTaskNode))
                {
                    _ParallelTaskNodes.Remove(Node as BTTaskNode);
                }
            }
        }

        public void AddServiceNode(BTServiceNode BTServiceNode)
        {
            if (_ExecuteServiceNodes.Contains(BTServiceNode) == false)
            {
                _ExecuteServiceNodes.Add(BTServiceNode);
            }
        }

        public void RemoveServiceNode(BTServiceNode BTServiceNode)
        {
            if (_ExecuteServiceNodes.Contains(BTServiceNode) == true)
            {
                _ExecuteServiceNodes.Remove(BTServiceNode);
            }
        }

        public void AddDecoratorNode(BTDecoratorNode BTDecoratorNode)
        {
            if (_ExecuteDecoratorNodes.Contains(BTDecoratorNode) == false)
            {
                _ExecuteDecoratorNodes.Add(BTDecoratorNode);
            }
        }

        public void RemoveDecoratorNode(BTDecoratorNode BTDecoratorNode)
        {
            if (_ExecuteDecoratorNodes.Contains(BTDecoratorNode) == true)
            {
                _ExecuteDecoratorNodes.Remove(BTDecoratorNode);
            }
        }

        public void DoCheckCondition()
        {
            for (int i = 0; i < _ExecuteDecoratorNodes.Count; i++)
            {
                BTDecoratorNode DecoratorNode = _ExecuteDecoratorNodes[i];
                bool bAllow = DecoratorNode.CheckCondition(this);
                if (bAllow == false)
                {
                    string Message = string.Format("Condition Failed: At {0} Node (Name:{1}, ID:{2})", DecoratorNode._Name, DecoratorNode._Custom, DecoratorNode.ID);
                    ConsoleUI.GetInstance().AddLogItem(LogMessageType.Information, Message);
                    // recursively remove parent node
                    RemoveParallelNode(DecoratorNode._ParentNode);
                    OnTaskFinished(DecoratorNode._ParentNode, ResultType.Aborted);
                }
            }
        }
    }
}
