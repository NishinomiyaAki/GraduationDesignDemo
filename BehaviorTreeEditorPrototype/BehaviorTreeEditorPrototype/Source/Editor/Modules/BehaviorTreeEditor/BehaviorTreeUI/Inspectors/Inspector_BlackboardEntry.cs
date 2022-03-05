using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using EditorUI;

namespace CrossEditor
{
    class Inspector_BlackboardEntry : Inspector
    {
        private BlackboardEntry _BlackboardEntry;

        private Edit _EditName;
        private Panel _PanelField;
        private bool _bEditable;

        public Inspector_BlackboardEntry(bool bEditable)
        {
            _bEditable = bEditable;
        }

        public override void Inspect(Control Container, object Parameter1, object Parameter2, object Parameter3)
        {
            _BlackboardEntry = Parameter1 as BlackboardEntry;

            _EditName = new Edit();
            _EditName.SetText("Selected Key");
            Container.AddChild(_EditName);

            _PanelField = new Panel();
            _PanelField.Initialize();
            Container.AddChild(_PanelField);

            RefreshChildInspectors();
        }

        private void RefreshChildInspectors()
        {
            _PanelField.ClearChildren();
            _ChildInspectors.Clear();

            Type Type = _BlackboardEntry.GetType();
            FieldInfo[] FieldInfos = Type.GetFields();

            foreach(FieldInfo FieldInfo in FieldInfos)
            {
                AddFieldInspector(FieldInfo);
            }
        }

        private void AddFieldInspector(FieldInfo FieldInfo)
        {
            InspectType InspectType;
            switch (FieldInfo.Name)
            {
                case "EntryName":
                    InspectType = InspectType.Input;
                    break;
                case "KeyType":
                    InspectType = InspectType.Select;
                    break;
                case "bInstanceSynced":
                    InspectType = InspectType.Check;
                    break;
                default:
                    InspectType = InspectType.Unknown;
                    break;
            }
            Inspector_Field Inspector_Field = new Inspector_Field(InspectType, _bEditable);
            Inspector_Field.Inspect(_PanelField, _BlackboardEntry, FieldInfo, null);
            _ChildInspectors.Add(Inspector_Field);
        }

        public override void UpdateLayout(int Width, ref int Y)
        {
            int SpanX = 5, SpanY = 3;

            _EditName.Location = new System.Drawing.Point(0, Y);
            _EditName.Width = Width;
            _EditName.Height = Edit.GetDefalutFontHeight();
            Y += _EditName.Height + SpanY;

            int Y1 = 0;
            base.UpdateLayout(Width - SpanX, ref Y1);
            _PanelField.Location = new System.Drawing.Point(SpanX, Y);
            _PanelField.Width = Width - SpanX;
            _PanelField.Height = Y1;
            Y += Y1 + SpanY;
        }
    }
}
