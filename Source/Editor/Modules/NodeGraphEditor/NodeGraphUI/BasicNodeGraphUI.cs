using EditorUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrossEditor
{
    class BasicNodeGraphUI
    {
        static BasicNodeGraphUI Instance = new BasicNodeGraphUI();

        // Model
        BasicNodeGraph BasicNodeGraph;
        // View (and partial Controller)
        NodeGraphView View;

        public static BasicNodeGraphUI GetInstance()
        {
            return Instance;
        }

        public BasicNodeGraphUI()
        {
            BasicNodeGraph = new BasicNodeGraph();
            View = new NodeGraphView();
            View.BindModel(BasicNodeGraph);
        }

        public void Initialize()
        {
            View.SaveEvent += OnSave;
            View.RunEvent += OnRun;
            View.DoubleClickEvent += OnDoubleClick;
        }

        public DockingCard GetDockingCard() => View.GetDockingCard();

        public NodeGraphView GetGraphView() => View;

        #region Event

        public void OnSave()
        {
            BasicNodeGraph.SaveToFile();
            View.ClearModified();
        }

        public void OnRun()
        {
            BasicNodeGraph.Run();
            View.ClearModified();
        }

        public void OnDoubleClick(object Context)
        {
            if (Context is Node)
            {
                Node Node = Context as Node;
                if (Node.bHasSubGraph)
                {
                    View.BindModel(Node.SubGraph);
                }
            }
        }

        #endregion

        public void OpenVisualScript(string FilePath)
        {
            BasicNodeGraph.LoadFromFile(FilePath);
            View.UpdateDockingCardText();
            View.RefreshNavigator();
            View.RefreshZoom();

            GetDockingCard().SetTagString1(FilePath);
        }
    }
}
