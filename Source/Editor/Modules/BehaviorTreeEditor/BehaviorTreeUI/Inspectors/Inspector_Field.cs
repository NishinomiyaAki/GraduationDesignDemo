using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using EditorUI;

namespace Editor
{
    enum InspectType
    {
        Input,
        Select,
        Check,
        Unknown
    }

    class Inspector_Field : Inspector
    {
        private InspectType _Type;
        private BlackboardEntry _BlackboardEntry;
        private FieldInfo _FieldInfo;
        private Edit _FieldName;
        private System.Windows.Forms.Control _FieldValue;
        private bool _bEditable;

        public Inspector_Field(InspectType Type, bool bEditable)
        {
            _Type = Type;
            _bEditable = bEditable;
        }

        public override void Inspect(Control Container, object Parameter1, object Parameter2, object Parameter3)
        {
            _BlackboardEntry = Parameter1 as BlackboardEntry;
            _FieldInfo = Parameter2 as FieldInfo;

            _FieldName = new Edit();
            _FieldName.SetText(_FieldInfo.Name);
            Container.AddChild(_FieldName);

            if (_Type == InspectType.Input)
            {
                _FieldValue = new System.Windows.Forms.TextBox();
                System.Windows.Forms.TextBox TextBox = (_FieldValue as System.Windows.Forms.TextBox);
                object Value = _FieldInfo.GetValue(_BlackboardEntry);
                _FieldValue.Text = Value.ToString();
                _FieldValue.TextChanged += OnTextChange;
            }
            else if (_Type == InspectType.Select)
            {
                _FieldValue = new System.Windows.Forms.ComboBox();
                System.Windows.Forms.ComboBox ComboBox = (_FieldValue as System.Windows.Forms.ComboBox);
                string Value = _FieldInfo.GetValue(_BlackboardEntry).GetType().Name;
                Type BaseType = _FieldInfo.FieldType;
                Type[] Types = Assembly.GetExecutingAssembly().GetTypes();
                foreach(Type Type in Types)
                {
                    if(Type.BaseType == BaseType)
                    {
                        ComboBox.Items.Add(Type.Name);
                        if(Type.Name == Value)
                        {
                            ComboBox.SelectedIndex = ComboBox.Items.IndexOf(Type.Name);
                        }
                    }
                }
                ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                ComboBox.SelectedValueChanged += OnSelectedValueChange;
            }
            else if (_Type == InspectType.Check)
            {
                _FieldValue = new System.Windows.Forms.CheckBox();
                System.Windows.Forms.CheckBox CheckBox = (_FieldValue as System.Windows.Forms.CheckBox);
                object Value = _FieldInfo.GetValue(_BlackboardEntry);
                CheckBox.Checked = (bool)Value;
                CheckBox.CheckedChanged += OnCheckedChanged;
            }
            _FieldValue.Enabled = _bEditable;
            Container.Controls.Add(_FieldValue);
        }

        private void OnCheckedChanged(object sender, EventArgs e)
        {
            bool Value = (sender as System.Windows.Forms.CheckBox).Checked;
            _FieldInfo.SetValue(_BlackboardEntry, Value);
        }

        private void OnSelectedValueChange(object sender, EventArgs e)
        {
            string Value = (string)(sender as System.Windows.Forms.ComboBox).SelectedItem;

            foreach(Type Type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if(Type.Name == Value)
                {
                    object NewValue = Activator.CreateInstance(Type);
                    _FieldInfo.SetValue(_BlackboardEntry, NewValue);
                }
            }
        }

        private void OnTextChange(object sender, EventArgs e)
        {
            string Text = (sender as System.Windows.Forms.TextBox).Text;

            if (Text == "")
            {
                Text = "None";
            }

            if (_FieldInfo.FieldType.Name == "String")
            {
                _FieldInfo.SetValue(_BlackboardEntry, Text);
            }

            BlackboardUI.GetInstance().DoLayout();
        }

        public override void UpdateLayout(int Width, ref int Y)
        {
            int SpanX = 5, SpanY = 3;
            _FieldName.Location = new System.Drawing.Point(0, Y);
            _FieldName.Width = Width / 2;
            _FieldName.Height = Edit.GetDefalutFontHeight();

            if(_FieldValue != null)
            {
                _FieldValue.Location = new System.Drawing.Point(SpanX + _FieldName.Width, Y);
                _FieldValue.Width = Width / 2 - 2 * SpanX;
                _FieldValue.Height = _FieldName.Height;
            }

            Y += _FieldName.Height + SpanY;
        }
    }
}
