using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GenVideo.ViewModel;

namespace GenVideo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel;
        public string tempExpDate = string.Empty;
        public string tempExpDate2 = string.Empty;
        public MainWindow(string expDate, string ExpDate)
        {
            InitializeComponent();
            tempExpDate = expDate;
            tempExpDate2 = ExpDate;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = ViewModel = new MainViewModel();
            ViewModel.GetExpDate(tempExpDate, tempExpDate2);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ViewModel.SaveData();
            try
            {
                // Danh sách các tiến trình cần kill
                string[] processNames = { "ffmpeg" };

                foreach (var processName in processNames)
                {
                    var processes = Process.GetProcessesByName(processName);
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(); // Đợi tiến trình đóng hoàn toàn
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Không thể kill {processName}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
