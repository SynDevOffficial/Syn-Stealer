using Guna.UI2.WinForms;
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
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
        }
        private string GenerateRandomString(int length = 10)
        {
            string selectedLanguage = guna2ComboBox1.SelectedItem?.ToString() ?? "English";
            string chars = "";

            switch (selectedLanguage)
            {
                case "English":
                    chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    break;
                case "Arabic":
                    chars = "ابتثجحخدذرزسشصضطظعغفقكلمنهويءآأؤإئةى";
                    break;
                case "China":
                    chars = "的一是不了人我在有他这为之大来以个中上们到说国和地也子时道出而要于就下得可你年生自会那后能对着事其里所去行过家十用发天如然作方成者多日都三小军二无同么经法当起与好看学进种将还分此心前面又定见只主没公从";
                    break;
                default:
                    chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    break;
            }

            Random random = new Random();
            char[] result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            guna2TextBox1.Text = GenerateRandomString(80);
        }
    }
}
