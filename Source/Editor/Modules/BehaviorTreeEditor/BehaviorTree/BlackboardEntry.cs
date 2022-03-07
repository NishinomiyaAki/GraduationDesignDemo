using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace CrossEditor
{
    class BlackboardEntry
    {
        public string EntryName;
        public BlackboardKeyType KeyType;
        public bool bInstanceSynced;
        private object Value;

        public object SharedValue
        {
            get
            {
                return Value;
            }
            set
            {
                Value = value;
            }
        }

        public BlackboardEntry(string EntryName, BlackboardKeyType KeyType, bool bInstanceSynced)
        {
            this.EntryName = EntryName;
            this.KeyType = KeyType;
            this.bInstanceSynced = bInstanceSynced;
            this.Value = KeyType.DefaultValue;
        }

        public void ResetSharedValue()
        {
            SharedValue = KeyType.DefaultValue;
        }
    }
}
