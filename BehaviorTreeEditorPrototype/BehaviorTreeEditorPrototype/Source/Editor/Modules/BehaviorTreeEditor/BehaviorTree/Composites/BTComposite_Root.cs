using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace CrossEditor
{
    class BTComposite_Root : BTCompositeNode
    {
        public BlackboardData _Blackboard;

        [PropertyInfo(ToolTips = "Blackboard Asset")]
        public string BlackboardName
        {
            get
            {
                if(_Blackboard == null)
                {
                    return "None";
                }
                else
                {
                    return _Blackboard.Name;
                }
            }
            set
            {
                if(value == "None")
                {
                    _Blackboard = null;
                }
                else
                {
                    _Blackboard = BlackboardManager.GetInstance().GetBlackboardByName(value);
                }
                DoLayoutWithConnections();
            }
        }

        public BTComposite_Root()
        {
            _Name = "Root";
            _Custom = _Name;
            _ExecuteIndex = 0;
            _Blackboard = null;

            _NodeType = BTNodeType.Composite;

            SetOutSlot();
        }

        public override void GetContentSize(ref int ContentWidth, ref int ContentHeight)
        {

            int X = 0, Y = 0;
            X = Math.Max(X,_SpanX + _TitleFont.MeasureString_Fast(_Custom));
            Y += _SpanY + _TitleFont.GetCharHeight();

            X = Math.Max(X, _SpanX + _DetailFont.MeasureString_Fast(BlackboardName));
            Y += _SpanY + _DetailFont.GetCharHeight();

            ContentWidth = X + _SpanX;
            ContentHeight = Y + _SpanY;
        }

        public override void DrawContent(int ContentX, int ContentY, int ContentWidth, int ContentHeight)
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            GraphicsHelper.DrawString(_TitleFont, _Custom, Color.EDITOR_UI_GENERAL_TEXT_COLOR, ContentX + _SpanX, ContentY + _SpanY, ContentWidth - 2 * _SpanX, _TitleFont.GetCharHeight(), TextAlign.CenterCenter);
            GraphicsHelper.DrawString(_DetailFont, BlackboardName, Color.EDITOR_UI_GENERAL_TEXT_COLOR, ContentX + _SpanX, ContentY + _TitleFont.GetCharHeight() + 2 * _SpanY, ContentWidth - 2 * _SpanX, _DetailFont.GetCharHeight(), TextAlign.CenterLeft);
        }

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetString("Custom", _Custom);
            RecordNode.SetString("Blackboard", BlackboardName);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _Custom = RecordNode.GetString("Custom");
            BlackboardName = RecordNode.GetString("Blackboard");
        }

        public override void Execute(BehaviorTreeComponent BTComponent)
        {
            string Message = string.Format("Execute Stream: At {0} Node (Name:{1}, ID:{2})", _Name, _Custom, ID);
            ConsoleUI.GetInstance().AddLogItem(LogMessageType.Information, Message);

            base.Execute(BTComponent);
            BTComponent.SetExecuteNode(this);
            ExecuteInfo Info = BTComponent.GetExecuteInfo(this);

            if (Info.ExecuteIndex < ChildNodes.Count)
            {
                BTNode NextChild = ChildNodes[Info.ExecuteIndex];
                if (NextChild.DoPreDecoratorExecute(BTComponent))
                {
                    NextChild.Execute(BTComponent);
                }
                else
                {
                    BTComponent.OnTaskFinished(NextChild, ResultType.Aborted);
                }
            }
            else
            {
                if(Info.ChildResults.Count > 0)
                {
                    BTComponent.OnTaskFinished(this, Info.ChildResults[0]);
                }
                else
                {
                    BTComponent.OnTaskFinished(this, ResultType.Succeeded);
                }
            }
        }
    }
}
