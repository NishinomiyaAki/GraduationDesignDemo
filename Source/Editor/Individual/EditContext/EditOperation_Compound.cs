using System.Collections.Generic;

namespace Editor
{
    internal class EditOperation_Compound : EditOperation
    {
        private List<EditOperation> _RecordList;

        public EditOperation_Compound(List<EditOperation> RecordList)
        {
            _RecordList = RecordList;
        }

        public override void Undo()
        {
            for (int i = _RecordList.Count - 1; i >= 0; i--)
            {
                _RecordList[i].Undo();
            }
        }

        public override void Redo()
        {
            for (int i = 0; i < _RecordList.Count; i++)
            {
                _RecordList[i].Redo();
            }
        }
    }
}