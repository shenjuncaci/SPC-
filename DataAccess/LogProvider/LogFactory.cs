using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace DataAccess
{
    /// <summary>
    /// Log4Net日志 工厂
    /// <author>
    ///		<name>shenjun</name>
    ///		<date>2018.08.21</date>
    /// </author>
    /// </summary>
    public class LogFactory
    {
        static LogFactory()
        {
            FileInfo configFile = new FileInfo(HttpContext.Current.Server.MapPath("/XmlConfig/log4net.config"));

            log4net.Config.XmlConfigurator.Configure(configFile);
        }

        public static LogHelper GetLogger(Type type)
        {
            return new LogHelper(LogManager.GetLogger(type));
        }

        public static LogHelper GetLogger(string str)
        {
            return new LogHelper(LogManager.GetLogger(str));
        }
    }
}
