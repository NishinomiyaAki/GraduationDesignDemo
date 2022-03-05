using System.Windows.Forms;

namespace BehaviorTreeEditorPrototype
{
    public partial class EditWindow : Form
    {
        public EditWindow()
        {
            InitializeComponent();
        }

        public void OpenNodeGraph(string FileName)
        {
            this.DockingManager.AddUI(this.NodeGraphUI);
            this.Show();
            this.BringToFront();
            this.NodeGraphUI.OpenVisualScript(FileName);
        }

        public void OpenBehaviorTree(string FileName)
        {
            this.DockingManager.AddUI(this.BehaviorTreeUI);
            this.Show();
            this.BringToFront();
            this.BehaviorTreeUI.OpenBehaviorTree(FileName);
        }

        public void OpenBlackboard(string FileName)
        {
            this.DockingManager.AddUI(this.BlackboardUI);
            this.Show();
            this.BringToFront();
            this.BlackboardUI.OpenBlackboard(FileName);
        }
    }
}