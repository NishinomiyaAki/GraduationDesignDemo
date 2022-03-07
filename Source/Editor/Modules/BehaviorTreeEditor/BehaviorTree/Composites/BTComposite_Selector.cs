using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace Editor
{
    class BTComposite_Selector : BTCompositeNode
    {
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

        public BTComposite_Selector()
        {
            _Name = "Selector";
            _Custom = _Name;

            _NodeType = BTNodeType.Composite;

            SetInSlot();
            SetOutSlot();
        }

        public override void GetContentSize(ref int ContentWidth, ref int ContentHeight)
        {
            int X = 0, Y = 0;
            X = Math.Max(X, _SpanX + _TitleFont.MeasureString_Fast(_Custom));
            Y += _SpanY + _TitleFont.GetCharHeight();

            X = Math.Max(X, _SpanX + _DetailFont.MeasureString_Fast(_Name));
            Y += _SpanY + _DetailFont.GetCharHeight();

            ContentWidth = X + _SpanX;
            ContentHeight = Y + _SpanY;
        }

        public override void DrawContent(int ContentX, int ContentY, int ContentWidth, int ContentHeight)
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            GraphicsHelper.DrawString(_TitleFont, _Custom, Color.EDITOR_UI_GENERAL_TEXT_COLOR, ContentX + _SpanX, ContentY + _SpanY, ContentWidth - 2 * _SpanX, _TitleFont.GetCharHeight(), TextAlign.CenterCenter);
            GraphicsHelper.DrawString(_DetailFont, _Name, Color.EDITOR_UI_GENERAL_TEXT_COLOR, ContentX + _SpanX, ContentY + _TitleFont.GetCharHeight() + 2 * _SpanY, ContentWidth - 2 * _SpanX, _DetailFont.GetCharHeight(), TextAlign.CenterLeft);
        }

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetString("Custom", _Custom);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _Custom = RecordNode.GetString("Custom");
        }

        public override void Execute(BehaviorTreeComponent BTComponent)
        {
            string Message = string.Format("Execute Stream: At {0} Node (Name:{1}, ID:{2})", _Name, _Custom, ID);
            ConsoleUI.GetInstance().AddLogItem(LogMessageType.Information, Message);

            base.Execute(BTComponent);
            BTComponent.SetExecuteNode(this);

            ExecuteInfo Info = BTComponent.GetExecuteInfo(this);
            if(Info.ExecuteIndex < ChildNodes.Count)
            {
                // when one of child nodes succeeded, stop executing and return succeeded
                if(Info.ExecuteIndex != 0 && Info.ChildResults[Info.ExecuteIndex - 1] == ResultType.Succeeded)
                {
                    BTComponent.OnTaskFinished(this, ResultType.Succeeded);
                }
                else
                {
                    BTNode NextChild = ChildNodes[Info.ExecuteIndex];
                    if (NextChild.DoPreDecoratorExecute(BTComponent))
                    {
                        ChildNodes[Info.ExecuteIndex].Execute(BTComponent);
                    }
                    else
                    {
                        BTComponent.OnTaskFinished(NextChild, ResultType.Aborted);
                    }
                }
            }
            else
            {
                if(Info.ChildResults.Count > 0)
                {
                    if(Info.ChildResults[Info.ExecuteIndex - 1] == ResultType.Succeeded)
                    {
                        BTComponent.OnTaskFinished(this, ResultType.Succeeded);
                    }
                    else
                    {
                        BTComponent.OnTaskFinished(this, ResultType.Failed);
                    }
                    
                }
                else
                {
                    BTComponent.OnTaskFinished(this, ResultType.Succeeded);
                }
            }
        }
    }
}
