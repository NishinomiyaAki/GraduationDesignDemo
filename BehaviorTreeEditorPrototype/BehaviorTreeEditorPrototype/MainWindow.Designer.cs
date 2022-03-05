using EditorUI;
using CrossEditor;
using System;

namespace BehaviorTreeEditorPrototype
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

            this.TopPanel.Location = new System.Drawing.Point(0, 0);
            this.TopPanel.Size = new System.Drawing.Size(ClientSize.Width, ClientSize.Height * 3 / 4);

            this.BottomPanel.Location = new System.Drawing.Point(0, ClientSize.Height * 3 / 4);
            this.BottomPanel.Size = new System.Drawing.Size(ClientSize.Width, ClientSize.Height / 4);

            this.DisplayPanel.Location = new System.Drawing.Point(0, 0);
            this.DisplayPanel.Size = TopPanel.Size;

            this.FileManager?.DoLayout();

            base.OnSizeChanged(e);
        }

        #region Windows Form Designer generated code

        private MainUI MainUI;
        private EditWindow EditWindow;
        private System.Windows.Forms.Panel TopPanel;
        private System.Windows.Forms.Panel BottomPanel;
        private DisplayPanel DisplayPanel;
        private FileManager FileManager;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.DisplayPanel = new DisplayPanel();

            this.TopPanel = new System.Windows.Forms.Panel();
            this.BottomPanel = new System.Windows.Forms.Panel();

            this.TopPanel.SuspendLayout();
            this.BottomPanel.SuspendLayout();
            this.SuspendLayout();

            int Width = 2400, Height = 1600;

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(Width, Height);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Text = "MainWindow";

            this.Controls.Add(this.TopPanel);
            this.Controls.Add(this.BottomPanel);

            this.TopPanel.Controls.Add(DisplayPanel);

            CreateFolder("TestFolder");

            this.FileManager = FileManager.GetInstance();
            FileManager.Initialize(BottomPanel, "./TestFolder/");

            this.TopPanel.ResumeLayout();
            this.BottomPanel.ResumeLayout();
            this.ResumeLayout();

            this.EditWindow = new EditWindow();

            this.MainUI = MainUI.GetInstance();
            MainUI.Initialize(this, this.EditWindow);
        }

        #endregion
    }
}