using EditorUI;
using System.Collections.Generic;

namespace CrossEditor
{
    internal class EditOperationManager
    {
        private static EditOperationManager _Instance = new EditOperationManager();

        private List<EditOperation> _UndoList;
        private List<EditOperation> _RedoList;

        private bool bRecord;
        private List<EditOperation> _RecordList;

        public static EditOperationManager GetInstance()
        {
            return _Instance;
        }

        private EditOperationManager()
        {
            _UndoList = new List<EditOperation>();
            _RedoList = new List<EditOperation>();
            bRecord = false;
            _RecordList = new List<EditOperation>();
        }

        public void ClearAll()
        {
            _UndoList.Clear();
            _RedoList.Clear();
        }

        public void AddOperation(EditOperation Operation)
        {
            if (bRecord)
            {
                _RecordList.Add(Operation);
            }
            else
            {
                _UndoList.Add(Operation);
                _RedoList.Clear();
            }
        }

        public bool CanUndo()
        {
            return _UndoList.Count > 0;
        }

        public bool CanRedo()
        {
            return _RedoList.Count > 0;
        }

        public void Undo()
        {
            if (!CanUndo())
            {
                return;
            }
            int Last = _UndoList.Count - 1;
            EditOperation Undo = _UndoList[Last];
            _UndoList.RemoveAt(Last);
            Undo.Undo();
            _RedoList.Add(Undo);
        }

        public void Redo()
        {
            if (!CanRedo())
            {
                return;
            }
            int Last = _RedoList.Count - 1;
            EditOperation Redo = _RedoList[Last];
            _RedoList.RemoveAt(Last);
            Redo.Redo();
            _UndoList.Add(Redo);
        }

        public EditOperation GetLatestOperation()
        {
            if (_UndoList.Count > 0)
            {
                return _UndoList[_UndoList.Count - 1];
            }
            else
            {
                return null;
            }
        }

        private bool IsSameModifyPropertyOperation(EditOperation Operation1, EditOperation Operation2)
        {
            if (Operation1 == null)
            {
                return false;
            }
            if (Operation2 == null)
            {
                return false;
            }
            if (!(Operation1 is EditOperation_ModifyProperty))
            {
                return false;
            }
            if (!(Operation2 is EditOperation_ModifyProperty))
            {
                return false;
            }
            EditOperation_ModifyProperty Operation_ModifyProperty1 = (EditOperation_ModifyProperty)Operation1;
            EditOperation_ModifyProperty Operation_ModifyProperty2 = (EditOperation_ModifyProperty)Operation2;
            if (Operation_ModifyProperty1._Object != Operation_ModifyProperty2._Object)
            {
                return false;
            }
            if (Operation_ModifyProperty1._ObjectProperty != Operation_ModifyProperty2._ObjectProperty)
            {
                return false;
            }
            return true;
        }

        private EditOperation CombineTwoModifyPropertyOperation(EditOperation OperationOld, EditOperation OperationNew)
        {
            EditOperation_ModifyProperty Operation_ModifyPropertyOld = (EditOperation_ModifyProperty)OperationOld;
            EditOperation_ModifyProperty Operation_ModifyPropertyNew = (EditOperation_ModifyProperty)OperationNew;
            object Object = Operation_ModifyPropertyNew._Object;
            ObjectProperty ObjectProperty = Operation_ModifyPropertyNew._ObjectProperty;
            object OldValue = Operation_ModifyPropertyOld._OldValue;
            object NewValue = Operation_ModifyPropertyNew._NewValue;
            EditOperation_ModifyProperty Operation_ModifyProperty = new EditOperation_ModifyProperty(Object, ObjectProperty, OldValue, NewValue);
            return Operation_ModifyProperty;
        }

        public void AddOperation_AutoCombine(EditOperation Operation)
        {
            EditOperation LatestOperation = GetLatestOperation();
            if (IsSameModifyPropertyOperation(Operation, LatestOperation))
            {
                int Last = _UndoList.Count - 1;
                _UndoList.RemoveAt(Last);
                EditOperation OperationCombined = CombineTwoModifyPropertyOperation(LatestOperation, Operation);
                _UndoList.Add(OperationCombined);
            }
            else
            {
                _UndoList.Add(Operation);
            }
            _RedoList.Clear();
        }

        public void BeginRecordCompoundOperation()
        {
            DebugHelper.Assert(bRecord == false);
            bRecord = true;
        }

        public void EndRecordCompoundOperation()
        {
            DebugHelper.Assert(bRecord == true);
            bRecord = false;
            EditOperation_Compound EditOperation_Compound = new EditOperation_Compound(_RecordList);
            AddOperation(EditOperation_Compound);
            _RecordList.Clear();
        }
    }
}