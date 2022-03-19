using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace CrossEditor
{
    public class NodeGraphRegister
    {
        static NodeGraphRegister _Instance = new NodeGraphRegister();

        public static NodeGraphRegister GetInstance()
        {
            return _Instance;
        }

        Dictionary<NodeGraphType, Type> Map = new Dictionary<NodeGraphType, Type>();

        public NodeGraphRegister()
        {
            RegisterNodeGraph();
        }

        public void RegisterNodeGraph()
        {
            IEnumerable<Type> SubClasses =
                from Assembly in AppDomain.CurrentDomain.GetAssemblies()
                from Type in Assembly.GetTypes()
                where Type.IsSubclassOf(typeof(NodeGraphModel))
                select Type;

            foreach(Type Type in SubClasses)
            {
                Map.Add(Enum.Parse<NodeGraphType>(Type.Name), Type);
            }
        }

        public NodeGraphModel CreateNodeGraphModel(NodeGraphType ModelType)
        {
            if(Map.ContainsKey(ModelType))
            {
                return (NodeGraphModel)Activator.CreateInstance(Map[ModelType]);
            }
            else
            {
                return null;
            }
        }
    }
}
