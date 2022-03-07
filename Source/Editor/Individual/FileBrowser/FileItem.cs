using System.IO;

namespace Editor
{
    internal class FileItem : Edit
    {
        private bool _bIsDirectory;
        private bool _bIsSelected;
        private DirectoryInfo _DirectoryInfo;
        private FileInfo _FileInfo;

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

        public void Initialize(bool bIsDirectory)
        {
            _bIsSelected = false;
            _bIsDirectory = bIsDirectory;
            base.Initialize(EditMode.Simple_SingleLine);
        }

        public void SetInfo(object Info)
        {
            if (_bIsDirectory)
            {
                _DirectoryInfo = Info as DirectoryInfo;
            }
            else
            {
                _FileInfo = Info as FileInfo;
            }
        }
    }
}