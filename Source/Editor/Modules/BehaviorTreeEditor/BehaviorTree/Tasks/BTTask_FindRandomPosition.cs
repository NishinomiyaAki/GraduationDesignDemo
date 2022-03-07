using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace Editor
{
    class BTTask_FindRandomPosition : BTTaskNode
    {
        public string _SelectedEntryName;
        public float _Radius;

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

        public float Radius
        {
            get
            {
                return _Radius;
            }
            set
            {
                _Radius = value;
            }
        }

        private BlackboardEntry CurrentEntry
        {
            get
            {
                BehaviorTree CurrentBT = BehaviorTreeManager.GetInstance().GetBehaviorTreeByBTNode(this);
                if (CurrentBT == null)
                {
                    return null;
                }
                BlackboardData Blackboard = CurrentBT.Blackboard;
                if (Blackboard == null)
                {
                    return null;
                }
                return Blackboard.GetEntry(_SelectedEntryName);
            }
        }

        public BTTask_FindRandomPosition()
        {
            _Name = "Find Random Position";
            _Custom = _Name;

            _NodeType = BTNodeType.Task;

            _SelectedEntryName = "";
            _Radius = 5;

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
            RecordNode.SetString("SelectedEntryName", _SelectedEntryName);
            RecordNode.SetFloat("Radius", _Radius);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _Custom = RecordNode.GetString("Custom");
            _SelectedEntryName = RecordNode.GetString("SelectedEntryName");
            _Radius = RecordNode.GetFloat("Radius");
        }

        public override void Execute(BehaviorTreeComponent BTComponent)
        {
            base.Execute(BTComponent);
            BTComponent.SetExecuteNode(this);
        }

        public override void Tick(BehaviorTreeComponent BTComponent, float DeltaTime)
        {
            if (CurrentEntry == null)
            {
                BTComponent.OnTaskFinished(this, ResultType.Failed);
            }
            float X = 0, Y = 0, Z = 0;
            ImportFunc.GetPosition(ref X, ref Y, ref Z);
            Vector2f CurrentPosition = new Vector2f(X, Z);
            Vector2f NewPostion = new Vector2f();
            do
            {
                Random Random = new Random();
                float Distance = (float)(_Radius * Random.NextDouble());
                float Radian = (float)(2 * Math.PI * Random.NextDouble());
                Vector2f Movement = new Vector2f((float)(Math.Cos(Radian) * Distance), (float)(Math.Sin(Radian) * Distance));
                NewPostion = CurrentPosition + Movement;
            } while (InMovementArea(NewPostion) == false);
            BTComponent.GetBlackboardComponent().SetValue(CurrentEntry.EntryName, NewPostion);
            BTComponent.OnTaskFinished(this, ResultType.Succeeded);
        }

        public bool InMovementArea(Vector2f Position)
        {
            float XMin = 0, XMax = 0, YMin = 0, YMax = 0;
            ImportFunc.GetMovementArea(ref XMin, ref YMin, ref XMax, ref YMax);
            if (Position.X >= XMin && Position.X <= XMax && Position.Y >= YMin && Position.Y <= YMax)
            {
                return true;
            }
            return false;
        }

        public override string GetStringContent()
        {
            string Content = "Radius : " + _Radius.ToString();
            return Content;
        }
    }
}
