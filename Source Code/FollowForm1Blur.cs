using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SysCall_Stealer
{
    public partial class FollowForm1Blur : Form
    {
        private Form parentForm;
        private Timer followTimer;


        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public uint GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }


        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const uint SWP_NOMOVE = 0x2;
        private const uint SWP_NOSIZE = 0x1;
        private const uint SWP_NOACTIVATE = 0x10;

 
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        public FollowForm1Blur(Form parent = null)
        {
            parentForm = parent;
            SetupForm();
            SetupFollower();
        }

        private void SetupForm()
        {

            this.ClientSize = new Size(284, 261);
            this.Name = "FollowForm1Blur";
            this.Load += new EventHandler(this.FollowForm1Blur_Load);

   
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = false;
          
            this.BackColor = Color.Black;
        }

        private void SetupFollower()
        {

            followTimer = new Timer();
            followTimer.Interval = 16; 
            followTimer.Tick += FollowTimer_Tick;
        }

        public void StartFollowing(Form mainForm)
        {
            parentForm = mainForm;

            if (parentForm != null)
            {
      
                parentForm.Move += ParentForm_Move;
                parentForm.Resize += ParentForm_Resize;
                parentForm.VisibleChanged += ParentForm_VisibleChanged;
                parentForm.SizeChanged += ParentForm_SizeChanged;

          
                UpdatePosition();
                UpdateZOrder();


                this.Opacity = 0; 
                double targetOpacity = 0.85;
                Timer fadeTimer = new Timer();
                fadeTimer.Interval = 15;
                fadeTimer.Tick += (sender, e) =>
                {
                    if (this.Opacity < targetOpacity)
                    {
                        this.Opacity += 0.05;
                    }
                    else
                    {
                        this.Opacity = targetOpacity;
                        fadeTimer.Stop();
                        fadeTimer.Dispose();
                    }
                };
                fadeTimer.Start();

   
                followTimer.Start();
            }
        }

        private void FollowTimer_Tick(object sender, EventArgs e)
        {
            if (parentForm != null && !parentForm.IsDisposed)
            {
                UpdatePosition();
                UpdateZOrder();
            }
        }

        private void UpdatePosition()
        {
            if (parentForm != null && parentForm.Visible)
            {
            
                this.Location = parentForm.Location;
                this.Size = parentForm.Size;
                this.WindowState = parentForm.WindowState;

                if (!this.Visible)
                {
                    this.Show();
                }
            }
            else
            {
                if (this.Visible)
                {
                    this.Hide();
                }
            }
        }

        private void UpdateZOrder()
        {
            if (parentForm != null && parentForm.Visible)
            {
                
                SetWindowPos(this.Handle, parentForm.Handle, 0, 0, 0, 0,
                            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }
        }


        private void ParentForm_Move(object sender, EventArgs e)
        {
            UpdatePosition();
        }

        private void ParentForm_Resize(object sender, EventArgs e)
        {
            UpdatePosition();
        }

        private void ParentForm_VisibleChanged(object sender, EventArgs e)
        {
            if (parentForm.Visible)
            {
                this.Show();
            }
            else
            {
                this.Hide();
            }
        }

        private void ParentForm_SizeChanged(object sender, EventArgs e)
        {
     
            this.WindowState = parentForm.WindowState;
            UpdatePosition();
        }

        private void FollowForm1Blur_Load(object sender, EventArgs e)
        {
   
            EnableBlur(this.Handle);
        }

        private void EnableBlur(IntPtr handle)
        {

            AccentPolicy accent = new AccentPolicy
            {
                AccentState = 3,          
                AccentFlags = 0,           
                GradientColor = 0xFFFFFF,  
                AnimationId = 0            
            };

            int accentStructSize = Marshal.SizeOf(accent);
            IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            WindowCompositionAttributeData data = new WindowCompositionAttributeData
            {
                Attribute = 19,                
                Data = accentPtr,
                SizeOfData = accentStructSize
            };

        
            SetWindowCompositionAttribute(handle, ref data);

            
            Marshal.FreeHGlobal(accentPtr);
        }


        protected override bool ShowWithoutActivation => true;

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            followTimer?.Stop();
            followTimer?.Dispose();

         
            if (parentForm != null)
            {
                parentForm.Move -= ParentForm_Move;
                parentForm.Resize -= ParentForm_Resize;
                parentForm.VisibleChanged -= ParentForm_VisibleChanged;
                parentForm.SizeChanged -= ParentForm_SizeChanged;
            }

            base.OnFormClosed(e);
        }


        public void DisableBlur()
        {
            AccentPolicy accent = new AccentPolicy
            {
                AccentState = 0,          
                AccentFlags = 0,
                GradientColor = 0,
                AnimationId = 0
            };

            int accentStructSize = Marshal.SizeOf(accent);
            IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            WindowCompositionAttributeData data = new WindowCompositionAttributeData
            {
                Attribute = 19,
                Data = accentPtr,
                SizeOfData = accentStructSize
            };

            SetWindowCompositionAttribute(this.Handle, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }

        private void FollowForm1Blur_Load_1(object sender, EventArgs e)
        {

        }
    }
}