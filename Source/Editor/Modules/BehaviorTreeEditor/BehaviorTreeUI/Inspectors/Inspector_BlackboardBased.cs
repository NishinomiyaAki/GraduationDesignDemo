using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;
using System.Reflection;

namespace Editor
{
    class Inspector_BlackboardBased : Inspector
    {
        private BTDecorator_BlackboardBased _InspectedObject;
        private Edit _OperationName;
        private Edit _ValueName;
        private Edit _SelectorName;
        private System.Windows.Forms.ComboBox _Operation;
        private System.Windows.Forms.TextBox _Value;
        private System.Windows.Forms.ComboBox _Selector;

        private FieldInfo _EntryInfo;
        private FieldInfo _ArithInfo;
        private FieldInfo _BasicInfo;
        private FieldInfo _ValueInfo;

        private bool _bComputable;
        private string _CurrentEntryName;
        private BlackboardEntry _CurrentEntry;
        private BlackboardData _CurrentBlackboard;

        private bool _bInspectable = true;

        public override void Inspect(Control Container, object Parameter1, object Parameter2, object Parameter3)
        {
            _InspectedObject = Parameter1 as BTDecorator_BlackboardBased;
            _EntryInfo = _InspectedObject.GetType().GetField("_SelectedEntryName");
            _ArithInfo = _InspectedObject.GetType().GetField("_ArithOperation");
            _BasicInfo = _InspectedObject.GetType().GetField("_BasicOperation");
            _ValueInfo = _InspectedObject.GetType().GetField("_CompareValue");

            _CurrentBlackboard = BehaviorTreeManager.GetInstance().GetBehaviorTreeByBTAuxiliaryNode(_InspectedObject).Blackboard;
            if(_CurrentBlackboard == null)
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
            if(_CurrentEntry == null)
            {
                _bComputable = false;
            }
            else
            {
                _bComputable = _CurrentEntry.KeyType.IsComputable();
            }

            _OperationName = new Edit();
            Container.AddChild(_OperationName);

            _ValueName = new Edit();
            _ValueName.SetText("Target Value");
            Container.AddChild(_ValueName);

            _SelectorName = new Edit();
            _SelectorName.SetText("Blackboard Key");
            Container.AddChild(_SelectorName);

            _Operation = new System.Windows.Forms.ComboBox();
            _Operation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            Container.Controls.Add(_Operation);

            _Value = new System.Windows.Forms.TextBox();
            object Value = _ValueInfo.GetValue(_InspectedObject);
            _Value.Text = Value.ToString();
            _Value.TextChanged += OnTextChange;
            Container.Controls.Add(_Value);

            _Selector = new System.Windows.Forms.ComboBox();
            
            int Index = 0;
            foreach (BlackboardEntry Entry in _CurrentBlackboard.Entries)
            {
                _Selector.Items.Add(Entry.EntryName);
                if(Entry == _CurrentEntry)
                {
                    _Selector.SelectedIndex = Index;
                }
                Index++;
            }
            foreach (BlackboardEntry Entry in _CurrentBlackboard.InheritedEntries)
            {
                _Selector.Items.Add(Entry.EntryName);
                if(Entry == _CurrentEntry)
                {
                    _Selector.SelectedIndex = Index;
                }
                Index++;
            }
            _Selector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _Selector.SelectedIndexChanged += OnSelectedValueChange;
            Container.Controls.Add(_Selector);

            DoConstruct();
        }

        private void OnTextChange(object sender, EventArgs e)
        {
            string Text = (sender as System.Windows.Forms.TextBox).Text;

            object NewValue = null;
            MethodInfo[] Methods = _CurrentEntry.KeyType.DataType.GetMethods();
            MethodInfo TryParse = null;
            MethodInfo Parse = null;
            foreach (MethodInfo Method in Methods)
            {
                ParameterInfo[] MethodParameters = Method.GetParameters();
                if (Method.Name == "TryParse"
                    && MethodParameters.Length == 2
                    && MethodParameters[0].ParameterType == Text.GetType()
                    && MethodParameters[1].IsOut)
                {
                    TryParse = Method;
                }

                if (Method.Name == "Parse"
                    && MethodParameters.Length == 1
                    && MethodParameters[0].ParameterType == Text.GetType())
                {
                    Parse = Method;
                }
            }
            if (TryParse != null && (bool)TryParse.Invoke(null, new object[] { Text, NewValue }))
            {
                NewValue = Parse.Invoke(null, new object[] { Text });
                _ValueInfo.SetValue(_InspectedObject, NewValue);
            }
        }

        public void DoConstruct()
        {

            _OperationName.SetText(_bComputable ? "Arithmetic Operation" : "Basic Operation");

            _ValueName.Visible = _bComputable;

            FieldInfo CurrentInfo = _bComputable ? _ArithInfo : _BasicInfo;
            _Operation.Items.Clear();
            int Index = 0;
            string Value = CurrentInfo.GetValue(_InspectedObject).ToString();
            foreach (string Name in Enum.GetNames(CurrentInfo.FieldType))
            {
                _Operation.Items.Add(Name);
                if(Name == Value)
                {
                    _Operation.SelectedIndex = Index;
                }
                Index++;
            }
            _Operation.SelectedIndexChanged += OnOperationChanged;

            _Value.Visible = _bComputable;

        }

        private void OnOperationChanged(object sender, EventArgs e)
        {
            FieldInfo CurrentInfo = _bComputable ? _ArithInfo : _BasicInfo;
            string Value = (string)(sender as System.Windows.Forms.ComboBox).SelectedItem;
            CurrentInfo.SetValue(_InspectedObject, Enum.Parse(CurrentInfo.FieldType, Value));
        }

        private void OnSelectedValueChange(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox Self = sender as System.Windows.Forms.ComboBox;

            string EntryName = (string)Self.SelectedItem;

            _CurrentEntry = _CurrentBlackboard.GetEntry(EntryName);
            _bComputable = _CurrentEntry.KeyType.IsComputable();
            _EntryInfo.SetValue(_InspectedObject, _CurrentEntry.EntryName);

            InspectorUI.GetInstance().InspectObject();
        }

        public override void UpdateLayout(int Width, ref int Y)
        {
            if(_bInspectable == false)
            {
                return;
            }
            int SpanX = 5, SpanY = 3;
            _OperationName.Location = new System.Drawing.Point(SpanX, Y);
            _OperationName.Size = new System.Drawing.Size((Width - SpanX) / 2, Edit.GetDefalutFontHeight());

            _Operation.Location = new System.Drawing.Point(SpanX + (Width - SpanX) / 2 + SpanX, Y);
            _Operation.Size = new System.Drawing.Size((Width - SpanX) / 2 - 2 * SpanX, _OperationName.Height);

            Y += _Operation.Height + SpanY;

            if (_bComputable)
            {
                _ValueName.Location = new System.Drawing.Point(SpanX, Y);
                _ValueName.Size = new System.Drawing.Size((Width - SpanX) / 2, Edit.GetDefalutFontHeight());

                _Value.Location = new System.Drawing.Point(SpanX + (Width - SpanX) / 2 + SpanX, Y);
                _Value.Size = new System.Drawing.Size((Width - SpanX) / 2 - 2 * SpanX, _ValueName.Height);

                Y += _Value.Height + SpanY;
            }

            _SelectorName.Location = new System.Drawing.Point(SpanX, Y);
            _SelectorName.Size = new System.Drawing.Size((Width - SpanX) / 2, Edit.GetDefalutFontHeight());

            _Selector.Location = new System.Drawing.Point(SpanX + (Width - SpanX) / 2 + SpanX, Y);
            _Selector.Size = new System.Drawing.Size((Width - SpanX) / 2 - 2 * SpanX, _SelectorName.Height);
            
            Y += _Selector.Height + SpanY;
        }
    }
}
