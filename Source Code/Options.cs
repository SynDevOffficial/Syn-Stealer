using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SysCall_Stealer
{
    public partial class Options : UserControl
    {
        Logs logs;
        public Options()
        {
            InitializeComponent();
            logs = new Logs();
        }
        public void SetLogsInstance(Logs logsInstance)
        {
            logs = logsInstance;
        }
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            if (logs == null)
            {
                MessageBox.Show("Logs control not initialized!");
                return;
            }

            if (!int.TryParse(portTextBox.Text, out int port) || port <= 0 || port > 65535)
            {
                MessageBox.Show("Please enter a valid port number (1–65535).", "Invalid Port",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            logs.SetPort(port);
            logs.StartServer();
            status.Text = "SERVER IS RUNNING!";
           logs.packets.Visible = true;
            logs.pictureBox1.Visible = true;
            logs.label1.Visible = true;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (logs == null)
            {
                MessageBox.Show("Logs control not initialized!", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            logs.StopServer();
            MessageBox.Show("Server stopped successfully.", "Info",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            status.Text = "SERVER IS STOPPED!";
            logs.packets.Visible = false;
            logs.pictureBox1.Visible = false;
            logs.label1.Visible = false;
        }

        private void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch1.Checked == true)
            {
                portTextBox.Enabled = true;
            }
            else
            {
                portTextBox.Enabled = false;
            }
        }
    }
}
