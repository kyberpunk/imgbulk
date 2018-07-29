using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ImgBulk.Logging
{
    class FileLog : ILog
    {
        private readonly string _filePath;
        private List<LogRow> _logList;

        public FileLog(string filePath)
        {
            _filePath = filePath;
            _logList = new List<LogRow>();
        }

        public FileLog()
            : this("log.xml")
        {
            
        }
        
        public void Log(string message, LogInfo info)
        {
            _logList.Add(new LogRow()
            {
                Time = DateTime.Now,
                Info = info,
                Message = message
            });
        }

        public void Log(Exception exception, LogInfo info)
        {
            _logList.Add(new LogRow()
            {
                Time = DateTime.Now,
                Info = info,
                Message = "Exception " + exception.GetType() + ": " + exception.Message + " in " + exception.StackTrace
            });
        }

        public void Save()
        {
            var doc = !File.Exists(_filePath) ? new XDocument(new XElement("LogRows")) : XDocument.Load(_filePath);
            var logRows = _logList.Select(row => new XElement("LogRow", 
                new XElement("Time", row.Time),
                new XElement("Info", row.Info),
                new XElement("Message", row.Message)));
            doc.Root.Add(logRows);
            doc.Save(_filePath);
            _logList.Clear();
        }

        private class LogRow
        {
            public DateTime Time { get; set; }

            public LogInfo Info { get; set; }

            public string Message { get; set; }
        }
    }
}
