namespace Editor
{
    internal class EditOperation_ModifyProperty : EditOperation
    {
        public object _Object;
        public object _OldValue;
        public object _NewValue;
        public ObjectProperty _ObjectProperty;

        public EditOperation_ModifyProperty(object Object, ObjectProperty ObjectProperty, object OldValue, object NewValue)
        {
            _Object = Object;
            _ObjectProperty = ObjectProperty;
            _OldValue = OldValue;
            _NewValue = NewValue;
        }

        public override void Undo()
        {
            _ObjectProperty.SetPropertyValueFunction(_Object, _ObjectProperty.Name, _OldValue);
        }

        public override void Redo()
        {
            _ObjectProperty.SetPropertyValueFunction(_Object, _ObjectProperty.Name, _NewValue);
        }
    }
}