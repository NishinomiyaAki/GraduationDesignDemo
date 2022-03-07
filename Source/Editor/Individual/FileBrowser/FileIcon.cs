using System;
using System.IO;

namespace CrossEditor
{
    internal class FileIcon : Panel
    {
        private System.Drawing.Icon _Icon;
        private System.Windows.Forms.PictureBox _IconBox;
        private Edit _Edit;
        private System.Windows.Forms.TextBox _Input;

        private bool _bIsDirectory;
        private bool _bIsSelected;
        private DirectoryInfo _DirectoryInfo;
        private FileInfo _FileInfo;
        private string _Extension;
        public const int _Height = 200;
        public const int _Width = 200;
        private const int _IconPadding = 20;

        public bool IsDirectory
        {
            get
            {
                return _bIsDirectory;
            }
        }

        public bool IsSelected
        {
            get
            {
                return _bIsSelected;
            }
            set
            {
                _bIsSelected = value;
            }
        }

        public object Info
        {
            get
            {
                if (IsDirectory)
                {
                    return _DirectoryInfo;
                }
                else
                {
                    return _FileInfo;
                }
            }
        }

        public string FullName
        {
            get
            {
                if (IsDirectory)
                {
                    return _DirectoryInfo.FullName;
                }
                else
                {
                    return _FileInfo.FullName;
                }
            }
        }

        public void Initialize(bool bIsDirectory, object Info)
        {
            _bIsSelected = false;
            _bIsDirectory = bIsDirectory;

            _Edit = new Edit();
            _Edit.Initialize(EditMode.Simple_SingleLine);

            if (_bIsDirectory)
            {
                _DirectoryInfo = Info as DirectoryInfo;
                _Icon = System.Drawing.SystemIcons.Information;
                _Edit.SetText(_DirectoryInfo.Name);
            }
            else
            {
                _FileInfo = Info as FileInfo;
                _Icon = System.Drawing.Icon.ExtractAssociatedIcon(_FileInfo.FullName);
                _Edit.SetText(_FileInfo.Name);
            }

            _IconBox = new System.Windows.Forms.PictureBox();
            _IconBox.Image = System.Drawing.Image.FromHbitmap(_Icon.ToBitmap().GetHbitmap());
            _IconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;

            Controls.Add(_IconBox);
            Controls.Add(_Edit);
            base.Initialize();
        }

        public void Initialize(bool bIsDirectory, string Extension)
        {
            _bIsSelected = false;
            _bIsDirectory = bIsDirectory;
            _Extension = Extension;

            _Input = new System.Windows.Forms.TextBox();
            _Input.Text = "new" + Extension;
            _Input.KeyDown += OnEnterDown;

            _Icon = System.Drawing.SystemIcons.Information;

            _IconBox = new System.Windows.Forms.PictureBox();
            _IconBox.Image = System.Drawing.Image.FromHbitmap(_Icon.ToBitmap().GetHbitmap());
            _IconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;

            Controls.Add(_IconBox);
            Controls.Add(_Input);
            base.Initialize();
        }

        public void DoLayout()
        {
            Size = new System.Drawing.Size(_Width, _Height);

            if (_Edit != null)
            {
                _Edit.Width = Edit._SpanX + GraphicsHelper.GetInstance().DefaultFont.MeasureString_Fast(_Edit.Text) + Edit._SpanX;
                _Edit.Height = Edit._SpanY + GraphicsHelper.GetInstance().DefaultFont.GetCharHeight() + Edit._SpanY;
                _Edit.Location = new System.Drawing.Point((_Width - _Edit.Width) / 2, (_Height - _Edit.Height));
                _Edit.BackColor = System.Drawing.Color.Transparent;

                _IconBox.Size = new System.Drawing.Size(_Width - _IconPadding * 2, (_Height - _Edit.Height) - _IconPadding * 2);
                _IconBox.Location = new System.Drawing.Point(_IconPadding, _IconPadding);
            }

            if (_Input != null)
            {
                _Input.Width = Edit._SpanX + GraphicsHelper.GetInstance().DefaultFont.MeasureString_Fast(_Input.Text) + Edit._SpanX;
                _Input.Height = Edit._SpanY + GraphicsHelper.GetInstance().DefaultFont.GetCharHeight() + Edit._SpanY;
                _Input.Location = new System.Drawing.Point((_Width - _Input.Width) / 2, (_Height - _Input.Height));

                _IconBox.Size = new System.Drawing.Size(_Width - _IconPadding * 2, (_Height - _Input.Height) - _IconPadding * 2);
                _IconBox.Location = new System.Drawing.Point(_IconPadding, _IconPadding);
            }
        }

        private void OnEnterDown(object Sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                DirectoryInfo CurrentPath = FileManager.GetInstance().GetCurrentPath();
                System.Windows.Forms.TextBox Self = Sender as System.Windows.Forms.TextBox;
                if (Self.Text.Length != 0)
                {
                    if (_bIsDirectory)
                    {
                        DirectoryInfo CurrentDirectory = new DirectoryInfo(CurrentPath.FullName + Self.Text);
                        if (!CurrentDirectory.Exists)
                        {
                            CurrentDirectory.Create();
                            _Edit = new Edit();
                            _Edit.Initialize(EditMode.Simple_SingleLine);
                            _Edit.SetText(CurrentDirectory.Name);
                            _DirectoryInfo = CurrentDirectory;

                            Controls.Remove(_Input);
                            Controls.Add(_Edit);
                            FileManager.GetInstance().RefreshCurrentDirectory();
                        }
                    }
                    else
                    {
                        FileInfo CurrentFile = new FileInfo(CurrentPath.FullName + Self.Text);
                        if (!CurrentFile.Exists && string.Compare(CurrentFile.Extension, _Extension, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            FileStream FileStream = CurrentFile.Create();
                            FileStream.Close();
                            _Edit = new Edit();
                            _Edit.Initialize(EditMode.Simple_SingleLine);
                            _Edit.SetText(CurrentFile.Name);
                            _FileInfo = CurrentFile;

                            _Icon = System.Drawing.Icon.ExtractAssociatedIcon(_FileInfo.FullName);

                            _IconBox.Image = System.Drawing.Image.FromHbitmap(_Icon.ToBitmap().GetHbitmap());

                            Controls.Remove(_Input);
                            Controls.Add(_Edit);
                            FileManager.GetInstance().RefreshCurrentDirectory();

                            if(string.Compare(_FileInfo.Extension, ".BB", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                BlackboardManager.GetInstance().RegisterBlackboard(_FileInfo);
                            }
                            else if(string.Compare(_FileInfo.Extension, ".BT", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                BehaviorTreeManager.GetInstance().RegisterBehaviorTree(_FileInfo);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!this.RectangleToScreen(this.ClientRectangle).Contains(Control.MousePosition))
            {
                base.OnMouseLeave(e);
            }
        }

        private void SubControlLeave(object Sender, EventArgs e)
        {
            if (!this.RectangleToScreen(this.ClientRectangle).Contains(Control.MousePosition))
            {
                base.OnMouseLeave(e);
            }
        }

        private void SubMouseDown(object Sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (this.RectangleToScreen(this.ClientRectangle).Contains(Control.MousePosition))
            {
                base.OnMouseDown(e);
            }
        }

        private void SubMouseDoubleClick(object Sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (this.RectangleToScreen(this.ClientRectangle).Contains(Control.MousePosition))
            {
                base.OnMouseDoubleClick(e);
            }
        }

        protected override void OnControlAdded(System.Windows.Forms.ControlEventArgs e)
        {
            e.Control.MouseLeave += SubControlLeave;
            e.Control.MouseDown += SubMouseDown;
            e.Control.MouseDoubleClick += SubMouseDoubleClick;
            base.OnControlAdded(e);
        }
    }
}