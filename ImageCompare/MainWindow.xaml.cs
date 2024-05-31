using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace ImageCompare
{
    public partial class MainWindow : Window
    {
        private string folderPath = string.Empty;
        private string image1Path = string.Empty;
        private string image2Path = string.Empty;
        private CancellationTokenSource cancellationTokenSource;
        private Process pythonProcess;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                image1Path = openFileDialog.FileName;
                Image1.Source = new BitmapImage(new Uri(image1Path));
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                image2Path = openFileDialog.FileName;
                Image2.Source = new BitmapImage(new Uri(image2Path));
            }
        }

        private void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            double similarity = CompareImages();

            PercentageLabel.Content = $"{similarity:F2}%";

            if (similarity == 100.00)
            {
                LightIndicator.Fill = System.Windows.Media.Brushes.Red;
            }
            else if (similarity >= 99.99)
            {
                LightIndicator.Fill = System.Windows.Media.Brushes.Yellow;
            }
            else
            {
                LightIndicator.Fill = System.Windows.Media.Brushes.Green;
            }
        }

        private double CompareImages()
        {
            if (string.IsNullOrEmpty(image1Path) || string.IsNullOrEmpty(image2Path))
                return 0;

            using (Bitmap bmp1 = new Bitmap(image1Path))
            using (Bitmap bmp2 = new Bitmap(image2Path))
            {
                return GetSimilarity(bmp1, bmp2);
            }
        }

        private double GetSimilarity(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height)
                return 0;

            int totalPixels = bmp1.Width * bmp1.Height;
            int matchingPixels = 0;

            for (int y = 0; y < bmp1.Height; y++)
            {
                for (int x = 0; x < bmp1.Width; x++)
                {
                    System.Drawing.Color color1 = bmp1.GetPixel(x, y);
                    System.Drawing.Color color2 = bmp2.GetPixel(x, y);

                    if (ColorsAreSimilar(color1, color2))
                    {
                        matchingPixels++;
                    }
                }
            }

            return (double)matchingPixels / totalPixels * 100;
        }

        private bool ColorsAreSimilar(System.Drawing.Color color1, System.Drawing.Color color2, int tolerance = 5)
        {
            int diffR = Math.Abs(color1.R - color2.R);
            int diffG = Math.Abs(color1.G - color2.G);
            int diffB = Math.Abs(color1.B - color2.B);

            return diffR <= tolerance && diffG <= tolerance && diffB <= tolerance;
        }

        private void FolderSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            openFolderDialog.ShowDialog();
            folderPath = openFolderDialog.FolderName;
            FolderPathLabel.Content = folderPath;
            StartButton.IsEnabled = true;        
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(folderPath))
                return;

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            CompareButton.IsEnabled = false;

            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            Task.Run(() => ProcessImages(token), token);

            // This isn't working properly, so I commented it out
            // Run the Python script seperately in another IDE like VS Code, IDLE, or PyCharm
            //Task.Run(() => RunPythonScriptPeriodically(token), token);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            CompareButton.IsEnabled = true;
        }

        private async Task ProcessImages(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var files = Directory.GetFiles(folderPath, "*.png");

                if (files.Length > 0)
                {
                    // Sort files
                    Array.Sort(files, (x, y) => File.GetCreationTime(y).CompareTo(File.GetCreationTime(x)));
                    string latestImagePath = files[0];

                    //if (latestImagePath != image2Path)
                    {
                        // Wait until the file is available                        
                        while (!IsFileAvailable(latestImagePath) && (latestImagePath != image2Path))
                        {
                            await Task.Delay(100); // Wait for 100 milliseconds before retrying
                        }                        

                        if (!string.IsNullOrEmpty(image2Path))
                        {
                            image1Path = image2Path;

                            //Application.Current.Dispatcher.Invoke(() =>
                            //{
                            //    Image1.Source = new BitmapImage(new Uri(image1Path));
                            //});

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Image1.Source = new BitmapImage(new Uri(image1Path));
                                //Image1.Source = LoadBitmapImage(image1Path);
                            });
                        }

                        image2Path = latestImagePath;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                           Image2.Source = new BitmapImage(new Uri(image2Path));
                           //Image2.Source = LoadBitmapImage(image2Path);
                        });

                        Console.WriteLine(DateTime.Now.ToString());
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CompareButton_Click(null, null);
                        });
                        Console.WriteLine(DateTime.Now.ToString());
                    }
                }

                await Task.Delay(2500, token);
            }
        }

        private BitmapImage LoadBitmapImage(string path)
        {
            BitmapImage bitmap = new BitmapImage();

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }

            bitmap.Freeze(); // Freeze to make it cross-thread accessible

            return bitmap;
        }

        private bool IsFileAvailable(string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return true;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }

        private async Task RunPythonScriptPeriodically(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Run the Python script
                RunPythonScript();

                // Wait for 30 seconds
                await Task.Delay(30000, token);

                if (token.IsCancellationRequested)
                    break;

                // Stop the Python script
                StopPythonScript();

                // Wait for another 30 seconds
                await Task.Delay(30000, token);
            }
        }

        private void RunPythonScript()
        {
            try
            {
                string pythonPath = @"C:\Users\bsove\AppData\Local\Programs\Python\Python311\python.exe"; // Python interpreter path
                string baseDirectory = @"D:\Sandbox\ImageCompare\ImageCompare";
                string scriptPath = Path.Combine(baseDirectory, "Python", "ScreenCapture.py");

                //string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Python", "ScreenCapture.py"); 

                ProcessStartInfo start = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = $"\"{scriptPath}\" \"{folderPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                pythonProcess = new Process
                {
                    StartInfo = start
                };

                pythonProcess.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                pythonProcess.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

                pythonProcess.Start();
                pythonProcess.BeginOutputReadLine();
                pythonProcess.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to start Python script: " + ex.Message);
            }
        }


        private void StopPythonScript()
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                pythonProcess.Kill();
                pythonProcess = null;
            }
        }
    }
}
