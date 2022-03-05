using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace CrossEditor
{
    class BlackboardComponent
    {
        BlackboardData _Blackboard;
        Dictionary<string, object> _MemoryBlock;

        public BlackboardComponent()
        {
            _Blackboard = null;
            _MemoryBlock = new Dictionary<string, object>();
        }

        public void UseBlackboard(BlackboardData Blackboard)
        {
            if (Blackboard != null)
            {
                _Blackboard = Blackboard;
            }
            InitMemoryBlock();
        }

        public void InitMemoryBlock()
        {
            if (_Blackboard == null)
            {
                return;
            }
            foreach (BlackboardEntry Entry in _Blackboard.InheritedEntries)
            {
                if (_MemoryBlock.ContainsKey(Entry.EntryName) == false && Entry.bInstanceSynced == false)
                {
                    _MemoryBlock.Add(Entry.EntryName, Entry.KeyType.DefaultValue);
                }
                else if (Entry.bInstanceSynced == true)
                {
                    Entry.ResetSharedValue();
                }
            }
            foreach (BlackboardEntry Entry in _Blackboard.Entries)
            {
                if (_MemoryBlock.ContainsKey(Entry.EntryName) == false && Entry.bInstanceSynced == false)
                {
                    _MemoryBlock.Add(Entry.EntryName, Entry.KeyType.DefaultValue);
                }
                else if (Entry.bInstanceSynced == true)
                {
                    Entry.ResetSharedValue();
                }
            }
        }

        public void SetValue(string EntryName, object Value)
        {
            if (_MemoryBlock.ContainsKey(EntryName))
            {
                _MemoryBlock[EntryName] = Value;
                return;
            }

            BlackboardEntry Entry = _Blackboard.GetEntry(EntryName);
            if (Entry != null && Entry.bInstanceSynced == true)
            {
                Entry.SharedValue = Value;
            }
        }

        public object GetValue(string EntryName)
        {
            if (_MemoryBlock.ContainsKey(EntryName))
            {
                return _MemoryBlock[EntryName];
            }

            BlackboardEntry Entry = _Blackboard.GetEntry(EntryName);
            if (Entry != null && Entry.bInstanceSynced == true)
            {
                return Entry.SharedValue;
            }

            return null;
        }
    }
}
