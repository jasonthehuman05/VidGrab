using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace VidGrab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        YoutubeDL ytdl;
        CancellationTokenSource VCanceller;
        CancellationTokenSource ACanceller;
        Progress<DownloadProgress> progress;

        public MainWindow()
        {
            InitializeComponent();
            FilePathBox.Text = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Downloads";
            ytdl = new YoutubeDL()
            {
                YoutubeDLPath = "binaries/yt-dlp.exe",
                FFmpegPath = "binaries/ffmpeg/ffmpeg.exe",
                OutputFolder = FilePathBox.Text
            };

            progress = new Progress<DownloadProgress>(p => ProgressBar.Value = p.Progress*100);
//            progress = new Progress<DownloadProgress>(p => Console.WriteLine(p.Progress));
            CancelAudioDownloadButton.Visibility = Visibility.Hidden;
            CancelVideoDownloadButton.Visibility = Visibility.Hidden;
            VideoInformationHolder.Visibility = Visibility.Hidden;  
        }

        private async void GetVideoInfoButton_Click(object sender, RoutedEventArgs e)
        {
            string URL = VideoURL.Text;
            RunResult<VideoData> vd = await ytdl.RunVideoDataFetch(URL);
            if (vd.Success)
            {
                VideoTitleLabel.Content = vd.Data.Title;
                CreatorLabel.Content = $"By: {vd.Data.Channel}";
                ThumbnailHolder.Source = new BitmapImage(new Uri(vd.Data.Thumbnail));

                VideoInformationHolder.Visibility = Visibility.Visible;
                //RunResult<string> res = await ytdl.RunVideoDownload(URL);
            }
            else
            {
                System.Windows.MessageBox.Show("Something went wrong. Ensure a valid URL was provided and you have a working internet connecion", "An error occured", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DownloadVideoButton_Click(object sender, RoutedEventArgs e)
        {
            CancelVideoDownloadButton.Visibility = Visibility.Visible;
            string URL = VideoURL.Text;
            VCanceller = new CancellationTokenSource();
            try
            {
                RunResult<string> video = await ytdl.RunVideoDownload(URL, ct: VCanceller.Token, progress: progress);
            }
            catch { }
            CancelVideoDownloadButton.Visibility = Visibility.Hidden;
        }

        private void CancelDownloadVideo(object sender, RoutedEventArgs e){ VCanceller.Cancel(); }

        private void CancelDownloadAudio(object sender, RoutedEventArgs e){ ACanceller.Cancel(); }

        private async void DownloadAudioButton_Click(object sender, RoutedEventArgs e)
        {
            CancelAudioDownloadButton.Visibility = Visibility.Visible;
            string URL = VideoURL.Text;
            ACanceller = new CancellationTokenSource();
            try
            {
                RunResult<string> video = await ytdl.RunAudioDownload(URL, format: AudioConversionFormat.Mp3, ct: ACanceller.Token, progress: progress);
            }
            catch { }
            CancelAudioDownloadButton.Visibility = Visibility.Hidden;
        }

        private void OpenDownloadDirButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(FilePathBox.Text);
        }

        private void LocateFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            if(fbd.SelectedPath != string.Empty)
            {
                FilePathBox.Text = fbd.SelectedPath;
            }
        }
    }
}
