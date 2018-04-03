using log4net;
using log4net.Config;

namespace Kontur.ImageTransformer.Logger
{
    internal static class Log4NetLogger
    {
        static Log4NetLogger()
        {
            XmlConfigurator.Configure();
        }

        public static ILog Log { get; } = LogManager.GetLogger("LOGGER");
    }
}