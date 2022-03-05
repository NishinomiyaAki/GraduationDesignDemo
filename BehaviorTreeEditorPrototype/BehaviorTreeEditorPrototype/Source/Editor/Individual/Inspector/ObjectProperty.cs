namespace CrossEditor
{
    internal class ObjectProperty
    {
        public string Name;
        public System.Type Type;
        public System.Func<object, string, object> GetPropertyValueFunction;
        public System.Action<object, string, object> SetPropertyValueFunction;
    }
}