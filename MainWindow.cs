using System.IO;
using System.Windows.Forms;

namespace GraduationDesignDemo
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void CreateFolder(string FolderName)
        {
            DirectoryInfo CurrentPath = new DirectoryInfo("./");
            CurrentPath.CreateSubdirectory(FolderName);
        }

        public void OpenNodeGraph(string FileName)
        {
            this.DockingManager.AddUI(this.NodeGraphUI);
            this.NodeGraphUI.OpenVisualScript(FileName);
        }

        public void OpenBehaviorTree(string FileName)
        {
            this.DockingManager.AddUI(this.BehaviorTreeUI);
            this.BehaviorTreeUI.OpenBehaviorTree(FileName);
        }

        public void OpenBlackboard(string FileName)
        {
            this.DockingManager.AddUI(this.BlackboardUI);
            this.BlackboardUI.OpenBlackboard(FileName);
        }
    }
}