using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GenVideo;
using GenVideo.ViewModel;
using xNet;


namespace WPF_SeedingTiktok
{
    /// <summary>
    /// Interaction logic for DevicesKey.xaml
    /// </summary>
    public partial class DevicesKey : Window
    {
        public string Key { get; set; }

        public DevicesKey()
        {
            InitializeComponent();
            CheckKey();
        }
        private void CopyLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(Key);
            MessageBox.Show("Copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void CheckKey()
        {
            var checkVersion = this.Title;
            HttpRequest http = new HttpRequest();
            string getRaw = http.Get("https://script.google.com/macros/s/AKfycbxB5_Uf4r30GarYqVSrRTC0Pg3pOOqfV5UmAFt7eZqiDCDcoPClCDkc-2SiwixREjQaYg/exec").ToString();
            var listKey = getRaw.Split(',');
            string version = checkVersion.Substring(checkVersion.Length - 5, 5);
            Key = CheckDevicesKey();
            Thread.Sleep(100);
            tbKey.Text = Key;
            foreach (var item in listKey)
            {
                if (item.Contains(Key))
                {
                    string expDate = item.Split('-')[2];
                    DateTime today = DateTime.Today;

                    DateTime targetDate;
                    if (DateTime.TryParseExact(expDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out targetDate))
                    {
                        TimeSpan remaining = targetDate - today;

                        if (remaining.Days > 0)
                        {
                            tbKey.Text = Key + "\nKey đã được kích hoạt!";
                            tbKey.Foreground = System.Windows.Media.Brushes.Green;
                            MainWindow mainWindow = new MainWindow(remaining.Days.ToString(), expDate);
                            this.Hide();
                            this.Close();
                            mainWindow.Show();
                            break;
                        }
                        else
                        {
                            tbKey.Text = Key + "\nKey đã hết hạn sử dụng!\nClick vào đây để copy key!";
                            tbKey.Foreground = System.Windows.Media.Brushes.Red;
                            break;
                        }

                    }
                }
                else
                {
                    tbKey.Text = Key + "\nKey chưa được kích hoạt!\nClick vào đây để copy key!";
                    tbKey.Foreground = System.Windows.Media.Brushes.Red;
                }
            }

        }
        string CheckDevicesKey()
        {
            ManagementObjectCollection objectList = null;
            ManagementObjectSearcher objectSearcher = new ManagementObjectSearcher("Select * From Win32_processor");
            objectList = objectSearcher.Get();
            string id = "";
            foreach (ManagementObject obj in objectList)
            {
                id = obj["ProcessorID"].ToString();
            }
            objectSearcher = new ManagementObjectSearcher("Select * From Win32_BaseBoard");
            objectList = objectSearcher.Get();
            String motherBoard = "";
            foreach (ManagementObject obj in objectList)
            {
                motherBoard = (string)obj["SerialNumber"];
            }
            string uniqueID = id + "VTKiet" + motherBoard;

            var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(uniqueID));
            var sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
            }
            string result = sb.ToString().Substring(0, 36);


            return result;
        }
    }
}
