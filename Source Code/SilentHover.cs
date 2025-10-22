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
    public partial class SilentHover : Form
    {
        private Timer fadeTimer;
        private double opacityTarget = 1.0;
        public SilentHover()
        {
            InitializeComponent();
            Opacity = 0;
            fadeTimer = new Timer();
            fadeTimer.Interval = 10; // smooth speed
            fadeTimer.Tick += FadeTimer_Tick;
        }


        private void FadeTimer_Tick(object sender, EventArgs e)
        {

            try
            {

                if (Opacity < opacityTarget)
                    Opacity += 0.05;
                else
                    fadeTimer.Stop();
            }
            catch
            {

            }

        }

        public void ShowAtCursor()
        {
            var mousePos = Cursor.Position;
            Location = new System.Drawing.Point(mousePos.X + 10, mousePos.Y + 10);
            Show();
            fadeTimer.Start();
        }

        private void SilentHover_Load(object sender, EventArgs e)
        {
            guna2ShadowForm1.SetShadowForm(this);
        }
    }
}
