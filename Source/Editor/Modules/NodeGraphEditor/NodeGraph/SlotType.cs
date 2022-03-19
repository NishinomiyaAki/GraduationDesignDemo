using EditorUI;
using ParameterType = CrossEditor.SlotSubType;
using System.Collections.Generic;

namespace CrossEditor
{
    public enum SlotType
    {
        DataFlow,
        ControlFlow,

        // Anim Link Begin
        LocalPoseLink,
        RootPoseLink,
        ParamImplLink,
        // Anim Link End

        // State Graph Begin
        StateLink,
        // State Graph End
    }
    public enum SlotSubType
    {
        Bool,
        Int,
        Float,
        String,
        Vector2,
        Vector3,
        Vector4,
        Transform,
        Customized,
        Default = Bool
    }

    public static class Extension
    {
        public static string GetSlotSubTypeDesc(this SlotSubType subtype)
        {
            if(subtype == ParameterType.Customized)
            {
                return "void*";
            }
            return subtype.ToString().ToLower();
        }
    }

    public class SlotColor
    {
        static Dictionary<SlotType, Color> _SlotColorMap = new Dictionary<SlotType, Color> {
            {SlotType.DataFlow, Color.FromRGB(110, 220, 120)},
            {SlotType.ControlFlow, Color.FromRGB(220, 70, 89)},
            {SlotType.LocalPoseLink, Color.FromRGB(180, 105, 20)},
            {SlotType.RootPoseLink, Color.FromRGB(96, 132, 91)},
            {SlotType.ParamImplLink, Color.FromRGB(69, 110, 137)},
        };

        public static Color GetColor(SlotType Type)
        {
            if (_SlotColorMap.ContainsKey(Type))
            {
                return _SlotColorMap[Type];
            }
            else
            {
                return Color.FromRGB(110, 220, 120);
            }
        }
    }
}
