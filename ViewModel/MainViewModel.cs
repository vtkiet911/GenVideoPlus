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
        
        public MainViewModel()
        {
            FirstLoad();
            LoadCommand();
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
        void SetModel()
        {
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
            if(SettingUI.Audios.Count > 0)
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
                if(VideosInfo.Count < 2)
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
            foreach (var (index, file) in VideosInfo.Select((value, idx)=>(idx,value)))
            {
                int timeThan5s = 0;
                string strDurationTime = file.DurationTime.ToString();
                //string path = System.IO.Path.GetDirectoryName(file.FilePath);
                string fileName = file.FilePath;
                double d = double.Parse(strDurationTime);
                int durationTime = (int)d;
                var clips = GetValidClips(fileName, durationTime, lenghtVideo);
                if(SettingUI.Duration <= durationTime)
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
                countDuration = Combinations.Count() * (lenghtVideo-1) - 1;
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
                        if(end - start == 6)
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
                if(SettingData.Duration != "0")
                {
                    duration = int.Parse(SettingData.Duration) / 5;
                }
                for (int i = 0; i < Combinations.Count(); i++)
                {
                    if(SettingData.Quantity != "0")
                    {
                        if(i == int.Parse(SettingData.Quantity))
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
            if(VideosInfo.Count() < 2)
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
                var text = File.ReadAllText("Saved.txt");
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
                File.WriteAllText("Saved.txt", JsonConvert.SerializeObject(SettingData));
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