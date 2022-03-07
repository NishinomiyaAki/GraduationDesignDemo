using System.IO;
using System.Windows.Forms;

namespace BehaviorTreeEditorPrototype
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
    }
}