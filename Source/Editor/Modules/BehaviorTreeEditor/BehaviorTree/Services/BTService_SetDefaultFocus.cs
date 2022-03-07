using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace Editor
{
    class BTService_SetDefaultFocus : BTServiceNode
    {
        public BTService_SetDefaultFocus()
        {
            _Name = "Set Default Focus";
            _Custom = _Name;

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
            object Memory = BTComponent.GetServiceMemory(this);
            float RemainTime = Convert.ToSingle(Memory);
            if(Memory == null || RemainTime <= 0)
            {
                BTComponent.SetServiceMemory(this, ExecuteInterval);
            }

            RemainTime -= DeltaTime;
            if(RemainTime <= 0)
            {
                // do set default focus;
            }
            BTComponent.SetServiceMemory(this, RemainTime);
        }

        public override void DrawContent(int X, int Y, int Width, int Height)
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            GraphicsHelper.DrawString(_TitleFont, _Custom, Color.EDITOR_UI_GENERAL_TEXT_COLOR, X + _SpanX, Y + _SpanY, Width - 2 * _SpanX, _TitleFont.GetCharHeight(), TextAlign.CenterLeft);
            GraphicsHelper.DrawString(_DetailFont, GetStringContent(), Color.EDITOR_UI_GENERAL_TEXT_COLOR, X + _SpanX, Y + _TitleFont.GetCharHeight() + 2 * _SpanY, Width - 2 * _SpanX, _DetailFont.GetCharHeight(), TextAlign.CenterLeft);
        }

        public override string GetStringContent()
        {
            string Content = "Tick every " + ExecuteInterval.ToString("0.00") + "s";
            return Content;
        }
    }
}
