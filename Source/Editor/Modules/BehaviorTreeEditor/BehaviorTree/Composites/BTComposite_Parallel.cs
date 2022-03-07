using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace CrossEditor
{
    enum ParallelMode
    {
        Immediate,
        Delayed
    }

    class BTComposite_Parallel : BTCompositeNode
    {
        public ParallelMode _FinishMode;

        public ParallelMode FinishMode
        {
            get
            {
                return _FinishMode;
            }
            set
            {
                _FinishMode = value;
            }
        }

        public string NodeName
        {
            get
            {
                return _Custom;
            }
            set
            {
                _Custom = value;
            }
        }

        public BTComposite_Parallel()
        {
            _Name = "Simple Parallel";
            _Custom = _Name;

            _NodeType = BTNodeType.Composite;

            SetInSlot();
            SetMainOutSlot();
            SetOutSlot();
        }

        public override void GetContentSize(ref int ContentWidth, ref int ContentHeight)
        {
            int X = 0, Y = 0;
            X = Math.Max(X, _SpanX + _TitleFont.MeasureString_Fast(_Custom));
            Y += _SpanY + _TitleFont.GetCharHeight();

            X = Math.Max(X, _SpanX + _DetailFont.MeasureString_Fast(GetStringContent()));
            Y += _SpanY + _DetailFont.GetCharHeight();

            ContentWidth = X + _SpanX;
            ContentHeight = Y + _SpanY;
        }

        public override void DrawContent(int ContentX, int ContentY, int ContentWidth, int ContentHeight)
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            GraphicsHelper.DrawString(_TitleFont, _Custom, Color.EDITOR_UI_GENERAL_TEXT_COLOR, ContentX + _SpanX, ContentY + _SpanY, ContentWidth - 2 * _SpanX, _TitleFont.GetCharHeight(), TextAlign.CenterCenter);
            GraphicsHelper.DrawString(_DetailFont, GetStringContent(), Color.EDITOR_UI_GENERAL_TEXT_COLOR, ContentX + _SpanX, ContentY + _TitleFont.GetCharHeight() + 2 * _SpanY, ContentWidth - 2 * _SpanX, _DetailFont.GetCharHeight(), TextAlign.CenterLeft);
        }

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetString("Custom", _Custom);
            RecordNode.SetString("FinishMode", _FinishMode.ToString());
        }

        public ParallelMode StringToParallelMode(string String)
        {
            return Enum.Parse<ParallelMode>(String);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _Custom = RecordNode.GetString("Custom");
            _FinishMode = StringToParallelMode(RecordNode.GetString("FinishMode"));
        }

        public BTTaskNode GetMainChild()
        {
            if(_MainOutSlot.GetConnections().Count > 0)
            {
                return _MainOutSlot.GetConnections()[0].InSlot.Node as BTTaskNode;
            }
            else
            {
                return null;
            }
        }

        public BTNode GetSubChild()
        {
            if(_OutSlot.GetConnections().Count > 0)
            {
                return _OutSlot.GetConnections()[0].InSlot.Node as BTNode;
            }
            else
            {
                return null;
            }
        }

        public bool IsMainChild(BTNode Child)
        {
            return Child == GetMainChild();
        }

        public override void Execute(BehaviorTreeComponent BTComponent)
        {
            string Message = string.Format("Execute Stream: At {0} Node (Name:{1}, ID:{2})", _Name, _Custom, ID);
            ConsoleUI.GetInstance().AddLogItem(LogMessageType.Information, Message);

            base.Execute(BTComponent);
            BTComponent.SetExecuteNode(this);

            ExecuteInfo Info = BTComponent.GetExecuteInfo(this);
            if (_FinishMode == ParallelMode.Immediate)
            {
                if (Info.ExecuteIndex == 0)
                {
                    if (GetMainChild() != null)
                    {
                        if (GetSubChild() != null)
                        {
                            if (GetSubChild().DoPreDecoratorExecute(BTComponent))
                            {
                                BTComponent.AddParallelNode(GetSubChild());
                            }
                            else
                            {
                                BTComponent.OnTaskFinished(GetSubChild(), ResultType.Aborted);
                            }
                        }

                        if (GetMainChild().DoPreDecoratorExecute(BTComponent))
                        {
                            GetMainChild().Execute(BTComponent);
                        }
                        else
                        {
                            BTComponent.OnTaskFinished(GetMainChild(), ResultType.Aborted);
                        }
                    }
                    else
                    {
                        BTComponent.OnTaskFinished(this, ResultType.Succeeded);
                    }
                }
                else if (Info.ExecuteIndex == 1)
                {
                    if(Info.MainChildResults.Count > 0)
                    {
                        BTComponent.RemoveParallelNode(GetSubChild());
                        BTComponent.OnTaskFinished(this, Info.MainChildResults[0]);
                    }

                    if(Info.ChildResults.Count > 0)
                    {
                        BTComponent.RemoveParallelNode(GetSubChild());
                    }
                }
                else if (Info.ExecuteIndex == 2)
                {
                    BTComponent.OnTaskFinished(this, Info.MainChildResults[0]);
                }
            }

            if (_FinishMode == ParallelMode.Delayed)
            {
                if (Info.ExecuteIndex == 0)
                {
                    if (GetSubChild() != null)
                    {
                        if (GetSubChild().DoPreDecoratorExecute(BTComponent))
                        {
                            BTComponent.AddParallelNode(GetSubChild());
                        }
                        else
                        {
                            BTComponent.OnTaskFinished(GetSubChild(), ResultType.Aborted);
                        }
                    }

                    if (GetMainChild() != null)
                    {
                        if (GetMainChild().DoPreDecoratorExecute(BTComponent))
                        {
                            GetMainChild().Execute(BTComponent);
                        }
                        else
                        {
                            BTComponent.OnTaskFinished(GetMainChild(), ResultType.Aborted);
                        }
                    }

                    if(GetMainChild() == null && GetSubChild() == null)
                    {
                        BTComponent.OnTaskFinished(this, ResultType.Succeeded);
                    }
                }
                else if (Info.ExecuteIndex == 1)
                {
                    if(Info.ChildResults.Count > 0)
                    {
                        BTComponent.RemoveParallelNode(GetSubChild());
                        if (GetMainChild() == null)
                        {
                            BTComponent.OnTaskFinished(this, Info.ChildResults[0]);
                        }
                        else
                        {
                            GetMainChild().Execute(BTComponent);
                        }
                    }

                    if(Info.MainChildResults.Count > 0)
                    {
                        if(GetSubChild() == null)
                        {
                            BTComponent.OnTaskFinished(this, Info.MainChildResults[0]);
                        }
                        else
                        {
                            ConsoleUI.GetInstance().AddLogItem(LogMessageType.Information, "Execute Stream: Wait for subtree executing...");
                        }
                    } 
                }
                else if (Info.ExecuteIndex == 2)
                {
                    BTComponent.RemoveParallelNode(GetSubChild());
                    BTComponent.OnTaskFinished(this, Info.MainChildResults[0]);
                }
            }
        }

        public override string GetStringContent()
        {
            string Content = _Name + " : ";
            switch (_FinishMode)
            {
                case ParallelMode.Immediate:
                    Content += "Finish with main task";
                    break;
                case ParallelMode.Delayed:
                    Content += "Wait for subtree";
                    break;
            }
            return Content;
        }
    }
}
