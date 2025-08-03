using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
//нугеты
using TagLib;
// Псевдонимы
using SI = System.IO;

namespace WpfApp1
{
    public partial class Window1 : Window
    {
        public bool isPlaying = false;
        private double mediaDurationSeconds = 0;
        private bool isMediaLoaded = false;
        private DispatcherTimer progressTimer;
        private bool isDraggingProgressBar = false;
        public ObservableCollection<Track> Tracks { get; set; } = new ObservableCollection<Track>();
        public string ImageIcon { get; }
        public string AbsolutePath { get; }

        private Button add_ALL;
        private readonly string tempSongPath;
        private readonly string tempImgPath; 
        private readonly string defaultImagePath; 

        public Window1()
        {
            InitializeComponent();
            DataContext = this;

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            tempSongPath = SI.Path.Combine(basePath, "temp_song");
            tempImgPath = SI.Path.Combine(basePath, "temp_img");
            defaultImagePath = SI.Path.Combine(basePath, "Gakuseisean-Ivista-2-Files-Movie-File.256.png"); 

            // Установка изображения по умолчанию при запуске
            SetDefaultImage();

            progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            progressTimer.Tick += ProgressTimer_Tick;

            _ = LoadBackgroundAsync();
        }

        private async Task LoadBackgroundAsync()
        {
            await SetBackgroundVideo();
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            MediaPlayer.Position = TimeSpan.Zero;
            isPlaying = false;
        }

        private void ProgressBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingProgressBar && MediaPlayer.Source != null && MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                Point mousePosition = e.GetPosition(ProgressBar);
                double ratio = mousePosition.X / ProgressBar.ActualWidth;
                double newPositionInSeconds = ProgressBar.Maximum * ratio;
                newPositionInSeconds = Math.Min(newPositionInSeconds, ProgressBar.Maximum);
                newPositionInSeconds = Math.Max(newPositionInSeconds, 0);
                MediaPlayer.Position = TimeSpan.FromSeconds(newPositionInSeconds);
                ProgressBar.Value = newPositionInSeconds;
            }
        }
        private void ProgressBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDraggingProgressBar)
            {
                isDraggingProgressBar = false;
                if (wasPlayingBeforeDrag)
                {
                    MediaPlayer.Play(); // Возобновляем, если трек играл
                    isPlaying = true;
                }
                progressTimer.Start();
            }
        }

        private bool wasPlayingBeforeDrag = false;
        private void ProgressBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MediaPlayer.Source != null && MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                isDraggingProgressBar = true;
                wasPlayingBeforeDrag = isPlaying; // Запоминаем, играл ли трек
                if (isPlaying)
                {
                    MediaPlayer.Pause(); // Приостанавливаем воспроизведение
                    isPlaying = false;   // Обновляем флаг
                }
                progressTimer.Stop();

                Point mousePosition = e.GetPosition(ProgressBar);
                double ratio = mousePosition.X / ProgressBar.ActualWidth;
                double newPositionInSeconds = ProgressBar.Maximum * ratio;
                newPositionInSeconds = Math.Min(newPositionInSeconds, ProgressBar.Maximum);
                newPositionInSeconds = Math.Max(newPositionInSeconds, 0);
                MediaPlayer.Position = TimeSpan.FromSeconds(newPositionInSeconds);
                ProgressBar.Value = newPositionInSeconds;
            }
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (MediaPlayer.Source != null)
            {
                if (!isMediaLoaded && MediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    mediaDurationSeconds = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                    ProgressBar.Maximum = mediaDurationSeconds;
                    isMediaLoaded = true;

                    var totalTime = MediaPlayer.NaturalDuration.TimeSpan;
                    Duration.Text = $"{totalTime.Minutes}:{totalTime.Seconds:00}";
                }

                if (isMediaLoaded)
                {
                    double currentPosition = MediaPlayer.Position.TotalSeconds;
                    if (currentPosition > mediaDurationSeconds)
                    {
                        currentPosition = mediaDurationSeconds;
                        MediaPlayer.Position = TimeSpan.FromSeconds(mediaDurationSeconds);
                    }
                    ProgressBar.Value = currentPosition;
                    Duration.Text = $"{MediaPlayer.Position.Minutes}:{MediaPlayer.Position.Seconds:00}";
                }
            }
        }

        private async Task SetBackgroundVideo()
        {
            try
            {
                string relativePath = @"wallpaper\1003303.mp4";
                string absolutePath = SI.Path.GetFullPath(SI.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath));

                if (SI.File.Exists(absolutePath))
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        BackGroundPlayer.Source = new Uri(absolutePath);
                        BackGroundPlayer.Play();
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка {ex.Message}");
            }
        }

        private void BackGroundPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (BackGroundPlayer.Source != null)
            {
                BackGroundPlayer.Position = TimeSpan.Zero;
                BackGroundPlayer.Play();
            }
        }

        public class Track
        {
            public string Name { get; set; }
            public string Artist { get; set; }
            public string FilePath { get; set; }
            public byte[] ImageData { get; set; } // Изображение в памяти
            public string Duration { get; set; }
        }

        async Task ClearDirectory(string path)
        {
            if (!SI.Directory.Exists(path)) return;

         
                await Dispatcher.InvokeAsync(() =>
                {

                    SI.DirectoryInfo di = new SI.DirectoryInfo(path);
                    foreach (SI.FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (SI.DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                });

            }
        public bool CompareDuration(string filePath, string expectedDuration)
        {
            double ToleranceSeconds = 1.0;
            try
            {

                // Получаем длительность из файла
                var file = TagLib.File.Create(filePath);
                TimeSpan actualDuration = file.Properties.Duration;

                // Парсим ожидаемую длительность строго в формате "mm:ss"
                TimeSpan expected = TimeSpan.ParseExact(expectedDuration, @"mm\:ss", null);

                // Сравниваем с допуском
                double difference = Math.Abs((actualDuration - expected).TotalSeconds);
                return difference <= ToleranceSeconds;
            }
            catch
            {
                return false;
            }
        }

        public void Play(string fileName, string duration)
        {

            try
            {
                
                isMediaLoaded = false;
                mediaDurationSeconds = 0;
                ProgressBar.Value = 0;
                ProgressBar.Maximum = 1;
                Duration.Text = "00:00";

                if (!SI.File.Exists(fileName))
                {
                    MessageBox.Show($"Файл не найден: {fileName}");
                    return;
                }

                MediaPlayer.Stop();
                MediaPlayer.Source = new Uri(fileName);
                MediaPlayer.Play();
                progressTimer.Start();
              
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка воспроизведения: {ex.Message}\nПуть: {fileName}");
            }
        }

        private void ChangeImage(byte[] imageData)
        {
            try
            {
                if (imageData == null || imageData.Length == 0)
                {
                    SetDefaultImage();
                    return;
                }

                using (var memoryStream = new SI.MemoryStream(imageData))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = memoryStream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    
                    Dispatcher.Invoke(() => PlaySong.Source = bitmap);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения из памяти: {ex.Message}");
                SetDefaultImage();
            }
        }

        private void SetDefaultImage()
        {
            if (SI.File.Exists(defaultImagePath))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(defaultImagePath, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();
                Dispatcher.Invoke(() => PlaySong.Source = bitmap);
            }
            else
            {
                MessageBox.Show($"Изображение по умолчанию не найдено: {defaultImagePath}");
            }
        }

        private async void TracksListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            
            isPlaying = false;

            if (TracksListView.SelectedItem is Track selectedTrack)
            {
                string searchText = SearchBox.Text.Trim();

                string searchsolo = $"{selectedTrack.Name} {selectedTrack.Artist}";
                
                await PythonScript(1, searchText, selectedTrack.FilePath, selectedTrack.Name);
                ChangeImage(selectedTrack.ImageData);



                string path = (Path.GetFullPath(Path.Combine("temp_song", selectedTrack.FilePath)));

                if (!Directory.Exists(path))
                {

                    path = Path.ChangeExtension(path, "mp3");
                   

                    if (CompareDuration(path, selectedTrack.Duration))
                    {
                        Play(path, selectedTrack.Duration);   
                    }
                    else
                    {
                        MessageBox.Show("трек не загрузится, соси хуй");
                    }
                }
                    
            }
            else
            {
                
            }

            //снимает выеделение после клика, хзкак работает мне похуй(не ебу нахуя тут диспетчер)
            await Dispatcher.InvokeAsync((Action)(() =>
            {
                TracksListView.UnselectAll();
            }), DispatcherPriority.Background);


        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchBtn_Click(sender, e);
            }
        }
  
        private void PlayPauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlaying)
            {
                MediaPlayer.Pause();
                PlayPauseBtn.Content = "Play";
                isPlaying = true;
            }
            else
            {
                MediaPlayer.Play();
                PlayPauseBtn.Content = "Pause";
                isPlaying = false;
            }
        }

        private SemaphoreSlim _concurrencyLimiter = new SemaphoreSlim(2, 2); // макс 2 потока
        private CancellationTokenSource _previousCts; // хуйня отмены предыдущих потоков

        // arg = 1 скачиваются песни, 2 - скачиваются картинки(в них содержаться длительность, название, артист)

        public async Task PythonScript(int arg, string searchText1, string name, string name1, CancellationToken externalToken = default)
        {
            Debug.WriteLine($"Начало выполнения PythonScript: arg={arg}, searchText1={searchText1}, name={name}, name1={name1}");

            _previousCts?.Cancel();
            _previousCts = new CancellationTokenSource();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                externalToken,
                _previousCts.Token
            );
            var cancellationToken = linkedCts.Token;

            try
            {
                Debug.WriteLine("Ожидание свободного слота");
                await _concurrencyLimiter.WaitAsync(cancellationToken);

                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Untitled-3.py");
                string searchText = searchText1;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    Debug.WriteLine("Пустой или пробельный searchText");
                    MessageBox.Show("Пусто еп твою мать");
                    return;
                }

                string argument = string.Empty;
                string nameWithoutExtension = Path.GetFileNameWithoutExtension(name);

                if (arg == 1)
                {
                    argument = $"\"{scriptPath}\" \"{searchText}\" \"{arg}\" \"{nameWithoutExtension}\" 0";
                    Debug.WriteLine($"Сформирован аргумент для arg=1: {argument}");
                }
                else if (arg == 2)
                {
                    argument = $"\"{scriptPath}\" \"{searchText}\" {arg} 0 100";
                    Debug.WriteLine($"Сформирован аргумент для arg=2: {argument}");
                }

                using (var process = new Process())
                {
                    _pythonProcesses.Add(process);
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = argument,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    Debug.WriteLine($"Запуск Python процесса: {argument}");
                    process.Start();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Debug.WriteLine("Операция отменена до завершения процесса");
                        process.Kill();
                        throw new OperationCanceledException();
                    }

                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    Debug.WriteLine("Ожидание завершения процесса");
                    await process.WaitForExitAsync(cancellationToken);

                    string output = await outputTask;
                    string error = await errorTask;

                    Debug.WriteLine($"Python процесс завершен. Код выхода: {process.ExitCode}");
                    Debug.WriteLine($"Вывод Python: {output}");
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.WriteLine($"Ошибка Python: {error}");
                    }

                    Console.WriteLine($"Python output: {output}");
                    Console.WriteLine($"Python error: {error}");
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Выполнение PythonScript отменено");
                Console.WriteLine("Выполнение отменено.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка выполнения PythonScript: {ex.Message}");
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                Debug.WriteLine("Освобождение слота");
                _concurrencyLimiter.Release();
            }
        }

        private List<Process> _pythonProcesses = new List<Process>();
        private void KillAllPythonProcesses()
        {
            foreach (var process in _pythonProcesses.ToList())
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit(); // Даем время на завершение
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при завершении процесса: {ex.Message}");
                }
                finally
                {
                    process.Dispose();
                    _pythonProcesses.Remove(process);
                }
            }

            // Дополнительно убиваем все процессы python в системе
            foreach (var process in Process.GetProcessesByName("python"))
            {
                try { process.Kill(); }
                catch { /* Игнорируем ошибки */ }
            }
        }

        //спиздил у нейронки 
        private async Task UnloadImagesFromMemoryAsync()
        {
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    foreach (var track in Tracks)
                    {
                        track.ImageData = null; // Очищаем ImageData
                    }
                    Debug.WriteLine("ImageData очищен для всех треков");
                });

                // Принудительный вызов сборщика мусора
                await Task.Run(() =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в UnloadImagesFromMemoryAsync: {ex.Message}");
                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при очистке изображений: {ex.Message}");
                });
            }
        }

        private async void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
              
                KillAllPythonProcesses();

                // чистка епта
                PlaySong.Source = null;
                MediaPlayer.Stop();
                SetDefaultImage();

                await UnloadImagesFromMemoryAsync();
                await ClearDirectory(tempSongPath);
                await ClearDirectory(tempImgPath);


                string searchText = SearchBox.Text.Trim();
               
                await PythonScript(2, searchText, "emp", "emp");


                songView songView = new songView();

                // Получение списка изображений
                string[] imgFiles = Directory.GetFiles(tempImgPath, "*.jpg");
                Tracks.Clear();
                
                foreach (string imgFile in imgFiles)
                {
                    string fullImgPath = SI.Path.GetFullPath(imgFile);
                    byte[] imageData = SI.File.ReadAllBytes(fullImgPath);

                    string full = await Task.Run(() => songView.GetNamePart(fullImgPath, 4));

                    string name = await Task.Run(() => songView.GetNamePart(fullImgPath, 1));
                    string artist = await Task.Run(() => songView.GetNamePart(fullImgPath, 2));
                    string duration = await Task.Run(() => songView.GetNamePart(fullImgPath, 3));

                    Tracks.Add(new Track
                    {
                        Name = name,
                        Artist = artist,
                        FilePath = full,
                        ImageData = imageData,
                        Duration = duration
                    });

                }
               
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в ProcessTracks: {ex.ToString()}");
            }

        }


        class songView
        {
            public string SongName { get; set; }
            public string ArtistName { get; set; }
            public string Title { get; set; }

            public string GetNamePart(string filePath, int namePart)
            {
                try
                {
                    string fileName = Path.GetFileName(filePath);
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    string[] parts = fileNameWithoutExtension.Split('_');



                    switch (namePart)
                    {
                        case 1:
                            if (parts.Length >= 4)
                            {

                                return string.Join("_", parts.Take(parts.Length - 3));
                            }
                            return parts.Length > 0 ? parts[0] : string.Empty;

                        case 2:
                            if (parts.Length >= 4)
                            {
                                return parts[parts.Length - 3];
                            }
                            return parts.Length > 1 ? parts[1] : string.Empty;

                        case 3:
                            if (parts.Length >= 4)
                            {

                                string minutes = parts[parts.Length - 2];
                                string seconds = parts[parts.Length - 1];


                                return $"{minutes.PadLeft(2, '0')}:{seconds.PadLeft(2, '0')}";
                            }
                            else if (parts.Length >= 3)
                            {

                                return parts[parts.Length - 1];
                            }
                            return string.Empty;

                        case 4:
                            return fileName;

                        default:
                            Debug.WriteLine($"Неподдерживаемый namePart={namePart} в файле: {fileName}");
                            return string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка в GetNamePart для файла {filePath}: {ex.Message}");
                    return string.Empty;
                }
            }


        }



      


        private void add_Click(object sender, RoutedEventArgs e)
        {

         
        }

        private void addALL_Click(object sender, RoutedEventArgs e)
        {
            if (TracksListView.SelectedItem is Track selectedTrack)
            {

            }
        }


        private void addPlay_Click(object sender, RoutedEventArgs e)
        {
            if (TracksListView.SelectedItem is Track selectedTrack)
            {

            }
        }


        

        //две функции спизженные с stackoverflow 
        private void close_Click(object sender, RoutedEventArgs e)
        {
            for (int intCounter = App.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
            {
                App.Current.Windows[intCounter].Close();
            }
        }

        //доработал сам
        private void GridOfWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(WindowState == WindowState.Normal)
            {
                var move = sender as System.Windows.Controls.Grid;
                var win = Window.GetWindow(move);
                win.DragMove();
            }
            else
            {

                WindowState = WindowState.Normal;
                var move = sender as System.Windows.Controls.Grid;
                var win = Window.GetWindow(move);
                win.DragMove();

            }
        }

        private void Minimized_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }





    }




}