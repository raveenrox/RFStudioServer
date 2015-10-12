using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace RFStudioServer
{
    public partial class mainUI : Form
    {
        private PrivateSocket socket;
        private Thread socketThread;
        private String username = "";
        private String password = "";

        public mainUI()
        {
            InitializeComponent();
            btnStop.Enabled = false;
            prefLoad();
            loadAccount();
            if(cbAuto.Checked)
            {
                socketStart();
            }
            if (cbMin.Checked)
            {
                this.WindowState = FormWindowState.Minimized;
            }

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            socketStart();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            //TODO FixMe
            socketStop();
            notifyIcon.Visible = false;
            Environment.Exit(0);
        }
        private void btnSaveAccount_Click(object sender, EventArgs e)
        {
            loadAccount();
            lblStatus.Text = "Account Details Saved";

        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            savePref();
        }

        private void mainUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon.Visible = false;
            Environment.Exit(0);
        }

        private void mainUI_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                hideForm();
            }
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            showForm();
        }

        private void mainUI_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon.Visible = false;
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                showForm();
            }
            else
            {
                hideForm();
            }
        }

        private void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            showForm();
        }

        public String processData(String data)
        {
            //TODO UPDATE THIS
            /**************************/
            /***EVERYTHING GOES HERE***/
            /**USE 'data' TO GET TEXT**/
            /**vv**vv**vv**vv**vv**vv**/
            if(data.StartsWith("ping"))
            {
                return "SUCCESSFUL";
            }
            else
            {
                return "EMPTY_RESPONSE : " + data;
            }
        }

        private void socketStart()
        {
            socket = new PrivateSocket(this, Int32.Parse(txtPort.Text));
            socketThread = new Thread(socket.StartListening);
            socketThread.Start();
            btnStart.Enabled = false;
            txtPort.Enabled = false;
            btnStop.Enabled = true;
            gbAccount.Enabled = false;
            lblStatus.Text = "Socket Started";
        }

        private void socketStop()
        {
            socket.stopConnection();
            socketThread.Abort();
            btnStop.Enabled = false;
            btnStart.Enabled = true;
            txtPort.Enabled = true;
            gbAccount.Enabled = true;
            lblStatus.Text = "Socket Stopped";
        }


        public bool accountVerify(string data)
        {
            String usr = "";
            String pwd = "";
            char[] arr;
            arr = data.ToCharArray();
            usr = data.Substring(0, data.IndexOf(":"));
            pwd = data.Substring(data.IndexOf(":") + 1);

            if (usr == this.username)
            {
                if (pwd == this.password)
                {
                    return true;
                }
            }
            return false;
        }

        public void logError(Exception e)
        {
            File.AppendAllText("ErrorLog.log", "\n" + DateTime.Now + "\t" + e.ToString() + e.StackTrace);
        }

        public void logError(string line)
        {
            File.AppendAllText("ErrorLog.log", "\n" + DateTime.Now + "\t" + line);
        }

        public void logData(string line)
        {
            File.AppendAllText("IncomingLog.log", "\n" + DateTime.Now + "\t" + line);
        }

        private void loadAccount()
        {
            username = txtUsername.Text;
            password = txtPassword.Text;
        }

        private void prefLoad()
        {
            string xml = "";
            try
            {
                xml = Base64Decode(File.ReadAllText("pref.bin"));

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);

                XmlNode usernameNode = xmlDoc.GetElementsByTagName("username")[0];
                XmlNode passwordNode = xmlDoc.GetElementsByTagName("password")[0];
                XmlNode portNode = xmlDoc.GetElementsByTagName("port")[0];
                XmlNode autoStartNode = xmlDoc.GetElementsByTagName("auto")[0];
                XmlNode minimizedNode = xmlDoc.GetElementsByTagName("minimized")[0];

                txtUsername.Text = usernameNode.InnerText;
                txtPassword.Text = Base64Decode(passwordNode.InnerText);
                txtPort.Text = portNode.InnerText;
                if(autoStartNode.InnerText=="True")
                {
                    cbAuto.Checked = true;
                }else if(autoStartNode.InnerText=="False")
                {
                    cbAuto.Checked = false;
                }
                if(minimizedNode.InnerText=="True")
                {
                    cbMin.Checked = true;
                }else if (minimizedNode.InnerText == "False")
                {
                    cbMin.Checked = false;
                }

            }
            catch (Exception e) { logError(e); }
        }

        private void savePref()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement element = (XmlElement)xmlDoc.AppendChild(xmlDoc.CreateElement("details"));
            element.AppendChild(xmlDoc.CreateElement("username")).InnerText = txtUsername.Text;
            element.AppendChild(xmlDoc.CreateElement("password")).InnerText = Base64Encode(txtPassword.Text);
            element.AppendChild(xmlDoc.CreateElement("port")).InnerText = txtPort.Text;
            element.AppendChild(xmlDoc.CreateElement("auto")).InnerText = cbAuto.Checked.ToString();
            element.AppendChild(xmlDoc.CreateElement("minimized")).InnerText = cbMin.Checked.ToString();
            File.WriteAllText("pref.bin", Base64Encode(xmlDoc.OuterXml));
            lblStatus.Text = "Preferences Saved";
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public void updateStatus(String line)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => lblStatus.Text = line));
            }
        }

        private void showForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void hideForm()
        {
            if (socket != null)
            {
                notifyIcon.BalloonTipText = "The server is running in background";
            }
            else
            {
                notifyIcon.BalloonTipText = "The server is stopped";
            }
            //notifyIcon.ShowBalloonTip(500);
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
        }
    }
}
