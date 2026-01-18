using System;
using System.IO;

namespace APBridgeAddIn
{
    /// <summary>
    /// Simple file logger for the ArcGIS Pro Bridge Add-In
    /// </summary>
    internal static class Logger
    {
        private static readonly string LogFilePath;
        private static readonly object _lock = new object();

        static Logger()
        {
            // Log to user's AppData folder
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logDir = Path.Combine(appDataPath, "APBridgeAddIn", "Logs");
            Directory.CreateDirectory(logDir);

            var logFileName = $"bridge_{DateTime.Now:yyyyMMdd}.log";
            LogFilePath = Path.Combine(logDir, logFileName);
        }

        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        public static void Error(string message, Exception ex = null)
        {
            var fullMessage = ex != null
                ? $"{message}\n{ex.Message}\n{ex.StackTrace}"
                : message;
            WriteLog("ERROR", fullMessage);
        }

        public static void Warning(string message)
        {
            WriteLog("WARN", message);
        }

        private static void WriteLog(string level, string message)
        {
            try
            {
                lock (_lock)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logEntry = $"[{timestamp}] [{level}] {message}\n";
                    File.AppendAllText(LogFilePath, logEntry);
                }
            }
            catch
            {
                // Fail silently if logging fails
            }
        }

        public static string GetLogPath() => LogFilePath;
    }
}
