using System;
using System.Collections.Generic;
using System.Text;
using EditorUI;

namespace CrossEditor
{
    enum BasicOperationType
    {
        HasSet,
        NotSet
    }

    class BTDecorator_BlackboardBased : BTDecoratorNode
    {
        public string _SelectedEntryName;
        public Relation _ArithOperation;
        public BasicOperationType _BasicOperation;
        public object _CompareValue;

        private BlackboardEntry CurrentEntry
        {
            get
            {
                BehaviorTree CurrentBT = BehaviorTreeManager.GetInstance().GetBehaviorTreeByBTAuxiliaryNode(this);
                if (CurrentBT == null)
                {
                    return null;
                }
                BlackboardData Blackboard = CurrentBT.Blackboard;
                if (Blackboard == null)
                {
                    return null;
                }
                return Blackboard.GetEntry(_SelectedEntryName);
            }
        }

        public string NodeName
        {
            get
            {
                return _Custom;
            }
            set
            {
                _Custom = value;
            }
        }

        public BTDecorator_BlackboardBased()
        {
            _Name = "Blackboard Based Condition";
            _Custom = _Name;

            _NodeType = BTNodeType.Decorator;

            _SelectedEntryName = "";
            _CompareValue = 0;
        }

        public override void GetContentSize(ref int ContentWidth, ref int ContentHeight)
        {
            int X = 0, Y = 0;
            X = Math.Max(X, _SpanX + _TitleFont.MeasureString_Fast(_Custom));
            Y += _SpanY + _TitleFont.GetCharHeight();

            X = Math.Max(X, _SpanX + _DetailFont.MeasureString_Fast(GetStringContent()));
            Y += _SpanY + _DetailFont.GetCharHeight();

            ContentWidth = X + _SpanX;
            ContentHeight = Y + _SpanY;
        }

        public override void SaveToXml(Record RecordNode)
        {
            base.SaveToXml(RecordNode);
            RecordNode.SetString("Custom", _Custom);
            RecordNode.SetString("SelectedEntryName", _SelectedEntryName);
            RecordNode.SetString("ArithOp", _ArithOperation.ToString());
            RecordNode.SetString("BasicOp", _BasicOperation.ToString());
            if (CurrentEntry != null && CurrentEntry.KeyType.IsComputable())
            {
                if (CurrentEntry.KeyType.DataType == typeof(int))
                {
                    RecordNode.SetInt("CompareValue", Convert.ToInt32(_CompareValue));
                }
                else if (CurrentEntry.KeyType.DataType == typeof(float))
                {
                    RecordNode.SetFloat("CompareValue", Convert.ToSingle(_CompareValue));
                }
            }
            else
            {
                RecordNode.SetString("CompareValue", "NULL");
            }
        }

        public override void LoadFromXml(Record RecordNode, BehaviorTree BehaviorTree)
        {
            base.LoadFromXml(RecordNode, BehaviorTree);
            _Custom = RecordNode.GetString("Custom");
            _SelectedEntryName = RecordNode.GetString("SelectedEntryName");
            _ArithOperation = Enum.Parse<Relation>(RecordNode.GetString("ArithOp"));
            _BasicOperation = Enum.Parse<BasicOperationType>(RecordNode.GetString("BasicOp"));
            if(BehaviorTree.Blackboard != null)
            {
                BlackboardEntry Entry = BehaviorTree.Blackboard.GetEntry(_SelectedEntryName);
                if (Entry != null && Entry.KeyType.IsComputable())
                {
                    if (Entry.KeyType.DataType == typeof(int))
                    {
                        _CompareValue = RecordNode.GetInt("CompareValue");
                    }
                    else if (Entry.KeyType.DataType == typeof(float))
                    {
                        _CompareValue = RecordNode.GetFloat("CompareValue");
                    }
                }
                else
                {
                    _CompareValue = 0;
                }
            }
            else
            {
                _CompareValue = 0;
            }
        }

        public override bool CheckCondition(BehaviorTreeComponent BTComponent)
        {
            if(CurrentEntry == null)
            {
                return false;
            }
            else
            {
                object Value = BTComponent.GetBlackboardComponent().GetValue(CurrentEntry.EntryName);
                if (CurrentEntry.KeyType.IsComputable())
                {
                    return CurrentEntry.KeyType.Compare(Value, _CompareValue, _ArithOperation);
                }
                else
                {
                    return CurrentEntry.KeyType.HasSetOrNot(Value, _BasicOperation);
                }
            }
        }

        public override void DrawContent(int X, int Y, int Width, int Height)
        {
            GraphicsHelper GraphicsHelper = GraphicsHelper.GetInstance();

            GraphicsHelper.DrawString(_TitleFont, _Custom, Color.EDITOR_UI_GENERAL_TEXT_COLOR, X+_SpanX, Y+_SpanY, Width - 2*_SpanX, _TitleFont.GetCharHeight(), TextAlign.CenterLeft);
            GraphicsHelper.DrawString(_DetailFont, GetStringContent(), Color.EDITOR_UI_GENERAL_TEXT_COLOR, X + _SpanX, Y + _TitleFont.GetCharHeight() + 2 * _SpanY, Width - 2 * _SpanX, _DetailFont.GetCharHeight(), TextAlign.CenterLeft);
        }

        public override string GetStringContent()
        {
            string Content = "Blackboard : ";

            if(CurrentEntry == null)
            {
                Content += "Invalid";
            }
            else
            {
                Content += _SelectedEntryName;
                if (CurrentEntry.KeyType.IsComputable())
                {
                    switch (_ArithOperation)
                    {
                        case Relation.EqualTo:
                            Content += " equal to ";
                            break;
                        case Relation.InequalTo:
                            Content += " unequal to ";
                            break;
                        case Relation.GreaterTo:
                            Content += " greater than ";
                            break;
                        case Relation.GreaterEqualTo:
                            Content += " is equal or greater than ";
                            break;
                        case Relation.LowerTo:
                            Content += " less than ";
                            break;
                        case Relation.LowerEqualTo:
                            Content += " is equal or less than ";
                            break;
                    }
                    Content += _CompareValue.ToString();
                }
                else
                {
                    switch (_BasicOperation)
                    {
                        case BasicOperationType.HasSet:
                            Content += " has set";
                            break;
                        case BasicOperationType.NotSet:
                            Content += " not set";
                            break;
                    }
                }
            }

            return Content;
        }
    }
}
