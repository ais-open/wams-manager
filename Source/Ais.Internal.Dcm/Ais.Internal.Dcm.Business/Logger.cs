using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace Ais.Internal.Dcm.Business
{
    public class Logger
    {
        static string m_baseDir = null;

        static Logger()
        {
            m_baseDir = AppDomain.CurrentDomain.BaseDirectory +
                   AppDomain.CurrentDomain.RelativeSearchPath;
        }

        //returns filename in format: YYYMMDD
        public static string GetFilenameYyymmdd(string suffix, string extension)
        {
            return System.DateTime.Now.ToString("yyyy_MM_dd") 
                + suffix 
                + extension;
        }

        public static void WriteLog(String message)
        {
            try
            {
                string filename = m_baseDir
                    + GetFilenameYyymmdd("_LOG", ".log");
                StreamWriter sw = new System.IO.StreamWriter(filename, true);
                XElement xmlEntry = new XElement("logEntry",
                    new XElement("Date", System.DateTime.Now.ToString(CultureInfo.InvariantCulture)),
                    new XElement("Message", message));
                sw.WriteLine(xmlEntry);
                sw.Close();
            }
            catch (Exception)
            { }
        }

        public static void WriteLog(Exception ex)
        {
            try
            {
                string filename = m_baseDir
                    + GetFilenameYyymmdd("_LOG", ".log");
                StreamWriter sw = new System.IO.StreamWriter(filename, true);
                XElement xmlEntry = new XElement("logEntry",
                    new XElement("Date", System.DateTime.Now.ToString()),
                    new XElement("Exception",
                        new XElement("Source", ex.Source),
                        new XElement("Message", ex.Message),
                        new XElement("Stack", ex.StackTrace)
                     )//end exception
                );
                //has inner exception?
                if (ex.InnerException != null)
                {
                    xmlEntry.Element("Exception").Add(
                        new XElement("InnerException",
                            new XElement("Source", ex.InnerException.Source),
                            new XElement("Message", ex.InnerException.Message),
                            new XElement("Stack", ex.InnerException.StackTrace))
                        );
                }
                sw.WriteLine(xmlEntry);
                sw.Close();
            }
            catch (Exception) { }
        }
    }
}
