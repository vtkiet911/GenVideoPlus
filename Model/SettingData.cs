using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenVideo.ViewModel;

namespace GenVideo.Model
{
    public class SettingData : BaseViewModel
    {
        private string _Duration;
        public string Duration { get => _Duration; set { _Duration = value; OnPropertyChanged(); } }

        private string _Quantity;
        public string Quantity { get => _Quantity; set { _Quantity = value; OnPropertyChanged(); } }
    }
}
