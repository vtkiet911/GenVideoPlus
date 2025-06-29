using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using GenVideo.ViewModel;

namespace GenVideo.Model
{
    public class AudioInfo : BaseViewModel
    {
        private string _FilePath;
        public string FilePath { get => _FilePath; set { _FilePath = value; OnPropertyChanged(); } }

        private string _FileName;
        public string FileName { get => _FileName; set { _FileName = value; OnPropertyChanged(); } }

        private string _DurationText;
        public string DurationText { get => _DurationText; set { _DurationText = value; OnPropertyChanged(); } }


    }
}
