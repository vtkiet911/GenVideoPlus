using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using GenVideo.ViewModel;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using TagLib.Ape;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Interop;
using System.IO;

namespace GenVideo.Model
{
    public class MyStyleFonts : BaseViewModel
    {
        private string _pointX;
        public string PointX { get => _pointX; set { _pointX = value; OnPropertyChanged();}}
        private string _pointY;
        public string PointY { get => _pointY; set { _pointY = value; OnPropertyChanged(); } }
        private string _sizeFont;
        public string SizeFont { get => _sizeFont; set { _sizeFont = value; OnPropertyChanged(); } }

        // Danh sách item cho ComboBox
        private ObservableCollection<string> _items;
        public ObservableCollection<string> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<string> _styles;
        public ObservableCollection<string> Styles
        {
            get => _styles;
            set
            {
                _styles = value;
                OnPropertyChanged();
            }
        }
        // Item đang được chọn
        private string _selectedItem;
        public string SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }


        public MyStyleFonts()
        {
            // Khởi tạo danh sách font giả lập
            /*PointX = "0";
            PointY = "1155";
            SelectedItem = "Comic Sans";
            SelectedItemStyles = "Kiểu 1";*/

            Items = new ObservableCollection<string>
            {
                "Kiểu 1",
                "Kiểu 2",
                "Kiểu 3"
            };

        }

        

       
    }
}
