using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

using NLog;
using NLog.Config;

using Newtonsoft.Json;

using MySharpServer.Common;
using MySharpServer.Framework;

namespace SimpleSharpServer
{
    class Program
    {
        static CommonServerContainerSetting m_ServerSetting = null;
        static CommonServerContainer m_Server = null;

        static ManualResetEvent m_Watcher = null;

        static string m_AppName = "SimpleSharpServer";
        static int m_ExtraStopAppSeconds = 2;

        static void Main(string[] args)
        {
            m_AppName = "SimpleSharpServer";

            m_ExtraStopAppSeconds = 2;

            m_Watcher = new ManualResetEvent(false);

            AppDomain.CurrentDomain.ProcessExit += (s, ev) =>
            {
                Console.WriteLine(m_AppName + " - process exit");
                if (m_Watcher != null) m_Watcher.Set();
            };

            Console.CancelKeyPress += (s, ev) =>
            {
                Console.WriteLine(m_AppName + " - Ctrl+C pressed");
                if (m_Watcher != null) m_Watcher.Set();
                ev.Cancel = true;
            };

            LogManager.Configuration = new XmlLoggingConfiguration($"{AppContext.BaseDirectory}/NLog.config");

            RemoteCaller.HttpConnectionLimit = 1000; // by default

            var appSettings = ConfigurationManager.AppSettings;
            var allKeys = appSettings.AllKeys;

            foreach (var key in allKeys)
            {
                if (key == "ServiceName") m_AppName = appSettings[key];
                if (key == "ExtraStopServiceSeconds")
                    m_ExtraStopAppSeconds = Convert.ToInt32(appSettings[key].ToString());
                if (key == "OutgoingHttpConnectionLimit")
                    RemoteCaller.HttpConnectionLimit = Convert.ToInt32(appSettings[key].ToString());
                if (key == "AppServerSetting")
                    m_ServerSetting = JsonConvert.DeserializeObject<CommonServerContainerSetting>(appSettings[key]);
            }

            if (m_ServerSetting == null)
            {
                CommonLog.Info("Invalid app config");
                return;
            }

            CommonLog.Info("=== " + m_AppName + " is starting ===");

            if (m_Server == null && m_ServerSetting != null) m_Server = new CommonServerContainer();
            if (m_Server != null && !m_Server.IsWorking() && m_ServerSetting != null)
            {
                m_Server.Start(m_ServerSetting, CommonLog.GetLogger());
                Thread.Sleep(100);
            }

            if (m_Watcher != null)
            {
                m_Watcher.Reset();
                m_Watcher.WaitOne();
            }

            if (m_Server != null && m_Server.IsWorking())
            {
                m_Server.Stop();
                Thread.Sleep(100);
            }

            CommonLog.Info("=== " + m_AppName + " stopped ===");

            Thread.Sleep(1000 * m_ExtraStopAppSeconds);

            //Console.WriteLine();
            //Console.WriteLine("Press any key to quit...");
            //Console.ReadLine();
        }
    }
}
