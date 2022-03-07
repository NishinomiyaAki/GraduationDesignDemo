using System;
using System.Reflection;

namespace CrossEditor
{
    public class Inspector_Node : Inspector
    {
        private Node _Node;

        private Panel _PanelIcon;
        private Edit _EditName;
        private Panel _PanelProperty;

        public Inspector_Node()
        {
        }

        public override void Inspect(Control Container, object Parameter1, object Parameter2, object Parameter3)
        {
            _Node = (Node)Parameter1;

            _PanelIcon = new Panel();
            _PanelIcon.Initialize();
            _PanelIcon.SetImage(UIManager.LoadUIImage("Editor/Game/GameObject.png"));
            Container.AddChild(_PanelIcon);

            _EditName = new Edit();
            _EditName.SetFontSize(16);
            _EditName.Initialize(EditMode.Simple_SingleLine);
            _EditName.LoadSource("");
            _EditName.SetReadOnly(true);
            Container.AddChild(_EditName);
            EditContextUI.GetInstance().RegisterEdit(_EditName);
            _EditName.SetText(_Node.GetType().Name);

            _PanelProperty = new Panel();
            _PanelProperty.Initialize();
            Container.AddChild(_PanelProperty);

            RefreshChildInspectors();
        }

        private void AddPropertyInspector(PropertyInfo PropertyInfo)
        {
            string PropertyTypeString = PropertyInfo.PropertyType.ToString();
            PropertyInfoAttribute PropertyInfoAttribute = PropertyInfoAttribute.GetPropertyInfoAttribute(PropertyInfo);
            if (PropertyInfoAttribute.PropertyType != "")
            {
                PropertyTypeString = PropertyInfoAttribute.PropertyType;
            }
            Type PropertyType = PropertyInfo.PropertyType;
            bool bIsEnum = PropertyType.IsEnum;
            ObjectProperty ObjectProperty = new ObjectProperty();
            ObjectProperty.Name = PropertyInfo.Name;
            ObjectProperty.Type = PropertyType;
            ObjectProperty.GetPropertyValueFunction = GetPropertyValueFunction;
            ObjectProperty.SetPropertyValueFunction = SetPropertyValueFunction;
            Inspector Inspector_Property = InspectorManager.GetInstance().CreatePropertyInspector(PropertyTypeString, bIsEnum);
            Inspector_Property.Inspect(_PanelProperty, _Node, ObjectProperty, PropertyInfoAttribute);
            _ChildInspectors.Add(Inspector_Property);
        }

        protected void RefreshChildInspectors()
        {
            Type Type = _Node.GetType();
            _PanelProperty.ClearChildren();
            _ChildInspectors.Clear();
            PropertyInfo[] Properties = Type.GetProperties();

            foreach (PropertyInfo PropertyInfo in Properties)
            {
                AddPropertyInspector(PropertyInfo);
            }
        }

        public object GetPropertyValueFunction(object Object, string PropertyName)
        {
            Type Type = Object.GetType();
            PropertyInfo PropertyInfo = Type.GetProperty(PropertyName);
            if (PropertyInfo != null)
            {
                return PropertyInfo.GetValue(Object);
            }
            return null;
        }

        public void SetPropertyValueFunction(object Object, string PropertyName, object PropertyValue)
        {
            Type Type = Object.GetType();
            PropertyInfo PropertyInfo = Type.GetProperty(PropertyName);
            if (PropertyInfo != null)
            {
                PropertyInfo.SetValue(Object, PropertyValue);
            }
            _Node.DoLayoutWithConnections();
        }

        public override void UpdateLayout(int Width, ref int Y)
        {
            int SpanX = 5;
            _EditName.Location = new System.Drawing.Point(0, Y);
            _EditName.Size = new System.Drawing.Size(Width, Edit.GetDefalutFontHeight());

            Y += _EditName.Height;

            int Y1 = 0;
            base.UpdateLayout(Width - SpanX, ref Y1);
            _PanelProperty.Size = new System.Drawing.Size(Width - SpanX, Y1);
            _PanelProperty.Location = new System.Drawing.Point(SpanX, Y);
            Y += Y1;
        }
    }
}