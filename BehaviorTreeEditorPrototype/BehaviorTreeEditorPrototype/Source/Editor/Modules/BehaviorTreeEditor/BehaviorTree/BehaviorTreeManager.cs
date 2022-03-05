using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using EditorUI;

namespace CrossEditor
{
    class BehaviorTreeManager
    {
        static BehaviorTreeManager Instance = new BehaviorTreeManager();
        public Dictionary<string, BehaviorTree> BehaviorTrees;
        List<FileInfo> BehaviorTreeFileInfos;

        public static BehaviorTreeManager GetInstance()
        {
            return Instance;
        }

        public BehaviorTreeManager()
        {
            BehaviorTrees = new Dictionary<string, BehaviorTree>();
            BehaviorTreeFileInfos = new List<FileInfo>();
        }

        public void Initialize(string FolderName)
        {
            DirectoryInfo DirectoryInfo = new DirectoryInfo(FolderName);
            foreach (FileInfo FileInfo in DirectoryInfo.GetFiles())
            {
                if (string.Compare(FileInfo.Extension, ".BT", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    LoadBehaviorTree(FileInfo);
                }
            }
        }

        public void RegisterBehaviorTree(FileInfo FileInfo)
        {
            BehaviorTreeFileInfos.Add(FileInfo);
            BehaviorTree BehaviorTree = new BehaviorTree();
            BehaviorTree.LoadFromXml(FileInfo.FullName);
            BehaviorTrees.Add(FileInfo.Name, BehaviorTree);
        }

        public void UnregisterBehaviorTree(FileInfo FileInfo)
        {
            FileInfo Target = null;
            foreach(FileInfo BehaviorTreeFileInfo in BehaviorTreeFileInfos)
            {
                if(BehaviorTreeFileInfo.FullName == FileInfo.FullName)
                {
                    Target = BehaviorTreeFileInfo;
                }
            }
            BehaviorTreeFileInfos.Remove(Target);
            BehaviorTrees.Remove(FileInfo.Name);
        }

        public void LoadBehaviorTree(FileInfo FileInfo)
        {
            BehaviorTree BehaviorTree = new BehaviorTree();
            BehaviorTree.LoadFromXml(FileInfo.FullName);
            BehaviorTreeFileInfos.Add(FileInfo);
            BehaviorTrees.Add(FileInfo.Name, BehaviorTree);
        }

        public BehaviorTree GetBehaviorTreeByName(string Name)
        {
            BehaviorTree BehaviorTree;
            BehaviorTrees.TryGetValue(Name, out BehaviorTree);
            if(BehaviorTree != null)
            {
                return BehaviorTree;
            }
            return null;
        }

        public BehaviorTree GetBehaviorTreeByBTNode(BTNode BTNode)
        {
            foreach (BehaviorTree BT in BehaviorTrees.Values)
            {
                if (BT.HasBTNode(BTNode))
                {
                    return BT;
                }
            }
            return null;
        }

        public BehaviorTree GetBehaviorTreeByBTAuxiliaryNode(BTAuxiliaryNode BTAuxiliaryNode)
        {
            foreach (BehaviorTree BT in BehaviorTrees.Values)
            {
                if (BT.HasBTAuxiliaryNode(BTAuxiliaryNode))
                {
                    return BT;
                }
            }
            return null;
        }

        public string GetName(BehaviorTree BehaviorTree)
        {
            foreach(KeyValuePair<string, BehaviorTree> Pair in BehaviorTrees)
            {
                if(BehaviorTree == Pair.Value)
                {
                    return Pair.Key;
                }
            }
            return "None";
        }
    }
}
