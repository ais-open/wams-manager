using System;

namespace Ais.Internal.Dcm.Web.Service
{

    public class LoggerService : ILoggerService
    {
        
        public void LogException(string message, Exception exp)
        {
            NLog.Logger logger = NLog.LogManager.GetLogger("default");
            logger.LogException(NLog.LogLevel.Error,string.Format("\n====== Message: {0}\n====== StackTrace:{1}\n======\n",message,exp!=null?exp.ToString():""), exp);
        }
    }
}
