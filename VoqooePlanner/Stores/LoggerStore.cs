using NLog;

namespace VoqooePlanner.Stores
{
    public class LoggerStore
    {
        private readonly Logger _logger;
        public LoggerStore() 
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void LogError(string message, Exception e)
        {
            _logger.Error(e, message);                
        }
    }
}
