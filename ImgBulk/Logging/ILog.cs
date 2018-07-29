using System;

namespace ImgBulk.Logging
{
    interface ILog
    {
        void Log(string message, LogInfo info);

        void Log(Exception exception, LogInfo info);

        void Save();
    }
}
