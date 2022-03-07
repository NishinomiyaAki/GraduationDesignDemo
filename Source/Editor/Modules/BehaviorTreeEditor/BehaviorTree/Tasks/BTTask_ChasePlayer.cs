using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace CrossEditor
{
    class BTTask_ChasePlayer : BTTaskNode
    {
        public string _SelectedEntryName;
        public float _AcceptDistance;

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

        public float AcceptDistance
        {
            get
            {
                return _AcceptDistance;
            }
            set
            {
                _AcceptDistance = value;
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

        public BTTask_ChasePlayer()
        {
            _Name = "Chase Player";
            _Custom = _Name;

            _NodeType = BTNodeType.Task;

            _SelectedEntryName = "";
            _AcceptDistance = 1.0F;

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
            RecordNode.SetFloat("AcceptDistance", _AcceptDistance);
        }

        public override void LoadFromXml(Record RecordNode)
        {
            base.LoadFromXml(RecordNode);
            _Custom = RecordNode.GetString("Custom");
            _SelectedEntryName = RecordNode.GetString("SelectedEntryName");
            _AcceptDistance = RecordNode.GetFloat("AcceptDistance");
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
            float X = 0, Y = 0, Z = 0;
            ImportFunc.GetPlayerPosition(ref X, ref Y, ref Z);
            Vector2f NewPosition = new Vector2f(X, Z);
            BTComponent.GetBlackboardComponent().SetValue(CurrentEntry.EntryName, NewPosition);

            ImportFunc.GetPosition(ref X, ref Y, ref Z);
            Vector2f Position = new Vector2f(X, Z);
            Vector2f Path = NewPosition - Position;
            float Distance = MathF.Sqrt(Path.X * Path.X + Path.Y * Path.Y);
            if(Distance <= AcceptDistance)
            {
                BTComponent.OnTaskFinished(this, ResultType.Succeeded);
            }
        }

        public override string GetStringContent()
        {
            string Content = "";
            if (CurrentEntry == null)
            {
                Content += "Invalid";
            }
            else
            {
                Content += "Player Position save to " + CurrentEntry.EntryName;
            }
            return Content;
        }
    }
}
