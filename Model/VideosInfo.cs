using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using GenVideo.ViewModel;

namespace GenVideo.Model
{
    public class VideosInfo : BaseViewModel
    {
        private string _FilePath;
        public string FilePath { get => _FilePath; set { _FilePath = value; OnPropertyChanged(); } }

        private ImageSource _Thumbnail;
        public ImageSource Thumbnail { get => _Thumbnail; set { _Thumbnail = value; OnPropertyChanged(); } }

        private string _DurationText;
        public string DurationText { get => _DurationText; set { _DurationText = value; OnPropertyChanged(); } }

        private string _DurationTime;
        public string DurationTime { get => _DurationTime; set { _DurationTime = value; OnPropertyChanged(); } }

    }
}
