using System;

namespace Kontur.ImageTransformer.Logger
{
    public static class LoggerWrapper
    {
        public static void LoggError(string message, Exception exception)
        {
            Log4NetLogger.Log.Error(message
                                    + "\n exception Name:" + exception.Message
                                    + "\n StackTrace:" + exception.StackTrace
                                    + "\n Data:" + exception.Data);
        }

        public static void LoggWarning(string message, Exception exception)
        {
            Log4NetLogger.Log.Warn(message
                                   + "\n exception Name:" + exception.Message
                                   + "\n StackTrace:" + exception.StackTrace
                                   + "\n Data:" + exception.Data);
        }
    }
}