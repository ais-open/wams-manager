using System;
namespace Ais.Internal.Dcm.Web.Service
{
    
    public interface ILoggerService
    {
        void LogException(string message,Exception exp);
    }

}
