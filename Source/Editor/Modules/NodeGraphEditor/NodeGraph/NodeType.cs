using EditorUI;
using System.Collections.Generic;

namespace CrossEditor
{
    public enum NodeType
    {
        Unknown,
        Expression,
        Statement,
        ControlFlow,
        Event,
        Texture,
        Material,
        Color,
        Coordinate,
        Const,
        Math,
        Parameter,
        Function,
        Others,
        VectorOperation,
        Utility,
        Misc,
		AnimNode,
        Comment,
        Particle,
        Sky,
        VirtualTexture,
        MaterialAttributes,
        Water,
        Volume,
        Custom,
        Shading,
        Hair,
        Font,
        Atmosphere,
        Tree,
        Terrain,
    }

    public class NodeColor
    {
        static Dictionary<NodeType, Color> _NodeColorMap = new Dictionary<NodeType, Color> {
            {NodeType.Unknown, Color.FromRGB(0, 0, 0) },
            {NodeType.Expression, Color.FromRGB(96, 132, 91) },
            {NodeType.Statement, Color.FromRGB(69, 110, 137) },
            {NodeType.ControlFlow, Color.FromRGB(180, 105, 20) },
            {NodeType.Event, Color.FromRGB(142, 20, 19) },
            {NodeType.Texture, Color.FromRGB(69, 110, 137) },
            {NodeType.Material, Color.FromRGB(180, 105, 20) },
            {NodeType.Color, Color.FromRGB(142, 20, 19) },
            {NodeType.Coordinate, Color.FromRGB(69, 110, 137) },
            {NodeType.Const, Color.FromRGB(96, 132, 91) },
            {NodeType.Math, Color.FromRGB(180, 105, 20) },
            {NodeType.Parameter, Color.FromRGB(142, 20, 19) },
            {NodeType.Function, Color.FromRGB(69, 110, 137) },
            {NodeType.Others, Color.FromRGB(96, 132, 91) },
            {NodeType.VectorOperation, Color.FromRGB(96, 132, 91) },
            {NodeType.Utility, Color.FromRGB(180, 105, 20) },
            {NodeType.Misc, Color.FromRGB(69, 110, 137) },
            {NodeType.AnimNode, Color.FromRGB(142, 20, 19) },
            {NodeType.Comment, Color.FromRGB(131, 111, 255) },
            {NodeType.Particle, Color.FromRGB(188, 10, 136) },
            {NodeType.Sky, Color.FromRGB(50, 10, 200) },
            {NodeType.VirtualTexture,  Color.FromRGB(200, 10, 200) },
            {NodeType.MaterialAttributes, Color.FromRGB(10, 220, 16) },
            {NodeType.Water, Color.FromRGB(180, 105, 20) },
            {NodeType.Volume,  Color.FromRGB(96, 132, 91)},
            {NodeType.Custom, Color.FromRGB(69, 110, 137) },
            {NodeType.Shading,  Color.FromRGB(142, 20, 19)},
            {NodeType.Hair, Color.FromRGB(96, 132, 91) },
            {NodeType.Font, Color.FromRGB(69, 110, 137) },
            {NodeType.Atmosphere,  Color.FromRGB(10, 220, 16)},
            {NodeType.Tree, Color.FromRGB(142, 20, 19) },
            {NodeType.Terrain, Color.FromRGB(96, 132, 91) },
        };

        public static Color GetColor(NodeType Type)
        {
            if (_NodeColorMap.ContainsKey(Type))
            {
                return _NodeColorMap[Type];
            }
            else
            {
                return Color.FromRGB(0, 0, 0);
            }
        }
    }
}
