using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenVideo.ViewModel;

namespace GenVideo.Model
{
    public class Combinations : BaseViewModel
    {
        private string _Combination;
        public string Combination { get => _Combination; set { _Combination = value; OnPropertyChanged(); } }
    }
}
