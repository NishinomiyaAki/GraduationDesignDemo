using System.Collections.Generic;

namespace Editor
{
    public class Inspector
    {
        public const string PROPERTY_TYPE_FILE = "file";
        public const string PROPERTY_TYPE_FOLDER = "folder";

        protected List<Inspector> _ChildInspectors;

        public Inspector()
        {
            _ChildInspectors = new List<Inspector>();
        }

        public virtual void Inspect(Control Container, object Parameter1, object Parameter2, object Parameter3)
        {
        }

        public virtual void UpdateLayout(int Width, ref int Y)
        {
            foreach (Inspector Inspector in _ChildInspectors)
            {
                Inspector.UpdateLayout(Width, ref Y);
            }
        }

        public virtual void ReadValue()
        {
            foreach (Inspector Inspector in _ChildInspectors)
            {
                Inspector.ReadValue();
            }
        }

        public virtual void WriteValue()
        {
        }

        public virtual void OnDropPathes(int MouseX, int MouseY, List<string> PathesDragged)
        {
        }

        internal virtual void OnDropEntity(int MouseX, int MouseY, Entity entity)
        {
        }

        public List<Inspector> GetChildInspectors()
        {
            return _ChildInspectors;
        }
    }
}