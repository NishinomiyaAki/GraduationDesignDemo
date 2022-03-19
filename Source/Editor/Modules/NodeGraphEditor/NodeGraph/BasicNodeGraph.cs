using System;
using EditorUI;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace CrossEditor
{
    public class BasicNodeGraph : NodeGraphModel
    {
        public string FilePath;

        public BasicNodeGraph() : base()
        {
            Name = "Dashboard";
            Type = NodeGraphType.BasicNodeGraph;
            FilePath = "";
        }

        public override List<MenuBuilder> BuildNodeMenu(Action<Node> ProcessNode)
        {
            List<MenuBuilder> MenuBuilders = new List<MenuBuilder>();

            MenuBuilders.Add(new MenuBuilder
            {
                Text = "Event",
                bHasChild = true,
                Children = new List<MenuBuilder>
                {
                    new MenuBuilder { Text="Start", Event=(Sender)=> { ProcessNode(new FlowNode_Event("Start")); } },
                    new MenuBuilder { Text="Update", Event=(Sender)=> { ProcessNode(new FlowNode_Event("Update")); } }
                }
            });
            MenuBuilders.Add(new MenuBuilder
            {
                Text = "Control",
                bHasChild = true,
                Children = new List<MenuBuilder>
                {
                    new MenuBuilder { Text="If", Event=(Sender)=> { ProcessNode(new FlowNode_If()); } },
                    new MenuBuilder { Text="WhileLoop", Event=(Sender)=> { ProcessNode(new FlowNode_WhileLoop()); } },
                    new MenuBuilder { Text="ForLoop", Event=(Sender)=> { ProcessNode(new FlowNode_ForLoop()); } }
                }
            });
            MenuBuilders.Add(new MenuBuilder
            {
                Text = "LogicOp",
                bHasChild = true,
                Children = new List<MenuBuilder>
                {
                    new MenuBuilder { Text="And", Event=(Sender)=> { ProcessNode(new FlowNode_BinaryLogicOp(BinaryLogicOp.And)); } },
                    new MenuBuilder { Text="Or", Event=(Sender)=> { ProcessNode(new FlowNode_BinaryLogicOp(BinaryLogicOp.Or)); } },
                    new MenuBuilder { Text="Xor", Event=(Sender)=> { ProcessNode(new FlowNode_BinaryLogicOp(BinaryLogicOp.Xor)); } },
                    new MenuBuilder { bIsSeperator = true },
                    new MenuBuilder { Text="Not", Event=(Sender)=> { ProcessNode(new FlowNode_UnaryLogicOp(UnaryLogicOp.Not)); } }
                }
            });
            MenuBuilders.Add(new MenuBuilder
            {
                Text = "Compare",
                bHasChild = true,
                Children = new List<MenuBuilder>
                {
                    new MenuBuilder { Text="==", Event=(Sender)=> { ProcessNode(new FlowNode_Compare(Relation.EqualTo)); } },
                    new MenuBuilder { Text="!=", Event=(Sender)=> { ProcessNode(new FlowNode_Compare(Relation.InequalTo)); } },
                    new MenuBuilder { Text="<", Event=(Sender)=> { ProcessNode(new FlowNode_Compare(Relation.LowerTo)); } },
                    new MenuBuilder { Text="<=", Event=(Sender)=> { ProcessNode(new FlowNode_Compare(Relation.LowerEqualTo)); } },
                    new MenuBuilder { Text=">", Event=(Sender)=> { ProcessNode(new FlowNode_Compare(Relation.GreaterTo)); } },
                    new MenuBuilder { Text=">=", Event=(Sender)=> { ProcessNode(new FlowNode_Compare(Relation.GreaterEqualTo)); } }
                }
            });
            MenuBuilders.Add(new MenuBuilder
            {
                Text = "ArithOp",
                bHasChild = true,
                Children = new List<MenuBuilder>
                {
                    new MenuBuilder { Text="+", Event=(Sender)=> { ProcessNode(new FlowNode_BinaryArithOp(BinaryArithOp.Add)); } },
                    new MenuBuilder { Text="- ", Event=(Sender)=> { ProcessNode(new FlowNode_BinaryArithOp(BinaryArithOp.Substract)); } },
                    new MenuBuilder { Text="*", Event=(Sender)=> { ProcessNode(new FlowNode_BinaryArithOp(BinaryArithOp.Multiply)); } },
                    new MenuBuilder { Text="/", Event=(Sender)=> { ProcessNode(new FlowNode_BinaryArithOp(BinaryArithOp.Divide)); } },
                    new MenuBuilder { Text="%", Event=(Sender)=> { ProcessNode(new FlowNode_BinaryArithOp(BinaryArithOp.Modulo)); } },
                    new MenuBuilder { bIsSeperator = true },
                    new MenuBuilder { Text="- ", Event=(Sender)=> { ProcessNode(new FlowNode_UnaryArithOp(UnaryArithOp.Negative)); } }
                }
            });
            MenuBuilders.Add(new MenuBuilder
            {
                Text = "Constant",
                bHasChild = true,
                Children = new List<MenuBuilder>
                {
                    new MenuBuilder { Text="True", Event=(Sender)=> { ProcessNode(new FlowNode_Bool(true)); } },
                    new MenuBuilder { Text="False", Event=(Sender)=> { ProcessNode(new FlowNode_Bool(false)); } },
                    new MenuBuilder { Text="Integer", Event=(Sender)=> { ProcessNode(new FlowNode_Integer(0)); } },
                    new MenuBuilder { Text="Float", Event=(Sender)=> { ProcessNode(new FlowNode_Float(0.0f)); } },
                    new MenuBuilder { Text="String", Event=(Sender)=> { ProcessNode(new FlowNode_String("")); } }
                }
            });
            MenuBuilders.Add(new MenuBuilder
            {
                Text = "Convert",
                bHasChild = true,
                Children = new List<MenuBuilder>
                {
                    new MenuBuilder { Text="ToInt", Event=(Sender)=> { ProcessNode(new FlowNode_ToInt()); } },
                    new MenuBuilder { Text="ToFloat", Event=(Sender)=> { ProcessNode(new FlowNode_ToFloat()); } },
                    new MenuBuilder { Text="ToString", Event=(Sender)=> { ProcessNode(new FlowNode_ToString()); } }
                }
            });
            MenuBuilders.Add(new MenuBuilder
            {
                Text = "Function",
                bHasChild = true,
                Children = new List<MenuBuilder>
                {
                    new MenuBuilder { Text="PrintString", Event=(Sender)=> { ProcessNode(new FlowNode_PrintString()); } }
                }
            });
            MenuBuilders.Add(new MenuBuilder { Text = "Comment", Event = (Sender) => { ProcessNode(new Node_Comment()); } });

            return MenuBuilders;
        }

        public override void Run()
        {
            FlowNode_Event StartNode = null;
            foreach(Node Node in Nodes)
            {
                if (Node is FlowNode_Event && (Node as FlowNode_Event).EventName == "Start")
                {
                    StartNode = Node as FlowNode_Event;
                }
            }

            if (StartNode != null)
            {
                StartNode.Run();
                ConsoleUI.GetInstance().AddLogItem(
                    LogMessageType.Information,
                    string.Format("Start at FlowNode_Event(ID:{0})", StartNode.ID.ToString())
                    );
            }
            else
            {
                ConsoleUI.GetInstance().AddLogItem(LogMessageType.Warning, "No start event in this graph!");
            }
        }

        public void SaveToFile()
        {
            if (FilePath == "")
            {
                return;
            }

            XmlScript Xml = new XmlScript();
            Record RootRecord = Xml.GetRootRecord();

            Record RecordNodeGraph = RootRecord.AddChild();
            SaveToRecord(RecordNodeGraph);

            Xml.Save(FilePath);
        }

        public void LoadFromFile(string FilePath)
        {
            if (File.Exists(FilePath) == false)
            {
                return;
            }

            this.FilePath = FilePath;
            this.Name = PathHelper.GetNameOfPath(FilePath);

            string Content = FileHelper.ReadTextFile(FilePath);
            if (Content == "")
            {
                return;
            }

            XmlScript Xml = new XmlScript();
            Xml.Open(FilePath);
            Record RootRecord = Xml.GetRootRecord();

            Record RecordNodeGraph = RootRecord.FindByTypeString("NodeGraph");
            LoadFromRecord(RecordNodeGraph);
        }
    }
}
