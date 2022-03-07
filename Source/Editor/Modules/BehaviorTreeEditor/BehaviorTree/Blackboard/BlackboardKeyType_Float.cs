using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace Editor
{
    class BlackboardKeyType_Float : BlackboardKeyType
    {

        public BlackboardKeyType_Float()
        {
            DataType = typeof(float);
            DefaultName = "FloatKey";
            DefaultValue = 0.0F;
        }

        public override bool IsComputable()
        {
            return true;
        }

        public override bool Compare(object Left, object Right, Relation Operation)
        {
            float LeftValue = Convert.ToSingle(Left);
            float RightValue = Convert.ToSingle(Right);

            switch (Operation)
            {
                case Relation.EqualTo:
                    return LeftValue == RightValue;
                case Relation.InequalTo:
                    return LeftValue != RightValue;
                case Relation.GreaterEqualTo:
                    return LeftValue >= RightValue;
                case Relation.GreaterTo:
                    return LeftValue > RightValue;
                case Relation.LowerEqualTo:
                    return LeftValue <= RightValue;
                case Relation.LowerTo:
                    return LeftValue < RightValue;
                default:
                    return base.Compare(LeftValue, RightValue, Operation);
            }
        }
    }
}
