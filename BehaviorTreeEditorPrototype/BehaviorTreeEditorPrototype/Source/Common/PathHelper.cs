using System.IO;

namespace EditorUI
{
    public static class PathHelper
    {
        public static string ToStandardForm(string FilePath)
        {
            return FilePath.Replace("\\", "/");
        }

        public static string ToWindowsForm(string FilePath)
        {
            return FilePath.Replace("/", "\\");
        }

        public static string GetExtension(string FilePath)
        {
            return Path.GetExtension(FilePath);
        }

        public static string GetExtensionOfPath(string FilePath)
        {
            int Index = StringHelper.FindLastChar(FilePath, '.');
            if (Index == -1)
            {
                return "";
            }
            else
            {
                return FilePath.Substring(Index + 1);
            }
        }

        public static string GetCompleteExtensionOfPath(string FilePath)
        {
            string Filename = GetFileName(FilePath);
            int Index = StringHelper.FindFirstChar(Filename, '.');
            if (Index == -1)
            {
                return "";
            }
            else
            {
                return Filename.Substring(Index + 1);
            }
        }

        public static string GetFileName(string FilePath)
        {
            return Path.GetFileName(FilePath);
        }

        public static string GetNameOfPath(string FilePath)
        {
            return Path.GetFileNameWithoutExtension(FilePath);
        }

        public static string GetDirectoryName(string FilePath)
        {
            string DirectoryName = Path.GetDirectoryName(FilePath);
            if (DirectoryName == null)
            {
                DirectoryName = "";
            }
            return ToStandardForm(DirectoryName);
        }

        public static bool IsSubDirectory(string Directory1, string Directory2)
        {
            Directory1 = ToStandardForm(Directory1);
            Directory2 = ToStandardForm(Directory2);
            if (Directory2.StartsWith(Directory1))
            {
                char NextChar = StringHelper.GetChar(Directory2, Directory1.Length);
                if (NextChar == '/' || NextChar == '\\')
                {
                    return true;
                }
            }
            return false;
        }

        public static string ToAbsolutePath(string RelativePath)
        {
            return Path.GetFullPath(RelativePath);
        }

        public static bool IsAbsolutePath(string Path)
        {
            char Char0 = StringHelper.GetChar(Path, 0);
            if (Char0 == '/')
            {
                return true;
            }
            char Char1 = StringHelper.GetChar(Path, 1);
            if (Char1 == ':')
            {
                return true;
            }
            return false;
        }

        public static string CombinePath(string Path1, string Path2)
        {
            string CombinedPath = Path.Combine(Path1, Path2);
            return ToStandardForm(CombinedPath);
        }
    }
}