using EditorUI;
using System.IO;
namespace CrossEditor
{
    class FlowNode_AnimTimeRemaining : FlowNode_StringContent
    {
        string _Anim;

        public FlowNode_AnimTimeRemaining(string String = "")
        {
            Name = "AnimTimeRemaining";
            NodeType = NodeType.Expression;
            TemplateExpression = "FF_AnimTimeRemaining('{0}')";

            _Anim = String;

            AddOutSlot("TimeRemaining", SlotType.DataFlow);
        }

        [PropertyInfo(PropertyType = "Auto", ToolTips = "Anim path.", bReadOnly = true)]
        public string Anim
        {
            get { return _Anim; }
            set
            {
                _Anim = value;
            }
        }

        public override object Eval(int OutSlotIndex)
        {
            if (OutSlotIndex == 0)
            {
                return _Anim;
            }
            else
            {
                return null;
            }
        }

        public override string GetStringContent()
        {
            return "";
            //return string.Format("'{0}'", AnimUtil.TrimAnimPath(PathHelper.GetFileName(_Anim)));
        }
        public override string ToExpression()
        {
            return string.Format(TemplateExpression, Path.GetFileNameWithoutExtension(_Anim));
        }
    }
}
