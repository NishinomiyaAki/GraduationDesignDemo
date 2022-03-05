using EditorUI;

namespace CrossEditor
{
    internal class InspectorManager
    {
        public static InspectorManager Instance = new InspectorManager();
        private Panel _Panel;
        private System.Windows.Forms.Control _Container;

        public static InspectorManager GetInstance()
        {
            return Instance;
        }

        public void Initialize(System.Windows.Forms.Control Control)
        {
            _Container = Control;
            _Panel = new Panel();
            _Panel.Initialize();
            _Panel.Location = new System.Drawing.Point(0, 0);
            _Panel.Size = _Container.Size;
            _Panel.BackColor = Graphics2D.GetInstance().ConvertColorToDrawingColor(Color.EDITOR_UI_CONTROL_BACK_COLOR);
            _Container.Controls.Add(_Panel);
        }

        public void ClearInspector()
        {
            _Panel.Controls.Clear();
            _Panel.Refresh();
        }

        public Inspector CreatePropertyInspector(string PropertyTypeString, bool bIsEnum)
        {
            return new Inspector_Property(PropertyTypeString, bIsEnum);
        }

        public Panel GetPanel()
        {
            return _Panel;
        }

        public void DoLayout()
        {
            _Panel.Size = _Container.Size;
            InspectorUI.GetInstance().InspectObject();
        }
    }
}