using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using EditorUI;

namespace CrossEditor
{
    class Inspector_BehaviorTree : Inspector
    {
        private object _InspectedObject;
        private PropertyInfo _PropertyInfo;
        private Edit _EditName;
        private Edit _PropertyName;
        private System.Windows.Forms.Control _PropertyValue;

        public override void Inspect(Control Container, object Parameter1, object Parameter2, object Parameter3)
        {
            _InspectedObject = Parameter1;
            Type Type = Parameter1.GetType();
            string PropertyName = Parameter2 as string;
            _PropertyInfo = Type.GetProperty(PropertyName,BindingFlags.Instance | BindingFlags.NonPublic);

            PropertyInfoAttribute Attribute = PropertyInfoAttribute.GetPropertyInfoAttribute(_PropertyInfo);

            _EditName = new Edit();
            _EditName.SetText(Parameter3 as string);
            Container.Controls.Add(_EditName);

            _PropertyName = new Edit();
            _PropertyName.SetText(Attribute.ToolTips);
            Container.Controls.Add(_PropertyName);

            _PropertyValue = new System.Windows.Forms.ComboBox();
            System.Windows.Forms.ComboBox ComboBox = _PropertyValue as System.Windows.Forms.ComboBox;
            string Value = _PropertyInfo.GetValue(_InspectedObject) as string;

            ComboBox.Items.Add("None");
            ComboBox.SelectedIndex = 0;
            foreach(string Name in BehaviorTreeManager.GetInstance().BehaviorTrees.Keys)
            {
                ComboBox.Items.Add(Name);
                if(Name == Value)
                {
                    ComboBox.SelectedIndex = ComboBox.Items.IndexOf(Name);
                }
            }
            ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ComboBox.SelectedValueChanged += OnSelectedValueChange;
            Container.Controls.Add(ComboBox);
        }

        private void OnSelectedValueChange(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox Self = sender as System.Windows.Forms.ComboBox;
            string ParentName = (string)Self.SelectedItem;
            _PropertyInfo.SetValue(_InspectedObject, ParentName);
            string Value = _PropertyInfo.GetValue(_InspectedObject) as string;
            if (ParentName != Value)
            {
                Self.SelectedIndex = Self.Items.IndexOf(Value);
            }
        }

        public override void UpdateLayout(int Width, ref int Y)
        {
            int SpanX = 5, SpanY = 3;

            _EditName.Location = new System.Drawing.Point(0, Y);
            _EditName.Width = Width;
            _EditName.Height = Edit.GetDefalutFontHeight();

            Y += _EditName.Height + SpanY;

            _PropertyName.Location = new System.Drawing.Point(SpanX, Y);
            _PropertyName.Width = (Width - SpanX) / 2;
            _PropertyName.Height = Edit.GetDefalutFontHeight();

            _PropertyValue.Location = new System.Drawing.Point(SpanX + _PropertyName.Width + SpanX, Y);
            _PropertyValue.Width = (Width - SpanX) / 2 - 2 * SpanX;
            _PropertyValue.Height = _PropertyName.Height;

            Y += _PropertyName.Height + SpanY;
        }
    }
}
