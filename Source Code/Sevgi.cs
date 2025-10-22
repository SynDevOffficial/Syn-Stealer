using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;

namespace WindowsFormsApp
{
    public partial class Sevgi : Control
    {
        private Timer animationTimer;
        private List<Packet> packets = new List<Packet>();
        private List<float> waveData = new List<float>();
        private Random random = new Random();
        private int scrollOffset = 0;
        private float gridSpacing = 40;

        public Sevgi()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(19, 34, 54);
            ForeColor = Color.White;

            // Wave verisi oluştur
            for (int i = 0; i < 100; i++)
            {
                waveData.Add((float)Math.Sin(i * 0.1f) * 20 + 30);
            }

            animationTimer = new Timer();
            animationTimer.Interval = 30;
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // Grid kaydırması
            scrollOffset += 3;
            if (scrollOffset > gridSpacing) scrollOffset = 0;

            // Yeni packet oluştur
            if (random.Next(0, 100) < 15)
            {
                packets.Add(new Packet(Width, random.Next(Height / 3, Height - Height / 3)));
            }

            // Packetleri hareket ettir
            for (int i = packets.Count - 1; i >= 0; i--)
            {
                packets[i].X -= 5;
                packets[i].Alpha -= 2;

                if (packets[i].X < -50 || packets[i].Alpha <= 0)
                {
                    packets.RemoveAt(i);
                }
            }

            // Wave verisi güncelle
            for (int i = 0; i < waveData.Count; i++)
            {
                waveData[i] += (float)(random.NextDouble() - 0.5f) * 2;
                waveData[i] = Math.Max(10, Math.Min(Height - 10, waveData[i]));
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Grid çiz
            DrawGrid(e.Graphics);

            // Wave çiz
            DrawWave(e.Graphics);

            // Packetleri çiz
            DrawPackets(e.Graphics);

            // İstatistik bilgisi
            DrawStats(e.Graphics);
        }

        private void DrawGrid(Graphics g)
        {
            using (Pen gridPen = new Pen(Color.FromArgb(100, 57, 144, 241), 1))
            {
                // Dikey çizgiler (sağdan sola)
                for (int x = -scrollOffset; x < Width; x += (int)gridSpacing)
                {
                    g.DrawLine(gridPen, x, 0, x, Height);
                }

                // Yatay çizgiler
                for (int y = 0; y < Height; y += (int)gridSpacing)
                {
                    g.DrawLine(gridPen, 0, y, Width, y);
                }
            }
        }

        private void DrawWave(Graphics g)
        {
            using (Pen wavePen = new Pen(Color.FromArgb(200, 57, 144, 241), 2))
            {
                Point[] wavePoints = new Point[waveData.Count];
                float step = (float)Width / waveData.Count;

                for (int i = 0; i < waveData.Count; i++)
                {
                    wavePoints[i] = new Point((int)(i * step), (int)waveData[i]);
                }

                if (wavePoints.Length > 1)
                {
                    g.DrawCurve(wavePen, wavePoints, 0.5f);
                }
            }
        }

        private void DrawPackets(Graphics g)
        {
            foreach (var packet in packets)
            {
                // Paket çerçevesi
                using (Pen packetPen = new Pen(Color.FromArgb(packet.Alpha, 57, 144, 241), 2))
                {
                    g.DrawRectangle(packetPen, packet.X, packet.Y - 8, 30, 16);
                }

                // Paket içeriği
                using (Brush packetBrush = new SolidBrush(Color.FromArgb(packet.Alpha / 2, 57, 144, 241)))
                {
                    g.FillRectangle(packetBrush, packet.X + 2, packet.Y - 6, 26, 12);
                }

                // Paket ID yazısı
                using (Font font = new Font("Arial", 7))
                using (Brush textBrush = new SolidBrush(Color.FromArgb(packet.Alpha, 57, 144, 241)))
                {
                    string packetId = packet.Id.ToString("X2");
                    g.DrawString(packetId, font, textBrush, packet.X + 10, packet.Y - 6);
                }

                // Paket izleri
                using (Pen tracePen = new Pen(Color.FromArgb(packet.Alpha / 3, 57, 144, 241), 1))
                {
                    tracePen.DashStyle = DashStyle.Dash;
                    g.DrawLine(tracePen, packet.X, packet.Y - 10, packet.X - 40, packet.Y - 10);
                }
            }
        }

        private void DrawStats(Graphics g)
        {
            using (Font font = new Font("Courier New", 10, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.FromArgb(200, 57, 144, 241)))
            {
                g.DrawString($"PACKETS: {packets.Count:D3}", font, brush, 10, 10);
                g.DrawString($"BANDWIDTH: {packets.Count * 1.2:F1} Mbps", font, brush, 10, 30);
                g.DrawString($"STATUS: ACTIVE", font, brush, 10, 50);
            }

            // Köşe efekti
            using (Pen cornerPen = new Pen(Color.FromArgb(150, 57, 144, 241), 2))
            {
                g.DrawRectangle(cornerPen, 5, 5, Width - 10, Height - 10);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            animationTimer?.Stop();
            animationTimer?.Dispose();
            base.OnHandleDestroyed(e);
        }

        private class Packet
        {
            public float X { get; set; }
            public float Y { get; set; }
            public int Alpha { get; set; }
            public int Id { get; set; }

            public Packet(int startX, int startY)
            {
                X = startX;
                Y = startY;
                Alpha = 255;
                Id = new Random().Next(256);
            }
        }
    }
}