using EditorUI;
using Editor;
using System;

namespace GraduationDesignDemo
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if(ClientSize.Width == 0 || ClientSize.Height == 0)
            {
                base.OnSizeChanged(e);
                return;
            }

            this.TopLeftPanel.Location = new System.Drawing.Point(0, 0);
            this.TopLeftPanel.Size = new System.Drawing.Size(ClientSize.Width * 3 / 4, ClientSize.Height * 3 / 4);

            this.TopRightPanel.Location = new System.Drawing.Point(ClientSize.Width * 3 / 4, 0);
            this.TopRightPanel.Size = new System.Drawing.Size(ClientSize.Width / 4, ClientSize.Height * 3 / 4);

            this.BottomPanel.Location = new System.Drawing.Point(0, ClientSize.Height * 3 / 4);
            this.BottomPanel.Size = new System.Drawing.Size(ClientSize.Width, ClientSize.Height / 4);

            this.FileManager?.DoLayout();
            this.DockingManager?.DoLayout();
            this.InspectorManager?.DoLayout();
            this.ConsoleManager?.DoLayout();

            base.OnSizeChanged(e);
        }

        #region Windows Form Designer generated code

        private System.Windows.Forms.Panel TopLeftPanel;
        private System.Windows.Forms.Panel TopRightPanel;
        private System.Windows.Forms.Panel BottomPanel;
        private FileManager FileManager;
        private NodeGraphUI NodeGraphUI;
        private BehaviorTreeUI BehaviorTreeUI;
        private BlackboardUI BlackboardUI;
        private DockingManager DockingManager;
        private InspectorManager InspectorManager;
        private ConsoleManager ConsoleManager;
        private BlackboardManager BlackboardManager;
        private BehaviorTreeManager BehaviorTreeManager;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.NodeGraphUI = NodeGraphUI.GetInstance();
            this.NodeGraphUI.Initialize();

            this.BehaviorTreeUI = BehaviorTreeUI.GetInstance();
            this.BehaviorTreeUI.Initialize();

            this.BlackboardUI = BlackboardUI.GetInstance();
            this.BlackboardUI.Initialize();

            this.TopLeftPanel = new System.Windows.Forms.Panel();
            this.TopRightPanel = new System.Windows.Forms.Panel();
            this.BottomPanel = new System.Windows.Forms.Panel();

            this.TopLeftPanel.SuspendLayout();
            this.TopRightPanel.SuspendLayout();
            this.BottomPanel.SuspendLayout();
            this.SuspendLayout();

            int Width = 1200, Height = 800;

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(Width, Height);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Text = "MainWindow";

            this.Controls.Add(this.TopLeftPanel);
            this.Controls.Add(this.TopRightPanel);
            this.Controls.Add(this.BottomPanel);

            CreateFolder("TestFolder");

            this.FileManager = FileManager.GetInstance();
            FileManager.Initialize(BottomPanel, "./TestFolder/");

            this.DockingManager = DockingManager.GetInstance();
            this.DockingManager.Initialize(this.TopLeftPanel);

            this.InspectorManager = InspectorManager.GetInstance();
            this.InspectorManager.Initialize(this.TopRightPanel);

            this.ConsoleManager = ConsoleManager.GetInstance();
            this.ConsoleManager.Initialize(this.BottomPanel);

            this.BlackboardManager = BlackboardManager.GetInstance();
            this.BlackboardManager.Initialize("./TestFolder/");

            this.BehaviorTreeManager = BehaviorTreeManager.GetInstance();
            this.BehaviorTreeManager.Initialize("./TestFolder/");

            this.TopLeftPanel.ResumeLayout();
            this.TopRightPanel.ResumeLayout();
            this.BottomPanel.ResumeLayout();
            this.ResumeLayout();

            MainUI MainUI = MainUI.GetInstance();
            MainUI.Initialize(this);
        }

        #endregion
    }
}