using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;
using MS.WindowsAPICodePack.Internal;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using GenVideo.Model;
using GenVideo.Properties;
using xNet;
using System.Globalization;
using System.Drawing;
using TagLib.Ape;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Drawing.Drawing2D;

namespace GenVideo.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        #region Properties
        public string appPath = AppDomain.CurrentDomain.BaseDirectory;

        private SettingData _SettingData;
        public SettingData SettingData { get => _SettingData; set { _SettingData = value; OnPropertyChanged(); } }

        private SettingUI _SettingUI;
        public SettingUI SettingUI { get => _SettingUI; set { _SettingUI = value; OnPropertyChanged(); } }

        private ObservableCollection<ProfileDetail> _Profiles;
        public ObservableCollection<ProfileDetail> Profiles { get => _Profiles; set { _Profiles = value; OnPropertyChanged(); } }

        private ObservableCollection<VideosInfo> _VideosInfo;
        public ObservableCollection<VideosInfo> VideosInfo { get => _VideosInfo; set { _VideosInfo = value; OnPropertyChanged(); } }

        private ObservableCollection<AudioInfo> _AudioInfo;
        public ObservableCollection<AudioInfo> AudioInfo { get => _AudioInfo; set { _AudioInfo = value; OnPropertyChanged(); } }
        #endregion
        private List<List<string>> _Combinations;
        public List<List<string>> Combinations { get => _Combinations; set { _Combinations = value; OnPropertyChanged(); } }

        private List<string> _ListCombinations;
        public List<string> ListCombinations { get => _ListCombinations; set { _ListCombinations = value; OnPropertyChanged(); } }

        private MyStyleFonts _MyStyleFonts;
        public MyStyleFonts MyStyleFonts { get => _MyStyleFonts; set { _MyStyleFonts = value; OnPropertyChanged(); } }

        public ICommand PasteCommand { get; }
        public MainViewModel()
        {
            FirstLoad();
            LoadCommand();
            PasteCommand = new RelayCommand<object>(
            _ => true,
            _ => PasteFromClipboard()
        );
        }

        #region CMD
        public ICommand ChoosenVideo_CMD { get; set; }
        public ICommand ChoosenAudio_CMD { get; set; }
        public ICommand GenerateCombine_CMD { get; set; }
        public ICommand GenerateVideo_CMD { get; set; }
        public ICommand AddData_CMD { get; set; }
        public ICommand PauseProfile_CMD { get; set; }
        public ICommand ResumeProfile_CMD { get; set; }
        public ICommand StartProfile_CMD { get; set; }
        public ICommand DeleteProfile_CMD { get; set; }
        public ICommand CreateProfile_CMD { get; set; }
        public ICommand StartAll_CMD { get; set; }
        public ICommand StopAll_CMD { get; set; }
        public ICommand StopProfile_CMD { get; set; }
        public ICommand DeleteAll_CMD { get; set; }

        public ICommand SelectedItemChangedCommand { get; set; }
        #endregion

        #region Method
        void FirstLoad()
        {
            LoadSavedData();
            SetModel();
        }

        void LoadCommand()
        {

            ChoosenVideo_CMD = new RelayCommand<VideosInfo>((p) => { return true; }, (p) => { ChoosenVideos(); });
            ChoosenAudio_CMD = new RelayCommand<VideosInfo>((p) => { return true; }, (p) => { ChoosenAudio(); });
            GenerateCombine_CMD = new RelayCommand<VideosInfo>((p) => { return true; }, (p) => { GenerateCombine(); });
            GenerateVideo_CMD = new RelayCommand<SettingUI>((p) => { return true; }, (p) => { GenerateVideo(); });

            StartAll_CMD = new RelayCommand<ProfileDetail>((p) => { return Profiles != null; }, (p) => { StartAll(); });
            StopAll_CMD = new RelayCommand<ProfileDetail>((p) => { return Profiles != null; }, (p) => { StopAll(); });
        }
        private string _selectedItem = "Kiểu 1";
        public string SelectedItem
        {
            get
            {
                if (MyStyleFonts.SelectedItem == null)
                    MyStyleFonts.SelectedItem = _selectedItem;

                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();

                /*// Gọi xử lý luôn tại đây
                if (_selectedItem != null)
                    MessageBox.Show($"Đã chọn: {_selectedItem}");*/

                // Khi SelectedItem đổi → gọi Command
                SelectedItemChangedCommand.Execute(SelectedItem);
            }
        }

        private string _inputTextPointX = "0";
        public string InputTextPointX
        {
            get => _inputTextPointX;
            set
            {
                if (_inputTextPointX != value)
                {
                    _inputTextPointX = value;
                    OnPropertyChanged();

                    if (double.TryParse(_inputTextPointX, out var px))
                        MyStyleFonts.PointX = _inputTextPointX;

                    SelectedItemChangedCommand.Execute(SelectedItem);
                }
            }
        }

        private string _inputTextPointY = "1155";
        public string InputTextPointY
        {
            get
            {
                if (MyStyleFonts.PointY == null)
                    MyStyleFonts.PointY = _inputTextPointY;

                return _inputTextPointY;
            }
            set
            {
                if (_inputTextPointY != value)
                {
                    _inputTextPointY = value;
                    OnPropertyChanged();

                    if (double.TryParse(_inputTextPointY, out var px))
                        MyStyleFonts.PointY = _inputTextPointY;

                    SelectedItemChangedCommand.Execute(SelectedItem);
                }
            }
        }

        private string _inputTextSizeFont = "55";
        public string InputTextSizeFont
        {
            get
            {
                // đồng bộ _inputTextSizeFont từ MyStyleFonts khi get
                if (MyStyleFonts.SizeFont == null)
                    MyStyleFonts.SizeFont = _inputTextSizeFont;

                return _inputTextSizeFont;
            }
            set
            {
                if (_inputTextSizeFont != value)
                {
                    _inputTextSizeFont = value;
                    OnPropertyChanged();

                    if (double.TryParse(_inputTextSizeFont, out var px))
                        MyStyleFonts.SizeFont = _inputTextSizeFont;

                    SelectedItemChangedCommand.Execute(SelectedItem);
                }
            }
        }
        private void PasteFromClipboard()
        {
            Content.Clear();
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText();
                string[] lines = text.Split(
                    new[] { "\r\n", "\n" },
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    Content.Add(line.Trim());
                }
            }
            SelectedLine = Content.FirstOrDefault();
        }
        public ICommand TextChangedCommand { get; }


        void OnSelectedItemChanged(string selected)
        {
            selected = SelectedLine;
            // ❌ Không MessageBox trong MVVM, thay vào đó raise event hoặc xử lý logic
            // Ở đây demo thôi:
            System.Diagnostics.Debug.WriteLine($"Bạn chọn: {selected}");

            System.Drawing.Image bitmap = Bitmap.FromFile(appPath + "tiktok interface design.png");
            Graphics g = Graphics.FromImage(bitmap);

            StringFormat strformat1 = new StringFormat();
            strformat1.Alignment = StringAlignment.Near;

            System.Drawing.Color strColor1 = ColorTranslator.FromHtml("#ed0c1b");

            Font font = new Font("Comic Sans MS", int.Parse(MyStyleFonts.SizeFont), System.Drawing.FontStyle.Bold);

            /*if (SettingUI == null)
            {
                SettingUI = new SettingUI();
            }*/

            if (SelectedItem == "Kiểu 1")
            {
                System.Drawing.Brush textBrush = System.Drawing.Brushes.White;

                // Tạo hiệu ứng Glow (viền đỏ mờ)
                for (int i = (int.Parse(MyStyleFonts.SizeFont) / 5); i >= 1; i--) // vẽ nhiều lớp viền từ to -> nhỏ
                {
                    using (System.Drawing.Pen glowPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(50, System.Drawing.Color.Red), i * 2)
                    {
                        LineJoin = System.Drawing.Drawing2D.LineJoin.Round
                    })
                    {
                        g.DrawPath(glowPen, GetTextPath(selected, font, new System.Drawing.Size(951, 1920), int.Parse(MyStyleFonts.PointY)));
                    }
                }

                // Vẽ chữ chính (trắng)
                using (System.Drawing.Brush whiteBrush = new SolidBrush(System.Drawing.Color.White))
                {
                    g.FillPath(whiteBrush, GetTextPath(selected, font, new System.Drawing.Size(951, 1920), int.Parse(MyStyleFonts.PointY)));
                }

                //g.DrawString(selected, font, new SolidBrush(strColor1), new System.Drawing.Point(int.Parse(MyStyleFonts.PointX), int.Parse(MyStyleFonts.PointY)), strformat1);
                //bitmap.Save("Arial.png");
                SettingUI.Image = ConvertBitmapToBitmapSource((Bitmap)bitmap);
                //SaveImageSourceToFile(SettingUI.Image, "Arial2.png");
            }
            if (SelectedItem == "Kiểu 2")
            {
                // Brush tô chữ (màu trắng)
                using (System.Drawing.Brush whiteBrush = new SolidBrush(System.Drawing.Color.White))
                using (System.Drawing.Pen outlinePen = new System.Drawing.Pen(System.Drawing.Color.Black, (int.Parse(MyStyleFonts.SizeFont) / 2)) // Độ dày viền
                {
                    LineJoin = System.Drawing.Drawing2D.LineJoin.Round // bo góc mượt
                })
                {
                    GraphicsPath textPath = GetTextPath(
                        selected,
                        font,
                        new System.Drawing.Size(951, 1920),
                        int.Parse(MyStyleFonts.PointY));

                    // Vẽ viền (đen)
                    g.DrawPath(outlinePen, textPath);

                    // Vẽ chữ (trắng)
                    g.FillPath(whiteBrush, textPath);
                }

                //g.DrawString(selected, font, new SolidBrush(strColor1), new System.Drawing.Point(int.Parse(MyStyleFonts.PointX), int.Parse(MyStyleFonts.PointY)), strformat1);
                //bitmap.Save("Arial.png");
                SettingUI.Image = ConvertBitmapToBitmapSource((Bitmap)bitmap);
                //SaveImageSourceToFile(SettingUI.Image, "Arial2.png");
            }
            if (SelectedItem == "Kiểu 3")
            {

                // Đo kích thước chữ
                var textSize = g.MeasureString(selected, font);

                // Tính vị trí để căn giữa
                float x = (951 - textSize.Width) / 2f;
                float y = int.Parse(MyStyleFonts.PointY);

                // Padding cho nền vàng
                int paddingX = 20;
                int paddingY = 10;

                RectangleF bgRect = new RectangleF(
                    x - paddingX,
                    y - paddingY,
                    textSize.Width + paddingX * 2,
                    textSize.Height + paddingY * 2
                );

                int radius = 30;

                // Vẽ nền vàng
                using (GraphicsPath path = RoundedRect(bgRect, radius))
                using (var bgBrush = new SolidBrush(System.Drawing.Color.Yellow))
                {
                    g.FillPath(bgBrush, path);
                }

                // Vẽ chữ đen
                using (var textBrush = new SolidBrush(System.Drawing.Color.Black))
                {
                    g.DrawString(selected, font, textBrush, new PointF(x, y));
                }


                //g.DrawString(selected, font, new SolidBrush(strColor1), new System.Drawing.Point(int.Parse(MyStyleFonts.PointX), int.Parse(MyStyleFonts.PointY)), strformat1);
                //bitmap.Save("Arial.png");
                SettingUI.Image = ConvertBitmapToBitmapSource((Bitmap)bitmap);
                    //SaveImageSourceToFile(SettingUI.Image, "Arial2.png");
                
            }
        }
        // Hàm tạo GraphicsPath cho rectangle bo góc
        static GraphicsPath RoundedRect(RectangleF rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radius * 2;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }
        void DrawText(string text)
        {
            OnSelectedItemChanged(text);
        }
        // Hàm tạo GraphicsPath từ chữ
        static System.Drawing.Drawing2D.GraphicsPath GetTextPath(string text, Font font, System.Drawing.Size canvasSize, int pointY)
        {
            if (text == null)
            {
                text = "Default text";
            }
            var path = new System.Drawing.Drawing2D.GraphicsPath();

            // Dùng StringFormat để đo chữ
            StringFormat format = StringFormat.GenericDefault;

            // Đo kích thước chữ
            using (Bitmap tempBmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(tempBmp))
            {
                SizeF textSize = g.MeasureString(text, font);

                // Tính vị trí canh giữa
                float x = (canvasSize.Width - textSize.Width) / 2;
                //float y = (canvasSize.Height - textSize.Height) / 2;
                PointF centerPoint = new PointF(x, pointY);

                path.AddString(
                    text,
                    font.FontFamily,
                    (int)font.Style,
                    font.Size * 1.2f, // scale để nét đẹp hơn
                    centerPoint,
                    format
                );
            }

            return path;
        }
        public static void SaveImageSourceToFile(ImageSource image, string filePath)
        {
            if (image is BitmapSource bitmapSource)
            {
                BitmapEncoder encoder;

                // chọn encoder theo đuôi file
                string ext = Path.GetExtension(filePath).ToLower();
                switch (ext)
                {
                    case ".jpg":
                    case ".jpeg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    case ".gif":
                        encoder = new GifBitmapEncoder();
                        break;
                    case ".tif":
                    case ".tiff":
                        encoder = new TiffBitmapEncoder();
                        break;
                    default:
                        encoder = new PngBitmapEncoder(); // mặc định PNG
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(stream);
                }
            }
            else
            {
                throw new InvalidOperationException("ImageSource không phải BitmapSource.");
            }
        }

        public static BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            BitmapSource thumbnail;
            try
            {
                thumbnail = Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                return thumbnail;
            }
            finally
            {
                //DeleteObject(hBitmap); // tránh leak memory
            }
        }
        //public static extern bool DeleteObject(IntPtr hObject);

        private ObservableCollection<string> _content = new ObservableCollection<string>();
        public ObservableCollection<string> Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged(); // nếu BaseViewModel implement INotifyPropertyChanged
            }
        }
        private string _selectedLine;
        public string SelectedLine
        {
            get => _selectedLine;
            set
            {
                _selectedLine = value;
                OnPropertyChanged();

                if (!string.IsNullOrEmpty(_selectedLine))
                {
                    // Ví dụ: Xử lý khi chọn dòng mới
                    Debug.WriteLine($"Bạn vừa chọn: {_selectedLine}");

                    DrawText(_selectedLine);
                }
            }
        }
        void SetModel()
        {
            Content = new ObservableCollection<string>();
            for (int i = 0; i < 5; i++)
            {
                Content.Add("Default text " + i.ToString());
            }


            if (MyStyleFonts == null)
            {
                MyStyleFonts = new MyStyleFonts();
                MyStyleFonts.SizeFont = "55";
                MyStyleFonts.PointY = "1155";
            }
            if (VideosInfo == null)
            {
                VideosInfo = new ObservableCollection<VideosInfo>();
            }
            if (AudioInfo == null)
            {
                AudioInfo = new ObservableCollection<AudioInfo>();
            }
            if (SettingUI == null)
            {
                SettingUI = new SettingUI();
                SettingUI.VolumnAudio = "10";
                SettingUI.IsHflip = true;
                SettingUI.Audios = new ObservableCollection<string>();
                SettingUI.Audio = new ObservableCollection<string>();
                Uri uri = new Uri(appPath + "tiktok interface design.png");
                System.Windows.Media.Imaging.BitmapImage bitmap = new System.Windows.Media.Imaging.BitmapImage(uri);
                SettingUI.Image = bitmap;

                // khởi tạo command
                SelectedItemChangedCommand = new RelayCommand<string>((p) => { return true; }, OnSelectedItemChanged);
            }
            if (Combinations == null)
            {
                Combinations = new List<List<string>>();
            }
            if (ListCombinations == null)
            {
                ListCombinations = new List<string>();
            }
            SettingData.Duration = "0";
            SettingUI.MaxDuration = 10;
            SettingUI.MaxQuantity = 1;

            SelectedLine = Content.FirstOrDefault();
        }
        void ChoosenAudio()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Audio files|*.mp3;*.m4a;*.wav",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                if (AudioInfo != null)
                    AudioInfo.Clear();
                SettingUI.Audios.Clear();
                SettingUI.Audio.Clear();
                foreach (var file in dialog.FileNames)
                {
                    string path = System.IO.Path.GetDirectoryName(file);
                    SettingUI.Audios.Add(file);
                    SettingUI.Audio.Add(file.Replace(path + "\\", ""));
                }
            }
            ResetStatus();
        }
        void SetUI()
        {
            if (SettingUI.Audios.Count > 0)
            {
                //string path = System.IO.Path.GetDirectoryName(Audio);
                //SettingUI.Audio = Audio.Replace(path + "\\", "");
            }
        }
        private void ChoosenVideos()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Media files|*.mp4;*.avi;*.mov;*.wmv;*.mkv;*.jpg;*.png;*.jpeg",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                if (VideosInfo != null)
                    VideosInfo.Clear();
                foreach (var file in dialog.FileNames)
                {
                    var videoInfo = LoadMediaInfo(file);
                    int d = (int)double.Parse(videoInfo.DurationTime);
                    if (d > 5)
                    {
                        VideosInfo.Add(LoadMediaInfo(file));
                    }
                    if (file.ToLower().Contains(".jpg") || file.ToLower().Contains(".jpeg") || file.ToLower().Contains(".png"))
                    {
                        VideosInfo.Add(LoadMediaInfo(file));
                    }
                }
                if (VideosInfo.Count < 2)
                {
                    string strFilePath = VideosInfo[0].FilePath.ToLower();
                    if (strFilePath.Contains(".jpg") || strFilePath.Contains(".jpeg") || strFilePath.Contains(".png"))
                    {

                    }
                    else
                    {
                        VideosInfo.Clear();
                        MessageBox.Show("Phải chọn ít nhất từ 2 clip trở lên!");
                    }
                    goto END;
                }
                int AllDuration = 0;
                foreach (var file in VideosInfo)
                {
                    var match = Regex.Match(file.DurationText, @"\d+");
                    if (match.Success)
                    {
                        int seconds = int.Parse(match.Value);  // seconds = 6
                        AllDuration += seconds;
                    }
                }
                SettingUI.MaxQuantity = VideosInfo.Count;
                SettingUI.MaxDuration = VideosInfo.Count * 5;
                SettingUI.AllDuration = "Tổng thời gian của " + VideosInfo.Count + " video: " + AllDuration + "s";
                StartAll();
            }
        END:
            ResetStatus();
            string end = "";
        }
        private VideosInfo LoadVideoInfo(string filePath)
        {
            var shell = ShellFile.FromFilePath(filePath);
            var duration = shell.Properties.System.Media.Duration.Value;

            var thumbnail = Imaging.CreateBitmapSourceFromHBitmap(
                shell.Thumbnail.ExtraLargeBitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return new VideosInfo
            {
                FilePath = filePath,
                DurationText = duration.HasValue
                    ? $"Duration: {TimeSpan.FromTicks((long)duration.Value).TotalSeconds:N0} s"
                    : "Unknown",
                DurationTime = TimeSpan.FromTicks((long)duration.Value).TotalSeconds.ToString(),
                Thumbnail = thumbnail
            };
        }

        private VideosInfo LoadMediaInfo(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            BitmapSource thumbnail;
            string durationText = "";
            string durationTime = "0";

            if (extension == ".mp4" || extension == ".avi" || extension == ".mov" || extension == ".wmv" || extension == ".mkv")
            {
                var shell = ShellFile.FromFilePath(filePath);
                var duration = shell.Properties.System.Media.Duration.Value;

                if (duration.HasValue)
                {
                    durationText = $"Duration: {TimeSpan.FromTicks((long)duration.Value).TotalSeconds:N0} s";
                    durationTime = TimeSpan.FromTicks((long)duration.Value).TotalSeconds.ToString();
                }

                thumbnail = Imaging.CreateBitmapSourceFromHBitmap(
                    shell.Thumbnail.ExtraLargeBitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            else if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp" || extension == ".gif")
            {
                thumbnail = new BitmapImage(new Uri(filePath));
            }
            else
            {
                throw new NotSupportedException("Unsupported file format.");
            }

            return new VideosInfo
            {
                FilePath = filePath,
                DurationText = durationText,
                DurationTime = durationTime,
                Thumbnail = thumbnail
            };
        }
        private AudioInfo LoadAudioInfo(string filePath)
        {
            var audioFile = TagLib.File.Create(filePath);
            var duration = audioFile.Properties.Duration;

            return new AudioInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                DurationText = double.Parse(duration.TotalSeconds.ToString()).ToString()
            };
        }
        void StartAll()
        {
            StartTask(() =>
            {
                GenerateCombination();
                //StartGenVideo();
                //GenAudio();
            }, null, null);
        }
        //void GenAudio() 
        //{
        //    // Thiết lập biến môi trường đến credentials.json
        //    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "credentials.json");

        //    var client = TextToSpeechClient.Create();

        //    var input = new SynthesisInput
        //    {
        //        Text = "Xin chào, tôi là trợ lý AI nói tiếng Việt."
        //    };

        //    var voice = new VoiceSelectionParams
        //    {
        //        LanguageCode = "vi-VN",
        //        Name = "vi-VN-Wavenet-B", // Giọng nữ tiếng Việt tự nhiên
        //        SsmlGender = SsmlVoiceGender.Neutral
        //    };

        //    var config = new AudioConfig
        //    {
        //        AudioEncoding = AudioEncoding.Mp3
        //    };

        //    var response = client.SynthesizeSpeech(input, voice, config);

        //    File.WriteAllBytes("output2.mp3", response.AudioContent.ToByteArray());

        //    Console.WriteLine("Đã tạo file giọng nói tiếng Việt: output.mp3");
        //    SettingUI.Audio = appPath + "output2.mp3";
        //}
        void GenerateCombine()
        {

            if (SettingData.Duration == "5")
            {
                SettingData.Duration = "0";
            }
            GenerateCombination();
        }
        void GenerateVideo()
        {
            StartTask(() =>
            {
                StartGenVideo();
            }, null, null);
        }
        void GenerateCombination()
        {
            int combination = 0;
            SettingUI.Duration = 6;//Rút ngắn độ dài của mỗi video trước khi xào
            int lenghtVideo = SettingUI.Duration;
            Combinations.Clear();
            ListCombinations.Clear();
            foreach (var (index, file) in VideosInfo.Select((value, idx) => (idx, value)))
            {
                int timeThan5s = 0;
                string strDurationTime = file.DurationTime.ToString();
                //string path = System.IO.Path.GetDirectoryName(file.FilePath);
                string fileName = file.FilePath;
                double d = double.Parse(strDurationTime);
                int durationTime = (int)d;
                var clips = GetValidClips(fileName, durationTime, lenghtVideo);
                if (SettingUI.Duration <= durationTime)
                {
                    Combinations.Add(clips);
                }
            }
            CALCCombination(lenghtVideo);
        }
        void CALCCombination(int lenghtVideo)
        {
            GenCombineVideo();

            SettingUI.SumCombination = "Có thể xào ra " + Combinations.Count + " clip!";
            int countDuration = 0;
            if (SettingData.Duration != "0")
            {
                countDuration = int.Parse(SettingData.Duration);
                SettingUI.SumDuration = "Tổng thời lượng của clip sau khi xào là " + countDuration + " giây!";
            }
            else
            {
                countDuration = Combinations.Count() * (lenghtVideo - 1) - 1;
                SettingUI.SumDuration = "Tổng thời lượng của clip sau khi xào là " + countDuration + " giây!";
            }
        }
        List<string> GetValidClips(string fileName, int videoLength, int minDuration)
        {
            var result = new List<string>();
            if (SettingUI.IsHflip)
            {
                for (int start = 0; start <= videoLength - minDuration; start++)
                {
                    for (int end = start + minDuration; end <= videoLength; end++)
                    {
                        if (end - start == 6)
                        {
                            result.Add(fileName + "†" + start + "†" + end);
                        }
                    }
                }
            }
            else
            {
                result.Add(fileName + "†0†" + videoLength);
            }
            return result;
        }

        void StopAll()
        {
            StartTask(() =>
            {
                foreach (var item in Profiles)
                {
                    //StopProfile(item);
                }
            }, null, null);
        }
        void GenCombineVideo()
        {
            try
            {
                var results = new List<List<string>>();
                Random rnd = new Random();
                ListCombinations.Clear();
                var keys = Combinations.ToList();
                string temp = "";
                int duration = 0;
                if (SettingData.Duration != "0")
                {
                    duration = int.Parse(SettingData.Duration) / 5;
                }
                for (int i = 0; i < Combinations.Count(); i++)
                {
                    if (SettingData.Quantity != "0")
                    {
                        if (i == int.Parse(SettingData.Quantity))
                        {
                            i = Combinations.Count() + 1;
                            break;
                        }
                    }
                    temp = "" + Combinations[i][rnd.Next(0, Combinations[i].Count())] + "|";
                _GETGENCOMBINEVIDEOAGAIN:
                    string result1 = "";
                    foreach (var (index, combination) in keys.Select((value, idx) => (idx, value)))
                    {
                        if (index == 0)
                        {
                            continue;
                        }
                        if (SettingData.Duration != "0")
                        {
                            if (index > duration - 1)
                            {
                                continue;
                            }
                        }
                        var values = keys[index];
                        var randomValue = values[rnd.Next(values.Count)];
                        result1 += $"{randomValue}|";
                    }
                    result1 = temp + result1.TrimEnd('|');
                    foreach (var value in ListCombinations)//check có cái nào trùng nhau k
                    {
                        var tempValue = value.Split('|');
                        var tempResult1 = result1.Split('|');
                        if (tempValue[0] == tempResult1[0])
                        {
                            keys = keys.OrderBy(x => rnd.Next()).ToList();
                            goto _GETGENCOMBINEVIDEOAGAIN;
                        }
                        if (result1 == value)
                        {
                            keys = keys.OrderBy(x => rnd.Next()).ToList();
                            goto _GETGENCOMBINEVIDEOAGAIN;
                        }
                    }
                    ListCombinations.Add("" + result1);
                }
            }
            catch (Exception ex)
            {
            }
        }
        void ResetStatus()
        {
            if (VideosInfo.Count() < 2)
            {
                SettingUI.Complete = "";
                SettingUI.PercentComplete = 0;
            }
            else
            {
                SettingUI.Complete = "0/" + VideosInfo.Count();
                SettingUI.PercentComplete = 0;
            }

        }
        void StartGenVideo()
        {
            DateTime today = DateTime.Today;

            DateTime targetDate;
            if (DateTime.TryParseExact(tempExpDate2, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out targetDate))
            {
                TimeSpan remaining = targetDate - today;
                if (remaining.Days > 0)
                {
                    Random rnd = new Random();
                    DateTime dateTime = new DateTime();
                    DateTime now = DateTime.Now;
                    string Audio = "";

                    int tempEnd = int.Parse(Regex.Match(SettingUI.SumDuration, @"\d+").Value);
                    string transition = "fade|fadeblack|fadewhite|distance|wipeleft|wiperight|wipeup|wipedown|slideleft|slideright|slideup|slidedown|smoothleft|smoothright|smoothup|smoothdown|circlecrop|rectcrop|circleclose|circleopen|horzclose|horzopen|vertclose|vertopen|diagbl|diagbr|diagtl|diagtr|hlslice|hrslice|vuslice|vdslice|dissolve|pixelize|radial|hblur|wipetl|wipetr|wipebl|wipebr|fadegrays|squeezev|squeezeh|zoomin|hlwind|hrwind|vuwind|vdwind|coverleft|coverright|coverup|coverdown|revealleft|revealright|revealup|revealdown";
                    var listTrasition = transition.Split('|');
                    string[] strTitle = new string[0];

                    if (SettingUI.Title != null)
                    {
                        strTitle = SettingUI.Title.Split(',');
                    }
                    int hflip = 0;
                    double volumnAudio = double.Parse(SettingUI.VolumnAudio) / 10;

                    SettingUI.Complete = "0/" + ListCombinations.Count();
                    foreach (var (index, combination) in ListCombinations.Select((value, idx) => (idx, value)))
                    {
                        string joinArgs = " ";
                        int sumDuration = 0;
                        string videoStr = "";
                        string audioStr = "";
                        string[] temp1 = combination.Split('|');
                        if (SettingUI.Audios.Count > 0)
                        {
                            Audio = SettingUI.Audios[rnd.Next(0, SettingUI.Audios.Count())];
                        }

                        foreach (var item in temp1)
                        {
                            joinArgs += "-i \"" + item.Split('†')[0] + "\" ";
                        }
                        if (SettingUI.Audios.Count > 0)
                        {
                            joinArgs += $" -i \"{Audio}\"";
                        }
                        joinArgs += " -filter_complex \"";
                        int i = 0;

                        foreach (var item in temp1)
                        {
                            if (SettingUI.IsHflip)
                            {
                                int tempHflip = rnd.Next(0, 999999);
                                if (tempHflip % 2 == 0)
                                {
                                    hflip = 0;
                                }
                                else
                                {
                                    hflip = 1;
                                }
                            }


                            string scaleClip = ScaleInfo[rnd.Next(0, ScaleInfo.Length)];

                            string fileName = item.Split('†')[0];
                            int startClip = int.Parse(item.Split('†')[1]);
                            int endClip = int.Parse(item.Split('†')[2]);

                            joinArgs += $"[{i}:v]trim=start={startClip}:end={endClip},scale={scaleClip}:force_original_aspect_ratio=decrease,crop=720:1280,fps=30,pad=720:1280:(ow-iw)/2:(oh-ih)/2";

                            if (hflip == 1)
                            {
                                joinArgs += $",hflip[v{i}];";
                            }
                            else
                            {
                                joinArgs += $"[v{i}];";
                            }
                            joinArgs += $"[{i}:a]atrim=start={startClip}:end={endClip},volume={volumnAudio}[a{i}];";
                            sumDuration += endClip - startClip;
                            if (i + 1 < temp1.Count())
                            {
                                if (i == 0)
                                {
                                    joinArgs += $"[v{i}][v{i + 1}]xfade=transition={listTrasition[rnd.Next(0, listTrasition.Length)]}:duration=1:offset={sumDuration - 1},format=yuv420p[xfade{i}];";
                                    joinArgs += $"[a{i}][a{i + 1}]acrossfade=d=1[afade{i}];";
                                }

                                if (i > 0)
                                {
                                    joinArgs += $"[xfade{i - 1}][v{i + 1}]xfade=transition={listTrasition[rnd.Next(0, listTrasition.Length)]}:duration=1:offset={sumDuration - (i + 1)},format=yuv420p[xfade{i}];";
                                    joinArgs += $"[afade{i - 1}][a{i + 1}]acrossfade=d=1[afade{i}];";
                                }
                            }
                            else
                            {
                                if (!joinArgs.Contains("[0:v]trim=start=0"))
                                {
                                    int start = int.Parse(Regex.Match(joinArgs, @"(?<=start=)\d+").Value);
                                    joinArgs += $"[xfade{i - 1}]trim=start={start},fps=30,setpts=PTS-STARTPTS,settb=AVTB";
                                    if (SettingUI.Title != null)
                                    {
                                        int yTitle = 0;
                                        string pathFont = Regex.Replace(appPath, @"^[A-Za-z]:", "").Replace('\\', '/');
                                        foreach (var title in strTitle)
                                        {
                                            joinArgs += $",drawtext=fontfile='{pathFont}Font/Montserrat-ExtraBold.ttf':text='{title}':fontcolor=red:fontsize=56:bordercolor=white:borderw=5:x=((w-text_w)/2):y=((h-text_h)/2+{yTitle}):enable='between(t,0,5)':alpha='if(lt(t\\,3), 1, max(0\\,1-(t-3)))'";
                                            yTitle += 66;
                                        }
                                    }
                                    joinArgs += $"[outv]";
                                }
                                else
                                {
                                    joinArgs += $"[xfade{i - 1}]trim=start=0,fps=30,setpts=PTS-STARTPTS,settb=AVTB";
                                    if (SettingUI.Title != null)
                                    {
                                        int yTitle = 0;
                                        string pathFont = Regex.Replace(appPath, @"^[A-Za-z]:", "").Replace('\\', '/');
                                        foreach (var title in strTitle)
                                        {
                                            joinArgs += $",drawtext=fontfile='{pathFont}Font/Montserrat-ExtraBold.ttf':text='{title}':fontcolor=red:fontsize=56:bordercolor=white:borderw=5:x=((w-text_w)/2):y=((h-text_h)/2+{yTitle}):enable='between(t,0,5)':alpha='if(lt(t\\,3), 1, max(0\\,1-(t-3)))'";
                                            yTitle += 66;
                                        }
                                    }
                                    joinArgs += $"[outv]";
                                }
                                if (SettingUI.Audios.Count > 0)
                                    joinArgs += $";[{i + 1}:a]atrim=start=0:end={sumDuration}[a{i + 1}];[afade{i - 1}][a{i + 1}]amix=inputs=2:duration=shortest[outa]";
                            }
                            i++;
                        }

                        //joinArgs += $"\" -map \"[outv]\" -map \"[afade{i-2}]\" -c:v libx264 -crf 23 -preset veryfast -t {sumDuration - 2} video_{now.Hour}_{now.Minute}_{index}.mp4";
                        joinArgs += $"\" -map \"[outv]\" -map \"";
                        if (SettingUI.Audios.Count > 0)
                        {
                            joinArgs += $"[outa]\"";
                        }
                        else
                        {
                            joinArgs += $"[afade{i - 2}]\"";
                        }

                        joinArgs += $" -c:v ";
                        joinArgs += $"libx264 -crf 23 -preset veryfast";
                        joinArgs += $" \"{appPath}Output\\video_{tempEnd}s_{now.Hour}h_{now.Minute}m_{now.Second}s_{index}.mp4\"";

                        SettingUI.Complete = "Đang hoạt động " + index + "/" + ListCombinations.Count();
                        RunFFmpeg(joinArgs);
                        SettingUI.Complete = index + 1 + "/" + ListCombinations.Count();
                        int countClip = ListCombinations.Count();
                        int complete = index + 1;
                        int percentComplete = (int)((double)complete / countClip * 100);
                        SettingUI.PercentComplete = percentComplete;
                    }
                }
                else
                {
                    SettingUI.ExpDate = "Key đã hết hạn!";
                    try
                    {
                        // Danh sách các tiến trình cần kill
                        string[] processNames = { "GenVideo" };

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
        public string[] ScaleInfo = new string[] { "734:1306", "756:1344", "770:1370", "785:1395", "799:1421", "814:1446", "828:1472", "842:1498", "857:1523", "871:1549", "886:1574", "900:1600", "914:1626" };
        void RunFFmpeg(string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe", // đảm bảo ffmpeg đã có trong PATH
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };


            process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }



        void LoadSavedData()
        {
            try
            {
                var text = System.IO.File.ReadAllText("Saved.txt");
                SettingData = JsonConvert.DeserializeObject<SettingData>(text);

            }
            catch
            {

            }

            if (SettingData == null)
            {
                SettingData = new SettingData();
            }
        }

        public void SaveData()
        {
            try
            {
                SettingData.Quantity = "0";
                System.IO.File.WriteAllText("Saved.txt", JsonConvert.SerializeObject(SettingData));
            }
            catch { }
        }
        public string tempExpDate2 = string.Empty;
        public void GetExpDate(string expDate, string ExpDate)
        {
            try
            {
                tempExpDate2 = ExpDate;
                SettingUI.ExpDate = "Số ngày sử dụng phần mềm còn lại " + expDate + " ngày";
            }
            catch { }
        }


        #endregion
    }
}