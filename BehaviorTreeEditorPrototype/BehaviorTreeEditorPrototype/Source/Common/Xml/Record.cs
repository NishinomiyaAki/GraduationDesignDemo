using System.Collections.Generic;
using System.Text;

namespace EditorUI
{
    public class Record
    {
        private string _TypeString;
        private List<Attribute> _AttributeList;
        private List<Record> _ChildList;

        public Record()
        {
            _TypeString = "";
            _AttributeList = new List<Attribute>();
            _ChildList = new List<Record>();
        }

        public string GetTypeString()
        {
            return _TypeString;
        }

        public void SetTypeString(string InTypeString)
        {
            _TypeString = InTypeString;
        }

        public int GetCount()
        {
            return _AttributeList.Count;
        }

        public string GetAttribute(int i)
        {
            return _AttributeList[i].Name;
        }

        public bool HasAttribute(string Name)
        {
            foreach (Attribute Attribute in _AttributeList)
            {
                if (Attribute.Name == Name)
                {
                    return true;
                }
            }
            return false;
        }

        public string GetString(string Name)
        {
            foreach (Attribute Attribute in _AttributeList)
            {
                if (Attribute.Name == Name)
                {
                    return Attribute.Value;
                }
            }
            return "";
        }

        public float GetFloat(string Name)
        {
            return MathHelper.ParseFloat(GetString(Name));
        }

        public int GetInt(string Name)
        {
            return MathHelper.ParseInt(GetString(Name));
        }

        public uint GetUnsignedInt(string Name)
        {
            return MathHelper.ParseUInt(GetString(Name));
        }

        public long GetLongLong(string Name)
        {
            return MathHelper.ParseLong(GetString(Name));
        }

        public bool GetBool(string Name)
        {
            if (GetString(Name) == "true")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetString(string Name, string Value)
        {
            foreach (Attribute Attribute in _AttributeList)
            {
                if (Attribute.Name == Name)
                {
                    Attribute.Value = Value;
                    return;
                }
            }
            Attribute pAttribute = new Attribute();
            pAttribute.Name = Name;
            pAttribute.Value = Value;
            _AttributeList.Add(pAttribute);
        }

        public void SetFloat(string Name, float Value)
        {
            SetString(Name, Value.ToString());
        }

        public void SetInt(string Name, int Value)
        {
            SetString(Name, Value.ToString());
        }

        public void SetUnsignedInt(string Name, uint Value)
        {
            SetString(Name, Value.ToString());
        }

        public void SetLongLong(string Name, long Value)
        {
            SetString(Name, Value.ToString());
        }

        public void SetBool(string Name, bool bValue)
        {
            SetString(Name, bValue ? "true" : "false");
        }

        public Record AddChild()
        {
            Record ChildElement = new Record();
            _ChildList.Add(ChildElement);
            return ChildElement;
        }

        public void AddChild(Record Record)
        {
            if (Record != null)
            {
                _ChildList.Add(Record);
            }
        }

        public int GetChildCount()
        {
            return _ChildList.Count;
        }

        public Record GetChild(int i)
        {
            if (i < 0 || i > _ChildList.Count - 1)
            {
                return null;
            }
            return _ChildList[i];
        }

        public void SetName(string Name)
        {
            SetString("Name", Name);
        }

        public string GetName()
        {
            return GetString("Name");
        }

        public void SetTag(string Tag)
        {
            SetString("Tag", Tag);
        }

        public string GetTag()
        {
            return GetString("Tag");
        }

        public Record FindByName(string Name)
        {
            for (int i = 0; i < GetChildCount(); ++i)
            {
                Record ChildElement = GetChild(i);
                if (ChildElement.GetString("Name") == Name)
                {
                    return ChildElement;
                }
            }
            return null;
        }

        public Record SearchByName(string Name)
        {
            Record Element = FindByName(Name);
            if (Element == null)
            {
                for (int i = 0; i < GetChildCount(); ++i)
                {
                    Record ChildElement = GetChild(i);
                    Element = ChildElement.FindByName(Name);
                    if (Element != null)
                    {
                        break;
                    }
                }
            }
            return Element;
        }

        public Record FindByTag(string Tag)
        {
            for (int i = 0; i < GetChildCount(); ++i)
            {
                Record ChildElement = GetChild(i);
                if (ChildElement.GetString("Tag") == Tag)
                {
                    return ChildElement;
                }
            }
            return null;
        }

        public Record SearchByTag(string Tag)
        {
            Record Element = FindByTag(Tag);
            if (Element == null)
            {
                for (int i = 0; i < GetChildCount(); ++i)
                {
                    Record ChildElement = GetChild(i);
                    Element = ChildElement.FindByTag(Tag);
                    if (Element != null)
                    {
                        break;
                    }
                }
            }
            return Element;
        }

        public Record FindByTypeString(string TypeString)
        {
            for (int i = 0; i < GetChildCount(); ++i)
            {
                Record ChildElement = GetChild(i);
                if (ChildElement.GetTypeString() == TypeString)
                {
                    return ChildElement;
                }
            }
            return null;
        }

        private void BuildString(StringBuilder StringBuilder, int Indent)
        {
            bool bRoot = (_TypeString == "Root");
            int ChildCount = _ChildList.Count;
            DebugHelper.Assert(_TypeString != "");
            if (bRoot == false)
            {
                StringBuilder.Append(' ', Indent);
                StringBuilder.Append('<');
                StringBuilder.Append(_TypeString);
                bool bAttributeNewLine = (_AttributeList.Count >= 4);
                foreach (Attribute Attribute in _AttributeList)
                {
                    if (bAttributeNewLine)
                    {
                        StringBuilder.Append("\r\n");
                        StringBuilder.Append(' ', Indent + 2);
                    }
                    else
                    {
                        StringBuilder.Append(' ');
                    }
                    StringBuilder.Append(Attribute.Name);
                    string Value = Attribute.Value;
                    if (Value.Contains('"'))
                    {
                        StringBuilder.Append(" = ##");
                        StringBuilder.Append(Value);
                        StringBuilder.Append("##");
                    }
                    else
                    {
                        StringBuilder.Append(" = \"");
                        StringBuilder.Append(Value);
                        StringBuilder.Append('\"');
                    }
                }
                if (ChildCount > 0)
                {
                    StringBuilder.Append(">\r\n");
                }
            }
            foreach (Record Child in _ChildList)
            {
                Child.BuildString(StringBuilder, bRoot ? 0 : Indent + 2);
                StringBuilder.Append("\r\n");
            }
            if (bRoot == false)
            {
                if (ChildCount == 0)
                {
                    StringBuilder.Append("/>");
                }
                else
                {
                    StringBuilder.Append(' ', Indent);
                    StringBuilder.Append("</");
                    StringBuilder.Append(_TypeString);
                    StringBuilder.Append('>');
                }
            }
        }

        public string FormatString()
        {
            StringBuilder StringBuilder = new StringBuilder(1000000);
            BuildString(StringBuilder, 0);
            return StringBuilder.ToString();
        }
    }
}