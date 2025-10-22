using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SysCall_Stealer
{
    public class ModernScrollbar : Control
    {
        private int _value;
        private int _maximum = 100;
        private int _largeChange = 10;
        private int _thumbSize = 50;
        private bool _isThumbDragging = false;
        private int _thumbDragOffset;
        private Timer _hoverTimer;
        private float _hoverProgress;

        public event EventHandler ValueChanged;

        public ModernScrollbar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.DoubleBuffer |
                    ControlStyles.ResizeRedraw, true);

            // Hover animation timer
            _hoverTimer = new Timer();
            _hoverTimer.Interval = 16; // ~60fps
            _hoverTimer.Tick += (s, e) => Invalidate();
        }

        public int Value
        {
            get => _value;
            set
            {
                value = Math.Max(0, Math.Min(_maximum - LargeChange, value));
                if (_value != value)
                {
                    _value = value;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = Math.Max(0, value);
                UpdateThumbSize();
                Invalidate();
            }
        }

        public int LargeChange
        {
            get => _largeChange;
            set
            {
                _largeChange = Math.Max(1, value);
                UpdateThumbSize();
                Invalidate();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateThumbSize();
        }

        private void UpdateThumbSize()
        {
            if (_maximum <= 0) return;

            float percentage = (float)_largeChange / _maximum;
            int trackHeight = Height - 8; // Padding
            _thumbSize = Math.Max(20, (int)(trackHeight * percentage));
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                Rectangle thumbRect = GetThumbRectangle();

                if (thumbRect.Contains(e.Location))
                {
                    // Start dragging thumb
                    _isThumbDragging = true;
                    _thumbDragOffset = e.Y - thumbRect.Y;
                }
                else
                {
                    // Jump to click position
                    int trackHeight = Height - 8 - _thumbSize;
                    int clickPos = e.Y - 4 - (_thumbSize / 2);
                    float percentage = (float)clickPos / trackHeight;
                    Value = (int)(percentage * (_maximum - _largeChange));
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isThumbDragging)
            {
                int trackHeight = Height - 8 - _thumbSize;
                int thumbPos = e.Y - _thumbDragOffset - 4;
                float percentage = (float)thumbPos / trackHeight;
                Value = (int)(percentage * (_maximum - _largeChange));
            }

            // Hover effect
            Rectangle thumbRect = GetThumbRectangle();
            bool isHovering = thumbRect.Contains(e.Location);

            if (isHovering && _hoverProgress < 1f)
            {
                _hoverProgress += 0.1f;
                _hoverTimer.Start();
            }
            else if (!isHovering && _hoverProgress > 0f)
            {
                _hoverProgress -= 0.1f;
                if (_hoverProgress <= 0f)
                    _hoverTimer.Stop();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isThumbDragging = false;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverProgress = 0f;
            _hoverTimer.Stop();
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Value -= e.Delta / 4; // Smooth scrolling
        }

        private Rectangle GetThumbRectangle()
        {
            if (_maximum <= 0) return Rectangle.Empty;

            int trackHeight = Height - 8 - _thumbSize;
            float percentage = (float)Value / (_maximum - _largeChange);
            int thumbY = 4 + (int)(percentage * trackHeight);

            return new Rectangle(2, thumbY, Width - 4, _thumbSize);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Background
            using (var bgBrush = new SolidBrush(Color.FromArgb(30, 30, 40)))
                g.FillRectangle(bgBrush, ClientRectangle);

            // Track
            var trackRect = new Rectangle(4, 4, Width - 8, Height - 8);
            using (var trackBrush = new SolidBrush(Color.FromArgb(50, 50, 60)))
                g.FillRectangle(trackBrush, trackRect);

            // Thumb with hover effect
            var thumbRect = GetThumbRectangle();
            if (!thumbRect.IsEmpty)
            {
                Color thumbColor = Color.FromArgb(
                    (int)(100 + (_hoverProgress * 55)),
                    (int)(200 + (_hoverProgress * 55)),
                    255
                );

                using (var thumbBrush = new SolidBrush(thumbColor))
                    g.FillRectangle(thumbBrush, thumbRect);

                // Thumb border
                using (var borderPen = new Pen(Color.FromArgb(150, 150, 180), 1))
                    g.DrawRectangle(borderPen, thumbRect);
            }
        }
    }
}