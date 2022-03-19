using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace CrossEditor
{
    public class NodeRegister
    {
        static NodeRegister _Instance = new NodeRegister();

        public static NodeRegister GetInstance()
        {
            return _Instance;
        }

        Dictionary<string, Type> map = new Dictionary<string, Type>();

        public void RegisterNodes()
        {
            var subclasses =
            from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where type.IsSubclassOf(typeof(Node))
            select type;

            foreach (var item in subclasses)
            {
                map.Add(item.Name, item);
            }
        }

        public object CreateNode(string TypeString)
        {
            if (map.ContainsKey(TypeString))
            {
                return Activator.CreateInstance(map[TypeString],
                     BindingFlags.CreateInstance |
                     BindingFlags.Public |
                     BindingFlags.Instance |
                     BindingFlags.OptionalParamBinding, null, null, null);
            }
            return null;
        }
    }

}
