using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;
using System.Reflection;

namespace CrossEditor
{
    class Inspector_EntrySelector : Inspector
    {
        private Node _InspectedObject;
        private Edit _SelectorName;
        private System.Windows.Forms.ComboBox _Selector;

        private FieldInfo _EntryInfo;
        private Type _EntryType;

        private string _CurrentEntryName;
        private BlackboardEntry _CurrentEntry;
        private BlackboardData _CurrentBlackboard;

        private bool _bInspectable = true;

        public override void Inspect(Control Container, object Parameter1, object Parameter2, object Parameter3)
        {
            _InspectedObject = Parameter1 as Node;
            _EntryType = Parameter2 as Type;

            _EntryInfo = _InspectedObject.GetType().GetField("_SelectedEntryName");

            if (_InspectedObject is BTNode)
            {
                _CurrentBlackboard = BehaviorTreeManager.GetInstance().GetBehaviorTreeByBTNode(_InspectedObject as BTNode).Blackboard;
            }
            else
            {
                _CurrentBlackboard = BehaviorTreeManager.GetInstance().GetBehaviorTreeByBTAuxiliaryNode(_InspectedObject as BTAuxiliaryNode).Blackboard;
            }

            
            if (_CurrentBlackboard == null)
            {
                _bInspectable = false;
                return;
            }
            else
            {
                _bInspectable = true;
            }

            _CurrentEntryName = (string)_EntryInfo.GetValue(_InspectedObject);
            _CurrentEntry = _CurrentBlackboard.GetEntry(_CurrentEntryName);

            _SelectorName = new Edit();
            _SelectorName.SetText("Blackboard Key");
            Container.AddChild(_SelectorName);

            _Selector = new System.Windows.Forms.ComboBox();
            int Index = 0;
            foreach (BlackboardEntry Entry in _CurrentBlackboard.Entries)
            {
                if (Entry.KeyType.DataType == _EntryType)
                {
                    _Selector.Items.Add(Entry.EntryName);
                    if (Entry == _CurrentEntry)
                    {
                        _Selector.SelectedIndex = Index;
                    }
                    Index++;
                }
            }
            foreach (BlackboardEntry Entry in _CurrentBlackboard.InheritedEntries)
            {
                if (Entry.KeyType.DataType == _EntryType)
                {
                    _Selector.Items.Add(Entry.EntryName);
                    if (Entry == _CurrentEntry)
                    {
                        _Selector.SelectedIndex = Index;
                    }
                    Index++;
                }
            }
            _Selector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _Selector.SelectedIndexChanged += OnSelectedValueChange;
            Container.Controls.Add(_Selector);
        }

        private void OnSelectedValueChange(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox Self = sender as System.Windows.Forms.ComboBox;

            string EntryName = (string)Self.SelectedItem;

            _CurrentEntry = _CurrentBlackboard.GetEntry(EntryName);
            _EntryInfo.SetValue(_InspectedObject, _CurrentEntry.EntryName);

            _InspectedObject.DoLayoutWithConnections();
        }

        public override void UpdateLayout(int Width, ref int Y)
        {
            if (_bInspectable == false)
            {
                return;
            }

            int SpanX = 5, SpanY = 3;

            _SelectorName.Location = new System.Drawing.Point(SpanX, Y);
            _SelectorName.Size = new System.Drawing.Size((Width - SpanX) / 2, Edit.GetDefalutFontHeight());

            _Selector.Location = new System.Drawing.Point(SpanX + (Width - SpanX) / 2 + SpanX, Y);
            _Selector.Size = new System.Drawing.Size((Width - SpanX) / 2 - 2 * SpanX, _SelectorName.Height);

            Y += _Selector.Height + SpanY;
        }
    }
}