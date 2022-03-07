using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace Editor
{
    class BlackboardKeyType_Vector : BlackboardKeyType
    {
        public BlackboardKeyType_Vector()
        {
            DataType = typeof(Vector2f);
            DefaultName = "Vector";
            DefaultValue = null;
        }

        public override bool HasSetOrNot(object Value, BasicOperationType Operation)
        {
            switch (Operation)
            {
                case BasicOperationType.HasSet:
                    return DefaultValue != null;
                case BasicOperationType.NotSet:
                    return DefaultValue == null;
                default:
                    return base.HasSetOrNot(Value, Operation);
            }
        }
    }
}
