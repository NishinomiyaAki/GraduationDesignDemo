using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace CrossEditor
{
    class BTTask_Wait : BTTaskNode
    {
        private float _WaitTime;
        private float _RandomDeviation;

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

        public float WaitTime
        {
            get
            {
                return _WaitTime;
            }
            set
            {
                _WaitTime = value;
            }
        }

        public float RandomDeviation
        {
            get
            {
                return _RandomDeviation;
            }
            set
            {
                _RandomDeviation = value;
            }
        }

        public BTTask_Wait()
        {
            _Name = "Wait";
            _Custom = _Name;

            _NodeType = BTNodeType.Task;

            _WaitTime = 5.0F;
            _RandomDeviation = 0.0F;

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
            RecordNode.SetFloat("WaitTime", _WaitTime);
            RecordNode.SetFloat("RandomDeviation", _RandomDeviation);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _Custom = RecordNode.GetString("Custom");
            _WaitTime = RecordNode.GetFloat("WaitTime");
            _RandomDeviation = RecordNode.GetFloat("RandomDeviation");
        }

        public override void Execute(BehaviorTreeComponent BTComponent)
        {
            base.Execute(BTComponent);
            BTComponent.SetExecuteNode(this);

            Object Memory = BTComponent.GetTaskMemory(this);
            if (Memory == null)
            {
                Random Random = new Random();
                float Deviation = (float)(_RandomDeviation * Random.NextDouble());
                if (Random.Next(0, 100) > 50)
                {
                    Memory = _WaitTime + Deviation;
                }
                else
                {
                    Memory = _WaitTime - Deviation;
                }
                BTComponent.SetTaskMemory(this, Memory);
            }
        }

        public override void Tick(BehaviorTreeComponent BTComponent, float DeltaTime)
        {
            object Memory = BTComponent.GetTaskMemory(this);
            float RemainTime = Convert.ToSingle(Memory);
            RemainTime -= DeltaTime;
            BTComponent.SetTaskMemory(this, RemainTime);
            if(RemainTime <= 0)
            {
                BTComponent.OnTaskFinished(this, ResultType.Succeeded);
            }
        }

        public override string GetStringContent()
        {
            string Content = _Name + " : ";

            Content += _WaitTime.ToString("0.00");

            if(_RandomDeviation > 0.05F)
            {
                Content += "±" + _RandomDeviation.ToString("0.0");
            }

            Content += "s";

            return Content;
        }
    }
}
