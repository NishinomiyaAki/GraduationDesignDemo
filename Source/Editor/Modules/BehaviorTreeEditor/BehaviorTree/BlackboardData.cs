using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace Editor
{
    class BlackboardData
    {
        public BlackboardData Parent;
        public string _ParentName;
        public string Name;
        public List<BlackboardEntry> Entries;

        [PropertyInfo(ToolTips = "Parent Blackboard")]
        public string ParentName
        {
            get
            {
                if(Parent == null)
                {
                    return "None";
                }
                else
                {
                    return _ParentName;
                }
            }
            set
            {
                if(value == "None")
                {
                    SetParent("");
                }
                else
                {
                    SetParent(value);
                }
                LoadParent();
                BlackboardUI.GetInstance().DoLayout();
            }
        }

        public List<BlackboardEntry> InheritedEntries
        {
            get
            {
                List<BlackboardEntry> InheritedEntries = new List<BlackboardEntry>();
                BlackboardData Parent = this.Parent;
                while (Parent != null)
                {
                    foreach (BlackboardEntry Entry in Parent.Entries)
                    {
                        if (HasEntryName(this.Entries, Entry.EntryName) == false &&
                            HasEntryName(InheritedEntries, Entry.EntryName) == false)
                        {
                            InheritedEntries.Add(Entry);
                        }
                    }
                    Parent = Parent.Parent;
                }
                return InheritedEntries;
            }
        }

        public BlackboardData()
        {
            Parent = null;
            _ParentName = "";
            Name = "";
            Entries = new List<BlackboardEntry>();
        }

        public bool HasSyunchronizedKeys()
        {
            foreach (BlackboardEntry Entry in Entries)
            {
                if (Entry.bInstanceSynced == true)
                {
                    return true;
                }
            }
            foreach (BlackboardEntry Entry in InheritedEntries)
            {
                if (Entry.bInstanceSynced == true)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsKeyInstanceSyned(string EntryName)
        {
            BlackboardEntry Entry = GetEntry(EntryName);
            if (Entry != null)
            {
                return Entry.bInstanceSynced;
            }
            return false;
        }

        public void SetParent(string BlackboardName)
        {
            // check inherit chain
            BlackboardData Blackboard = BlackboardManager.GetInstance().GetBlackboardByName(BlackboardName);
            while (Blackboard != null)
            {
                if (Blackboard.Parent == this)
                {
                    _ParentName = "";
                    return;
                }
                Blackboard = Blackboard.Parent;
            }
            // can not inherit self
            if (BlackboardName == Name)
            {
                _ParentName = "";
            }
            else
            {
                _ParentName = BlackboardName;
            }
        }

        public void LoadParent()
        {
            if (_ParentName != "")
            {
                Parent = BlackboardManager.GetInstance().GetBlackboardByName(_ParentName);
            }
            else
            {
                Parent = null;
            }
        }

        public BlackboardEntry GetEntry(string EntryName)
        {
            foreach (BlackboardEntry Entry in Entries)
            {
                if (Entry.EntryName == EntryName)
                {
                    return Entry;
                }
            }
            foreach (BlackboardEntry Entry in InheritedEntries)
            {
                if (Entry.EntryName == EntryName)
                {
                    return Entry;
                }
            }

            return null;
        }

        public bool HasEntryName(List<BlackboardEntry> Entries, string EntryName)
        {
            foreach (BlackboardEntry Entry in Entries)
            {
                if (Entry.EntryName == EntryName)
                {
                    return true;
                }
            }
            return false;
        }

        public bool AddEntry(string EntryName, BlackboardEntry Entry)
        {
            if (HasEntryName(Entries, EntryName))
            {
                return false;
            }
            else if (HasEntryName(InheritedEntries, EntryName))
            {
                return false;
            }
            else
            {
                Entries.Add(Entry);
                return true;
            }
        }

        public void RemoveEntry(string EntryName)
        {
            BlackboardEntry Target = null;
            foreach (BlackboardEntry Entry in Entries)
            {
                if (Entry.EntryName == EntryName)
                {
                    Target = Entry;
                }
            }
            if (Target != null)
            {
                Entries.Remove(Target);
            }
        }
    }
}
