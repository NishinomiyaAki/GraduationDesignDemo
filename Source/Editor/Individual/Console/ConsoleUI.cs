using System;
using EditorUI;

namespace Editor
{
    internal class ConsoleUI
    {
        private static ConsoleUI _Instance = new ConsoleUI();

        public static ConsoleUI GetInstance()
        {
            return _Instance;
        }

        public void ClearAll()
        {
            ConsoleManager.GetInstance().ClearLog();
        }

        public void AddLogItem(LogMessageType Type, string Message)
        {
            DateTime Now = DateTime.Now;
            Message = Now.ToString("HH:mm:ss.fff") + " : " + Message;
            LogItem LogItem = new LogItem(Type, Message);
            ConsoleManager.GetInstance().AddLogItem(LogItem);
        }
    }
}