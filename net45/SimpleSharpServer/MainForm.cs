using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;

using MySharpServer.Common;
using MySharpServer.Framework;

namespace SimpleSharpServer
{
    public partial class MainForm : Form
    {
        CommonServerContainerSetting m_ServerSetting = null;
        CommonServerContainer m_Server = null;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CommonLog.SetGuiControl(this, mmLog);

            RemoteCaller.HttpConnectionLimit = 1000; // by default

            var appSettings = ConfigurationManager.AppSettings;

            var allKeys = appSettings.AllKeys;

            foreach (var key in allKeys)
            {
                if (key == "OutgoingHttpConnectionLimit")
                    RemoteCaller.HttpConnectionLimit = Convert.ToInt32(appSettings[key].ToString());

                if (key == "AppServerSetting")
                    m_ServerSetting = JsonConvert.DeserializeObject<CommonServerContainerSetting>(appSettings[key]);
            }

            if (m_Server == null && m_ServerSetting != null) m_Server = new CommonServerContainer();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CommonLog.Info("Exiting...");
            this.Text = this.Text + " - Exiting...";
            if (m_Server != null) m_Server.Stop();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (m_Server != null && !m_Server.IsWorking() && m_ServerSetting != null)
            {
                btnStart.Enabled = false;
                CommonLog.Info("Starting...");
                await m_Server.StartAsync(m_ServerSetting, CommonLog.GetLogger());
                btnStop.Enabled = true;
            }
        }

        private async void btnStop_Click(object sender, EventArgs e)
        {
            if (m_Server != null && m_Server.IsWorking())
            {
                btnStop.Enabled = false;
                await m_Server.StopAsync();
                btnStart.Enabled = true;
            }
        }
    }
}
