using EditorUI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CrossEditor
{
    class Inspector_Node : Inspector
    {
        Node _Node;

        Panel _PanelIcon;
        Edit _EditName;
        Panel _PanelProperty;

        public Inspector_Node()
        {
        }

        public override void InspectObject(Control Container, object Object, object Tag)
        {
            _Node = (Node)Object;

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
            _EditName.SetSize(100, 16);
            _EditName.SetText(_Node.GetType().Name.ToString());

            _PanelProperty = new Panel();
            _PanelProperty.Initialize();
            Container.AddChild(_PanelProperty);

            RefreshChildInspectors();
        }

        public override void WriteValue()
        {
            foreach (Inspector Inspector in _ChildInspectors)
            {
                Inspector.WriteValue();
            }
        }

        void AddPropertyInspector(PropertyInfo PropertyInfo)
        {
            string PropertyTypeString = PropertyInfo.PropertyType.ToString();
            PropertyInfoAttribute PropertyInfoAttribute = PropertyInfoAttribute.GetPropertyInfoAttribute(PropertyInfo);
            if (PropertyInfoAttribute.bHide)
            {
                return;
            }
            if (PropertyInfoAttribute.PropertyType != "")
            {
                PropertyTypeString = PropertyInfoAttribute.PropertyType;
            }
            Type PropertyType = PropertyInfo.PropertyType;
            bool bIsEnum = PropertyType.IsEnum;
            ObjectProperty ObjectProperty = new ObjectProperty();
            ObjectProperty.Object = _Node;
            ObjectProperty.Name = PropertyInfo.Name;
            ObjectProperty.Type = PropertyType;
            ObjectProperty.EditValid = _Node.bEditable;
            ObjectProperty.PropertyInfoAttribute = PropertyInfoAttribute;
            ObjectProperty.GetPropertyValueFunction = GetPropertyValueFunction;
            ObjectProperty.SetPropertyValueFunction = SetPropertyValueFunction;
            Inspector Inspector_Property = InspectorManager.GetInstance().CreatePropertyInspector(PropertyTypeString, bIsEnum);
            Inspector_Property.InspectProperty(_PanelProperty, _PanelProperty, ObjectProperty);
            AddChildInspector(Inspector_Property);
        }

        protected virtual void RefreshChildInspectors()
        {
            Type Type = _Node.GetType();
            _PanelProperty.ClearChildren();
            _ChildInspectors.Clear();

            PropertyInfo[] Properties;
            Properties = Type.GetProperties();

            foreach (PropertyInfo PropertyInfo in Properties)
            {
                AddPropertyInspector(PropertyInfo);
            }
        }

        public virtual object GetPropertyValueFunction(object Object, string PropertyName, ValueExtraProperty ValueExtraProperty)
        {
            Type Type = Object.GetType();
            PropertyInfo PropertyInfo = Type.GetProperty(PropertyName);
            if (PropertyInfo != null)
            {
                return PropertyInfo.GetValue(Object);
            }
            return null;
        }

        public virtual void SetPropertyValueFunction(object Object, string PropertyName, object PropertyValue, SubProperty SubProperty)
        {
            Type Type = Object.GetType();
            PropertyInfo PropertyInfo = Type.GetProperty(PropertyName);
            if (PropertyInfo != null)
            {
                PropertyInfo.SetValue(Object, PropertyValue);
                _Node.GetOwner().GetView().SetModified();
            }
            _Node.DoLayoutWithConnections();
        }

        public override void UpdateLayout(int Width, ref int Y)
        {
            Width = Math.Max(Width, 260);
            int SpanX = 5;
            _PanelIcon.SetPosition(SpanX, Y + 3, 20, 20);
            int EditNameX = SpanX;
            int EditNameWidth = Math.Max(Width - SpanX - EditNameX, 100);
            _EditName.SetPosition(EditNameX, Y + 7, EditNameWidth, 16);

            Y += 28;

            int Y1 = 0;
            base.UpdateLayout(Width, ref Y1);
            _PanelProperty.SetPosition(0, Y, Width, Y1);
            Y += Y1;
        }
    }
}
