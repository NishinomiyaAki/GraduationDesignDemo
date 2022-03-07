using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace CrossEditor
{
    class BlackboardKeyType
    {
        public Type DataType;
        public string DefaultName;
        public object DefaultValue;

        public virtual bool IsComputable()
        {
            return false;
        }

        public virtual bool Compare(object Left, object Right, Relation Operation)
        {
            return false;
        }

        public virtual bool HasSetOrNot(object Value, BasicOperationType Operation)
        {
            return false;
        }
    }
}
