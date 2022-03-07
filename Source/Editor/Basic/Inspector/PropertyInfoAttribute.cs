using EditorUI;
using System;
using System.Reflection;

namespace Editor
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PropertyInfoAttribute : System.Attribute
    {
        public string PropertyType;
        public string ToolTips;
        public bool bGenerateCode;
        public bool bReadOnly;
        public string FileTypeDescriptor;
        public object DefaultValue;

        public PropertyInfoAttribute()
        {
            PropertyType = "";
            ToolTips = "";
            bGenerateCode = true;
            bReadOnly = false;
            FileTypeDescriptor = "All Files#*";
            DefaultValue = null;
        }

        public static PropertyInfoAttribute GetPropertyInfoAttribute(MemberInfo MemberInfo)
        {
            PropertyInfoAttribute PropertyInfoAttribute = new PropertyInfoAttribute();
            AttributeList AttributeList = AttributeManager.GetInstance().GetAttributeList(MemberInfo);
            if (AttributeList != null)
            {
                AttributeData AttributeData = AttributeList.GetAttributeData("PropertyInfo");
                if (AttributeData != null)
                {
                    string PropertyType1 = (string)AttributeData.GetNamedAttribute("PropertyType");
                    if (PropertyType1 != "" && PropertyType1 != "Auto")
                    {
                        PropertyInfoAttribute.PropertyType = PropertyType1;
                    }
                    object ToolTips1 = (object)AttributeData.GetNamedAttribute("ToolTips");
                    if (ToolTips1 != null)
                    {
                        PropertyInfoAttribute.ToolTips = (string)ToolTips1;
                    }
                    object bGenerateCode1 = AttributeData.GetNamedAttribute("bGenerateCode");
                    if (bGenerateCode1 != null)
                    {
                        PropertyInfoAttribute.bGenerateCode = (bool)bGenerateCode1;
                    }
                    object bReadOnly1 = AttributeData.GetNamedAttribute("bReadOnly");
                    if (bReadOnly1 != null)
                    {
                        PropertyInfoAttribute.bReadOnly = (bool)bReadOnly1;
                    }
                    string FileTypeDescriptor1 = (string)AttributeData.GetNamedAttribute("FileTypeDescriptor");
                    if (FileTypeDescriptor1 != null)
                    {
                        PropertyInfoAttribute.FileTypeDescriptor = FileTypeDescriptor1;
                    }
                    /*object ObjectClassID1 = AttributeData.GetNamedAttribute("ObjectClassID");
                    if (ObjectClassID1 != null)
                    {
                        PropertyInfoAttribute.ObjectClassID = (ObjectClassID)ObjectClassID1;
                    }*/
                    object DefaultValue1 = AttributeData.GetNamedAttribute("DefaultValue");
                    if (DefaultValue1 != null)
                    {
                        PropertyInfoAttribute.DefaultValue = DefaultValue1;
                    }
                }
            }
            return PropertyInfoAttribute;
        }
    }
}