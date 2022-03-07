using System;
using System.Reflection;

namespace CrossEditor
{
    internal class Inspector_Property : Inspector
    {
        private string _PropertyTypeString;
        private bool _bIsEnum;
        private Edit _PropertyName;
        private System.Windows.Forms.Control _PropertyValue;
        private ObjectProperty _ObjectProperty;
        private PropertyInfoAttribute _PropertyInfoAttribute;
        private Node _Node;

        public Inspector_Property(string PropertyTypeString, bool bIsEnum)
        {
            _PropertyTypeString = PropertyTypeString;
            _bIsEnum = bIsEnum;
        }

        public override void Inspect(Control Container, object Parameter1, object Parameter2, object Parameter3)
        {
            _Node = (Node)Parameter1;
            _ObjectProperty = (ObjectProperty)Parameter2;
            _PropertyInfoAttribute = (PropertyInfoAttribute)Parameter3;

            _PropertyName = new Edit();
            _PropertyName.SetText(_ObjectProperty.Name);
            Container.AddChild(_PropertyName);

            if (_bIsEnum)
            {
                _PropertyValue = new System.Windows.Forms.ComboBox();
                System.Windows.Forms.ComboBox ComboBox = (_PropertyValue as System.Windows.Forms.ComboBox);
                int Index = 0;
                string Value = _ObjectProperty.GetPropertyValueFunction(_Node, _ObjectProperty.Name).ToString();
                foreach (string Name in Enum.GetNames(_ObjectProperty.Type))
                {
                    ComboBox.Items.Add(Name);
                    if (Name == Value)
                    {
                        ComboBox.SelectedIndex = Index;
                    }
                    Index++;
                }
                ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                ComboBox.SelectedValueChanged += OnSelectedValueChange;
            }
            else
            {
                _PropertyValue = new System.Windows.Forms.TextBox();
                System.Windows.Forms.TextBox TextBox = (_PropertyValue as System.Windows.Forms.TextBox);
                object Value = _ObjectProperty.GetPropertyValueFunction(_Node, _ObjectProperty.Name);
                _PropertyValue.Text = Value.ToString();
                _PropertyValue.TextChanged += OnTextChange;
            }
            Container.Controls.Add(_PropertyValue);
        }

        private void OnTextChange(object Sender, System.EventArgs e)
        {
            string Text = (Sender as System.Windows.Forms.TextBox).Text;

            // when property type is string, change value directly
            if (_ObjectProperty.Type.Name == "String")
            {
                _ObjectProperty.SetPropertyValueFunction(_Node, _ObjectProperty.Name, Text);
                return;
            }

            object NewValue = null;
            MethodInfo[] Methods = _ObjectProperty.Type.GetMethods();
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
                _ObjectProperty.SetPropertyValueFunction(_Node, _ObjectProperty.Name, NewValue);
            }
        }

        private void OnSelectedValueChange(object Sender, System.EventArgs e)
        {
            string Value = (string)(Sender as System.Windows.Forms.ComboBox).SelectedItem;
            _ObjectProperty.SetPropertyValueFunction(_Node, _ObjectProperty.Name, Enum.Parse(_ObjectProperty.Type, Value));
        }

        public override void UpdateLayout(int Width, ref int Y)
        {
            int SpanX = 5, SpanY = 3;

            _PropertyName.Location = new System.Drawing.Point(0, Y);
            _PropertyName.Size = new System.Drawing.Size(Width / 2, Edit.GetDefalutFontHeight());

            _PropertyValue.Location = new System.Drawing.Point(_PropertyName.Width + SpanX, Y);
            _PropertyValue.Size = new System.Drawing.Size(Width / 2 - 2 * SpanX, _PropertyName.Height);

            Y += _PropertyName.Height + SpanY;
        }
    }
}