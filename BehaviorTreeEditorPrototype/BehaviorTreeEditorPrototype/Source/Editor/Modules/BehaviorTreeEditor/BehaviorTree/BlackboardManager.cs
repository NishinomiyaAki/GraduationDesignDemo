using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using EditorUI;

namespace CrossEditor
{
    class BlackboardManager
    {
        static BlackboardManager Instance = new BlackboardManager();
        public Dictionary<string, BlackboardData> Blackboards;
        List<FileInfo> BlackboardFileInfos;

        public static BlackboardManager GetInstance()
        {
            return Instance;
        }

        public BlackboardManager()
        {
            Blackboards = new Dictionary<string, BlackboardData>();
            BlackboardFileInfos = new List<FileInfo>();
        }

        public void Initialize(string FolderName)
        {
            DirectoryInfo DirectoryInfo = new DirectoryInfo(FolderName);
            foreach (FileInfo FileInfo in DirectoryInfo.GetFiles())
            {
                if(string.Compare(FileInfo.Extension, ".BB", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    LoadBlackboard(FileInfo);
                }
            }
            foreach (BlackboardData BlackboardData in Blackboards.Values)
            {
                BlackboardData.LoadParent();
            }
        }

        public BlackboardData GetBlackboardByName(string Name)
        {
            BlackboardData BlackboardData;
            Blackboards.TryGetValue(Name, out BlackboardData);
            if(BlackboardData != null)
            {
                return BlackboardData;
            }
            return null;
        }

        public BlackboardData GetBlackboardByEntry(BlackboardEntry Entry)
        {
            foreach(BlackboardData Blackboard in Blackboards.Values)
            {
                if (Blackboard.Entries.Contains(Entry))
                {
                    return Blackboard;
                }
            }
            return null;
        }

        public void RegisterBlackboard(FileInfo FileInfo)
        {
            if(string.Compare(FileInfo.Extension, ".BB", StringComparison.OrdinalIgnoreCase) == 0)
            {
                BlackboardFileInfos.Add(FileInfo);
                string Name = FileInfo.Name;
                BlackboardData BlackboardData = new BlackboardData();
                BlackboardData.Name = Name;
                Blackboards.Add(Name, BlackboardData);
            }
        }

        public void UnregisterBlackboard(FileInfo FileInfo)
        {
            FileInfo Target = null;
            foreach(FileInfo BlackboardFileInfo in BlackboardFileInfos)
            {
                if(BlackboardFileInfo.FullName == FileInfo.FullName)
                {
                    Target = BlackboardFileInfo;
                }
            }
            BlackboardFileInfos.Remove(Target);
            Blackboards.Remove(FileInfo.Name);
        }

        public BlackboardKeyType ConvertKeyType(string KeyType)
        {
            switch (KeyType)
            {
                case "BlackboardKeyType_Bool":
                    return new BlackboardKeyType_Bool();
                case "BlackboardKeyType_Int":
                    return new BlackboardKeyType_Int();
                case "BlackboardKeyType_Float":
                    return new BlackboardKeyType_Float();
                case "BlackboardKeyType_Vector":
                    return new BlackboardKeyType_Vector();
                default:
                    return null;
            }
        }

        public void LoadBlackboard(FileInfo FileInfo)
        {
            BlackboardData BlackboardData = LoadBlackboardFromFile(FileInfo.FullName);
            if (BlackboardData != null)
            {
                BlackboardFileInfos.Add(FileInfo);
                Blackboards.Add(FileInfo.Name, BlackboardData);
            }
        }

        public BlackboardData LoadBlackboardFromFile(string FileName)
        {
            string Content = FileHelper.ReadTextFile(FileName);
            if(Content == "")
            {
                RegisterBlackboard(new FileInfo(FileName));
                return null;
            }

            XmlScript Xml = new XmlScript();
            Xml.Open(FileName);
            Record RootRecord = Xml.GetRootRecord();

            Record RecordBlackboard = RootRecord.FindByTypeString("Blackboard");
            if(RecordBlackboard != null)
            {
                BlackboardData BlackboardData = new BlackboardData();
                BlackboardData.Name = RecordBlackboard.GetString("Name");
                BlackboardData._ParentName = RecordBlackboard.GetString("ParentName");

                int Count = RecordBlackboard.GetChildCount();
                for (int i = 0; i < Count; i++)
                {
                    Record RecordChild = RecordBlackboard.GetChild(i);
                    string TypeString = RecordChild.GetTypeString();
                    if (TypeString == "BlackboardEntry")
                    {
                        string EntryName = RecordChild.GetString("EntryName");
                        string KeyType = RecordChild.GetString("KeyType");
                        bool InstanceSynced = RecordChild.GetBool("InstanceSynced");

                        BlackboardEntry Entry = new BlackboardEntry(EntryName, ConvertKeyType(KeyType), InstanceSynced);
                        BlackboardData.AddEntry(EntryName, Entry);
                    }
                }
                return BlackboardData;
            }
            return null;
        }
    }
}
