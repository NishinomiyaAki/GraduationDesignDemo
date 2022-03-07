using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace CrossEditor
{
    class BTTask_MoveTo : BTTaskNode
    {
        public string _SelectedEntryName;
        public float _Speed;

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

        public float Speed
        {
            get
            {
                return _Speed;
            }
            set
            {
                _Speed = value;
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

        public BTTask_MoveTo()
        {
            _Name = "Move To";
            _Custom = _Name;

            _NodeType = BTNodeType.Task;

            _SelectedEntryName = "";
            _Speed = 5;

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
            RecordNode.SetFloat("Speed", _Speed);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _Custom = RecordNode.GetString("Custom");
            _SelectedEntryName = RecordNode.GetString("SelectedEntryName");
            _Speed = RecordNode.GetFloat("Speed");
        }

        public override void Execute(BehaviorTreeComponent BTComponent)
        {
            base.Execute(BTComponent);
            BTComponent.SetExecuteNode(this);
        }

        public override void Tick(BehaviorTreeComponent BTComponent, float DeltaTime)
        {
            if(CurrentEntry == null)
            {
                BTComponent.OnTaskFinished(this, ResultType.Failed);
            }
            object Value = BTComponent.GetBlackboardComponent().GetValue(CurrentEntry.EntryName);
            if(Value == null)
            {
                BTComponent.OnTaskFinished(this, ResultType.Failed);
            }
            else
            {
                float X = 0, Y = 0, Z = 0;
                ImportFunc.GetPosition(ref X, ref Y, ref Z);
                float Distance = _Speed * DeltaTime;
                Vector2f CurrentPosition = new Vector2f(X, Z);
                Vector2f Destination = (Vector2f)Value;
                Vector2f Path = Destination - CurrentPosition;
                if(MathF.Sqrt(Path.X*Path.X + Path.Y*Path.Y) <= Distance)
                {
                    ImportFunc.SetPosition(Destination.X, Y, Destination.Y);
                    BTComponent.OnTaskFinished(this, ResultType.Succeeded);
                }
                else
                {
                    Vector2f NextPosition = CurrentPosition + Path.Normalize() * Distance;
                    ImportFunc.SetPosition(NextPosition.X, Y, NextPosition.Y);
                }
            }
        }

        public override string GetStringContent()
        {
            string Content = _Name + " : ";
            if(CurrentEntry == null)
            {
                Content += "Invalid";
            }
            else
            {
                Content += _SelectedEntryName;
            }
            return Content;
        }
    }
}
