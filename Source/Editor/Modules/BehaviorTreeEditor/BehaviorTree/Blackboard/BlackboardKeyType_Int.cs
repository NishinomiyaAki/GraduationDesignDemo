using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace Editor
{
    class BlackboardKeyType_Int : BlackboardKeyType
    {
        

        public BlackboardKeyType_Int()
        {
            DataType = typeof(int);
            DefaultName = "IntKey";
            DefaultValue = 0;
        }

        public override bool IsComputable()
        {
            return true;
        }


        public override bool Compare(object Left, object Right, Relation Operation)
        {
            int LeftValue = Convert.ToInt32(Left);
            int RightValue = Convert.ToInt32(Right);

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
