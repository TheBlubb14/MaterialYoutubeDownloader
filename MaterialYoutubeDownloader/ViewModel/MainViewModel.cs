using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using YoutubeDown.Library;
using YoutubeDown.Library.Download.EventArg;

namespace MaterialYoutubeDownloader.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public bool IsMP3Downloading { get; set; }

        public bool IsVideoDownloading { get; set; }

        public int MP3Percentage { get; set; }

        public int VideoPercentage { get; set; }

        public ICommand MP3Command { get; set; }

        public ICommand VideoCommand { get; set; }

        public bool VideoButtonEnabled { get; set; }

        public bool MP3ButtonEnabled { get; set; }

        public string VideoUrl { get; set; }

        private YoutubeClient _YoutubeClient;
        private Progress<double> _MP3Progress;
        private Progress<double> _VideoProgress;

        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // canecaltion token / abbruche
                // download info
                // download pfad
                // video mp3 getrennt
            }
            else
            {
                MP3Command = new RelayCommand(MP3);
                VideoCommand = new RelayCommand(Video);

                MP3ButtonEnabled = true;
                VideoButtonEnabled = true;

                _YoutubeClient = new YoutubeClient(Loadffmpeg(), GetDownloadsPath() ?? Environment.CurrentDirectory, true);
                _MP3Progress = new Progress<double>(x =>
                {
                    MP3Percentage = Convert.ToInt32(x);
                });

                _VideoProgress = new Progress<double>(x =>
                {
                    VideoPercentage = Convert.ToInt32(x);
                });
            }
        }

        private async void MP3()
        {
            try
            {
                IsMP3Downloading = true;
                MP3ButtonEnabled = false;
                VideoButtonEnabled = false;

                await _YoutubeClient.DownloadAsMP3(VideoUrl, _MP3Progress, AudioDownloadInfo, null, null, CancellationToken.None);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Fehler beim Verbindungsaufbau" + Environment.NewLine + ex.Message +
                    (ex.InnerException == null ? Environment.NewLine + ex.InnerException.Message : ""));
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Ungültiger Parameter");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                IsMP3Downloading = false;
                MP3ButtonEnabled = true;
                VideoButtonEnabled = true;
                MP3Percentage = 0;
            }
        }

        private void AudioDownloadInfo(object sender, AudioDownloadInfoArgs e)
        {

        }

        private async void Video()
        {
            try
            {
                IsVideoDownloading = true;
                MP3ButtonEnabled = false;
                VideoButtonEnabled = false;

                await _YoutubeClient.DownloadHighestVideo(VideoUrl, _VideoProgress, VideoDownloadInfo, null, null, CancellationToken.None);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Fehler beim Verbindungsaufbau" + Environment.NewLine + ex.Message +
                    (ex.InnerException == null ? Environment.NewLine + ex.InnerException.Message : ""));
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Ungültiger Parameter");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                MP3ButtonEnabled = true;
                VideoButtonEnabled = true;
                IsVideoDownloading = false;
                VideoPercentage = 0;
            }
        }

        private void VideoDownloadInfo(object sender, VideoDownloadInfoArgs e)
        {

        }

        public string Loadffmpeg()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "ffmpeg.exe");

            if (!File.Exists(path))
                File.WriteAllBytes(path, Properties.Resources.ffmpeg);

            return path;
        }

        public static string GetDownloadsPath()
        {
            if (Environment.OSVersion.Version.Major < 6)
                return null;

            IntPtr pathPtr = IntPtr.Zero;
            try
            {
                SHGetKnownFolderPath(ref FolderDownloads, 0, IntPtr.Zero, out pathPtr);
                return Marshal.PtrToStringUni(pathPtr);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pathPtr);
            }
        }

        private static Guid FolderDownloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);
    }
}