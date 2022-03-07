using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace CrossEditor
{
    class BlackboardKeyType_Bool : BlackboardKeyType
    {
        public BlackboardKeyType_Bool()
        {
            DataType = typeof(bool);
            DefaultName = "BoolKey";
            DefaultValue = false;
        }

        public override bool HasSetOrNot(object Value, BasicOperationType Operation)
        {
            bool BoolValue = Convert.ToBoolean(Value);

            switch (Operation)
            {
                case BasicOperationType.HasSet:
                    return BoolValue == true;
                case BasicOperationType.NotSet:
                    return BoolValue == false;
                default:
                    return base.HasSetOrNot(Value, Operation);
            }
        }
    }
}
