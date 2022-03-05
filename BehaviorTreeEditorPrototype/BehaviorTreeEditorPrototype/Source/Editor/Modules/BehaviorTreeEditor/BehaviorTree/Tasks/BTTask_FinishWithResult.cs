using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace CrossEditor
{

    class BTTask_FinishWithResult : BTTaskNode
    {
        private ResultType _ExecuteResult;

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

        public ResultType Result
        {
            get
            {
                return _ExecuteResult;
            }
            set
            {
                _ExecuteResult = value;
            }
        }

        public BTTask_FinishWithResult()
        {
            _Name = "Finish With Result";
            _Custom = _Name;
            _ExecuteResult = ResultType.Succeeded;

            _NodeType = BTNodeType.Task;

            SetInSlot();
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
            RecordNode.SetString("Result", _ExecuteResult.ToString());
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _Custom = RecordNode.GetString("Custom");
            _ExecuteResult = StringToResultType(RecordNode.GetString("Result"));
        }

        public override void Execute(BehaviorTreeComponent BTComponent)
        {
            base.Execute(BTComponent);
            BTComponent.SetExecuteNode(this);
        }

        public override void Tick(BehaviorTreeComponent BTComponent, float DeltaTime)
        {
            if (_ExecuteResult != ResultType.Executing)
            {
                BTComponent.OnTaskFinished(this, _ExecuteResult);
            }
        }

        public override string GetStringContent()
        {
            string Content = _Name + " : ";
            switch (_ExecuteResult)
            {
                case ResultType.Succeeded:
                    return Content + "Succeed";
                case ResultType.Failed:
                    return Content + "Fail";
                case ResultType.Aborted:
                    return Content + "Abort";
                case ResultType.Executing:
                    return Content + "Executing";
            }
            return "<error>";
        }
    }
}
