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

namespace GenVideo.Model
{
    public class SettingUI : BaseViewModel
    {
        private string _ExpDate;
        public string ExpDate { get => _ExpDate; set { _ExpDate = value; OnPropertyChanged(); } }

        

        private bool _IsSplit;
        public bool IsSplit { get => _IsSplit; set { _IsSplit = value; OnPropertyChanged(); } }

        private bool _IsVga;
        public bool IsVga { get => _IsVga; set { _IsVga = value; OnPropertyChanged(); } }

        private bool _IsStartGen;
        public bool IsStartGen { get => _IsStartGen; set { _IsStartGen = value; OnPropertyChanged(); } }

        private string _Title;
        public string Title { get => _Title; set { _Title = value; OnPropertyChanged(); } }

        private string _Complete;
        public string Complete { get => _Complete; set { _Complete = value; OnPropertyChanged(); } }

        private double _PercentComplete;
        public double PercentComplete { get => _PercentComplete; set { _PercentComplete = value; OnPropertyChanged(); } }

        private ObservableCollection<string> _Audio;
        public ObservableCollection<string> Audio { get => _Audio; set { _Audio = value; OnPropertyChanged(); } }

        private ObservableCollection<string> _Audios;
        public ObservableCollection<string> Audios { get => _Audios; set { _Audios = value; OnPropertyChanged(); } }

        private string _VolumnAudio;
        public string VolumnAudio { get => _VolumnAudio; set { _VolumnAudio = value; OnPropertyChanged(); } }

        private string _AllDuration;
        public string AllDuration { get => _AllDuration; set { _AllDuration = value; OnPropertyChanged(); } }

        private int _Duration;
        public int Duration { get => _Duration; set { _Duration = value; OnPropertyChanged(); } }

        private int _MaxDuration;
        public int MaxDuration { get => _MaxDuration; set { _MaxDuration = value; OnPropertyChanged(); } }

        private int _Quantity;
        public int Quantity { get => _Quantity; set { _Quantity = value; OnPropertyChanged(); } }

        private int _MaxQuantity;
        public int MaxQuantity { get => _MaxQuantity; set { _MaxQuantity = value; OnPropertyChanged(); } }

        private string _SumCombination;
        public string SumCombination { get => _SumCombination; set { _SumCombination = value; OnPropertyChanged(); } }
        private string _SumDuration;
        public string SumDuration { get => _SumDuration; set { _SumDuration = value; OnPropertyChanged(); } }

        private ImageSource _Image;
        public ImageSource  Image { get => _Image; set { _Image = value; OnPropertyChanged(); } }
       
    }
}
