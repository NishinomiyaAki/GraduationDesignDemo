using System.Collections.Generic;
using System.Reflection;

namespace EditorUI
{
    public class AttributeData
    {
        public string Name;
        public List<object> Attributes;
        public Dictionary<string, object> NamedAttributes;

        public AttributeData()
        {
            Attributes = new List<object>();
            NamedAttributes = new Dictionary<string, object>();
        }

        public object GetAttribute(int Index)
        {
            if (Index < Attributes.Count)
            {
                return Attributes[Index];
            }
            return null;
        }

        public object GetNamedAttribute(string Name)
        {
            object AttributeValue;
            NamedAttributes.TryGetValue(Name, out AttributeValue);
            return AttributeValue;
        }
    }

    public class AttributeList
    {
        private List<AttributeData> _AttributeList;

        public AttributeList(MemberInfo MemberInfo)
        {
            _AttributeList = new List<AttributeData>();
            IEnumerable<CustomAttributeData> CustomAttributes = MemberInfo.CustomAttributes;
            foreach (CustomAttributeData CustomAttribute in CustomAttributes)
            {
                AttributeData AttributeData = new AttributeData();
                AttributeData.Name = CustomAttribute.AttributeType.ToString();
                foreach (CustomAttributeTypedArgument TypedArgument in CustomAttribute.ConstructorArguments)
                {
                    AttributeData.Attributes.Add(TypedArgument.Value);
                }
                foreach (CustomAttributeNamedArgument NamedArgument in CustomAttribute.NamedArguments)
                {
                    AttributeData.NamedAttributes[NamedArgument.MemberName] = NamedArgument.TypedValue.Value;
                }
                _AttributeList.Add(AttributeData);
            }
        }

        public AttributeData GetAttributeData(string AttributeName)
        {
            string FullAttributeName = AttributeName + "Attribute";
            foreach (AttributeData AttributeData in _AttributeList)
            {
                if (AttributeData.Name.Contains(FullAttributeName))
                {
                    return AttributeData;
                }
            }
            return null;
        }
    }

    public class AttributeManager
    {
        private static AttributeManager _Instance = new AttributeManager();

        public Dictionary<MemberInfo, AttributeList> AttributeLists;

        public static AttributeManager GetInstance()
        {
            return _Instance;
        }

        private AttributeManager()
        {
            AttributeLists = new Dictionary<MemberInfo, AttributeList>();
        }

        public AttributeList GetAttributeList(MemberInfo MemberInfo)
        {
            AttributeList AttributeList;
            if (AttributeLists.TryGetValue(MemberInfo, out AttributeList))
            {
                return AttributeList;
            }
            AttributeList = new AttributeList(MemberInfo);
            AttributeLists[MemberInfo] = AttributeList;
            return AttributeList;
        }
    }
}