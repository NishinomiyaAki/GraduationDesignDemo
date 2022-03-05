using System.Diagnostics;

namespace EditorUI
{
    public static class DebugHelper
    {
        public static void Assert(bool bCondition)
        {
            if (bCondition == false)
            {
                //Device.GetInstance().ShowMessageBox("Tips", "Assert!");
                Debug.Assert(bCondition);
            }
        }
    }
}