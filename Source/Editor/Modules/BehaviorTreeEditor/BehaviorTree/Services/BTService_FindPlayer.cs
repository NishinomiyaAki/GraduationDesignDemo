using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace CrossEditor
{
    class BTService_FindPlayer : BTServiceNode
    {
        public string _SelectedEntryName;

        private BlackboardEntry CurrentEntry
        {
            get
            {
                BehaviorTree CurrentBT = BehaviorTreeManager.GetInstance().GetBehaviorTreeByBTAuxiliaryNode(this);
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

        public BTService_FindPlayer()
        {
            _Name = "Find Player";
            _Custom = _Name;

            _SelectedEntryName = "";
            ExecuteInterval = 0.5F;

            _NodeType = BTNodeType.Service;
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

        public override void Tick(BehaviorTreeComponent BTComponent, float DeltaTime)
        {
            if(CurrentEntry == null)
            {
                return;
            }

            object Memory = BTComponent.GetServiceMemory(this);
            float RemainTime = Convert.ToSingle(Memory);
            if (Memory == null)
            {
                BTComponent.SetServiceMemory(this, ExecuteInterval);
            }

            RemainTime -= DeltaTime;
            if (RemainTime < 0)
            {
                float X = 0, Y = 0, Z = 0;
                ImportFunc.GetPosition(ref X, ref Y, ref Z);
                Vector2f Position = new Vector2f(X, Z);
                ImportFunc.GetPlayerPosition(ref X, ref Y, ref Z);
                Vector2f PlayerPosition = new Vector2f(X, Z);
                Vector2f Path = PlayerPosition - Position;
                float Distance = MathF.Sqrt(Path.X * Path.X + Path.Y * Path.Y);
                BTComponent.GetBlackboardComponent().SetValue(CurrentEntry.EntryName, Distance);
                BTComponent.SetServiceMemory(this, ExecuteInterval);
            }
            else
            {
                BTComponent.SetServiceMemory(this, RemainTime);
            }
        }

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetString("SelectedEntryName", _SelectedEntryName);
        }

        public override void LoadFromXml(Record RecordNode, BehaviorTree BehaviorTree)
        {
            base.LoadFromXml(RecordNode, BehaviorTree);
            _SelectedEntryName = RecordNode.GetString("SelectedEntryName");
        }

        public override void DrawContent(int X, int Y, int Width, int Height)
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            GraphicsHelper.DrawString(_TitleFont, _Custom, Color.EDITOR_UI_GENERAL_TEXT_COLOR, X + _SpanX, Y + _SpanY, Width - 2 * _SpanX, _TitleFont.GetCharHeight(), TextAlign.CenterLeft);
            GraphicsHelper.DrawString(_DetailFont, GetStringContent(), Color.EDITOR_UI_GENERAL_TEXT_COLOR, X + _SpanX, Y + _TitleFont.GetCharHeight() + 2 * _SpanY, Width - 2 * _SpanX, _DetailFont.GetCharHeight(), TextAlign.CenterLeft);
        }

        public override string GetStringContent()
        {
            string Content = "";
            if(CurrentEntry == null)
            {
                Content += "Invalid";
            }
            else
            {
                Content += "Distance save to " + CurrentEntry.EntryName + ", Tick every " + ExecuteInterval.ToString("0.00") + "s";
            }
            return Content;
        }
    }
}
