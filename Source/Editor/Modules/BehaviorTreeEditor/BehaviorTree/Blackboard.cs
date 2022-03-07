using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using EditorUI;

namespace Editor
{
    class Blackboard
    { 
        public System.Windows.Forms.MouseEventHandler EditMouseDown;
        public System.Windows.Forms.MouseEventHandler EditMouseUp;
        public EventHandler EditMouseEnter;
        public EventHandler EditMouseLeave;
        BlackboardData _BlackboardData;
        Panel _ParentEntriesPanel;
        Panel _CurrentEntriesPanel;

        public BlackboardData BlackboardData
        {
            get
            {
                return _BlackboardData;
            }
        }

        public void SetPanel(Panel ParentEntriesPanel, Panel CurrentEntriesPanel)
        {
            _ParentEntriesPanel = ParentEntriesPanel;
            _CurrentEntriesPanel = CurrentEntriesPanel;
        }

        public int GetParentEntriesCount()
        {
            if(_BlackboardData != null)
            {
                return _BlackboardData.InheritedEntries.Count;
            }
            return 0;
        }

        public int GetEntriesCount()
        {
            if(_BlackboardData != null)
            {
                return _BlackboardData.Entries.Count;
            }
            return 0;
        }

        public BlackboardEntry GetEntry(string EntryName)
        {
            return _BlackboardData.GetEntry(EntryName);
        }

        public void AddEntry(BlackboardEntry Entry)
        {
            _BlackboardData.AddEntry(Entry.EntryName, Entry);
        }

        public void RemoveEntry(string EntryName)
        {
            _BlackboardData.RemoveEntry(EntryName);
        }

        public string GenerateEntryName(BlackboardKeyType KeyType)
        {
            string EntryName = KeyType.DefaultName;
            if (_BlackboardData.HasEntryName(_BlackboardData.Entries, EntryName) == false &&
               _BlackboardData.HasEntryName(_BlackboardData.InheritedEntries, EntryName) == false)
            {
                return EntryName;
            }
            string Base = EntryName;
            for(int Suffix = 0; ; Suffix++)
            {
                EntryName = Base + Suffix.ToString();
                if (_BlackboardData.HasEntryName(_BlackboardData.Entries, EntryName) == false &&
               _BlackboardData.HasEntryName(_BlackboardData.InheritedEntries, EntryName) == false)
                {
                    return EntryName;
                }
            }
        }

        public void SaveToXml(string FileName)
        {
            FileInfo FileInfo = new FileInfo(FileName);

            XmlScript Xml = new XmlScript();
            Record RootRecord = Xml.GetRootRecord();

            Record RecordBlackboard = RootRecord.AddChild();
            RecordBlackboard.SetTypeString("Blackboard");

            RecordBlackboard.SetString("Name", FileInfo.Name);
            RecordBlackboard.SetString("ParentName", _BlackboardData._ParentName);

            foreach(BlackboardEntry Entry in _BlackboardData.Entries)
            {
                Record RecordEntry = RecordBlackboard.AddChild();
                RecordEntry.SetTypeString("BlackboardEntry");
                RecordEntry.SetString("EntryName", Entry.EntryName);
                RecordEntry.SetString("KeyType", Entry.KeyType.GetType().Name);
                RecordEntry.SetBool("InstanceSynced", Entry.bInstanceSynced);
            }

            Xml.Save(FileName);
        }

        public void LoadFromXml(string FileName)
        {
            FileInfo FileInfo = new FileInfo(FileName);
            _BlackboardData = BlackboardManager.GetInstance().GetBlackboardByName(FileInfo.Name);
        }

        public void DoLayout()
        {
            if(_BlackboardData == null)
            {
                return;
            }

            _ParentEntriesPanel.Controls.Clear();
            int Y = 0;
            foreach(BlackboardEntry Entry in _BlackboardData.InheritedEntries)
            {
                Edit Edit = new Edit();
                Edit.SetText(Entry.EntryName);
                Edit.SetPaddingX(20);
                Edit.Location = new System.Drawing.Point(0, Y);
                Edit.Width = _ParentEntriesPanel.Width;
                Edit.Height = Edit.GetDefalutFontHeight();
                Edit.MouseEnter += EditMouseEnter;
                Edit.MouseLeave += EditMouseLeave;
                Edit.MouseDown += EditMouseDown;

                Y += Edit.Height;
                _ParentEntriesPanel.Controls.Add(Edit);
            }

            _CurrentEntriesPanel.Controls.Clear();
            Y = 0;
            foreach(BlackboardEntry Entry in _BlackboardData.Entries)
            {
                Edit Edit = new Edit();
                Edit.SetText(Entry.EntryName);
                Edit.SetPaddingX(20);
                Edit.Location = new System.Drawing.Point(0, Y);
                Edit.Width = _CurrentEntriesPanel.Width;
                Edit.Height = Edit.GetDefalutFontHeight();
                Edit.MouseEnter += EditMouseEnter;
                Edit.MouseLeave += EditMouseLeave;
                Edit.MouseDown += EditMouseDown;
                Edit.MouseUp += EditMouseUp;

                Y += Edit.Height;
                _CurrentEntriesPanel.Controls.Add(Edit);
            }
        }
    }
}
