using System;

namespace Unt
{
    public class Log
    {
        public delegate void InfoMethod(object log);
        public delegate void WarningMethod(object log);
        public delegate void ErrorMethod(object log);
        private static InfoMethod infoMethod;
        private static WarningMethod warningMethod;
        private static ErrorMethod errorMethod;
        private static bool includeTimestamps;

        public static void Initialize(InfoMethod info, WarningMethod warning = null, ErrorMethod error = null, bool includeTimestamps = false)
        {
            Log.infoMethod = info;
            Log.warningMethod = warning;
            Log.errorMethod = error;
            Log.includeTimestamps = includeTimestamps;
        }

        public static void Info(object message)
        {
            if (infoMethod != null)
            {
                if (includeTimestamps)
                    infoMethod($"[{GetTimestamp(DateTime.Now)}]:{message}");
                else
                    infoMethod(message);
            }
        }

        public static void Warning(object message)
        {
            if (warningMethod != null)
            {
                if (includeTimestamps)
                    warningMethod($"[{GetTimestamp(DateTime.Now)}]:{message}");
                else
                    warningMethod(message);
            }
        }

        public static void Error(object message)
        {
            if (errorMethod != null)
            {
                if (includeTimestamps)
                    errorMethod($"[{GetTimestamp(DateTime.Now)}]:{message}");
                else
                    errorMethod(message);
            }
        }

        private static string GetTimestamp(DateTime time)
        {
            return time.ToString("HH:mm:ss:fff");
        }
    }
}