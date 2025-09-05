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
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
//нугеты
using TagLib;
using static System.Net.Mime.MediaTypeNames;

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

        //две колекции через один класс
        public ObservableCollection<Track> Tracks { get; set; } = new ObservableCollection<Track>();
        public ObservableCollection<Track> ALLTracks { get; set; } = new ObservableCollection<Track>();
        public ObservableCollection<PlayList> PlayLists { get; set; } = new ObservableCollection<PlayList>();
        public ObservableCollection<Track> PlayListsTrack { get; set; } = new ObservableCollection<Track>();

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
            AllTrackView();
            PlayListVIew();
            progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            progressTimer.Tick += ProgressTimer_Tick;

            _ = LoadBackgroundAsync();
        }


        private void AddPlay_Click(object sender, RoutedEventArgs e)
        {




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
                    MediaPlayer.Play(); // хуево тут все работает, но мне лень переделывать
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
                wasPlayingBeforeDrag = isPlaying; 
                if (isPlaying)
                {
                    MediaPlayer.Pause(); 
                    isPlaying = false;   
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
            //все светитя начало после перехода с .net framework на .net т.к. у них нет значения, ну и похуй, нахуя мне им нули присваивать
            public string Name { get; set; } 
            public string Artist { get; set; }
            public string FilePath { get; set; }
            public byte[] ImageData { get; set; } // Изображение в памяти ибо по иному хуево + нужны абсолютные пути, без них тоже поебота
            public string Duration { get; set; }
        }


        public class PlayList
        {
            public string Name { get; set; }
           

            public string Directory { get; set; }
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


        //спиздил и пох функция простая, она нужна что бы сранвить, файл уже полностью загрузился или нет
        public bool CompareDuration(string filePath, string expectedDuration)
        {
            double ToleranceSeconds = 1.0;
            try
            {

                var file = TagLib.File.Create(filePath);
                TimeSpan actualDuration = file.Properties.Duration;
                
                TimeSpan expected = TimeSpan.ParseExact(expectedDuration, @"mm\:ss", null);
                
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
                

                string filename = SI.Path.GetFileName(selectedTrack.FilePath);
                filename = SI.Path.GetFileNameWithoutExtension(filename);

                if (!SI.File.Exists(SI.Path.Combine("temp_song", $"{filename}.mp3"))) 
                {
                    await PythonScript(1, searchText, selectedTrack.FilePath, selectedTrack.Name);
                    ChangeImage(selectedTrack.ImageData);
                }




                string path = (SI.Path.GetFullPath(SI.Path.Combine("temp_song", selectedTrack.FilePath)));

                if (!Directory.Exists(path))
                {

                    path = SI.Path.ChangeExtension(path, "mp3");
                   

                    if (CompareDuration(path, selectedTrack.Duration))
                    {
                        Play(path, selectedTrack.Duration);   
                    }
                    else
                    {
                        MessageBox.Show("да падажди заебал!");
                    }
                }
                    
            }
            else
            {
                
            }

            //снимает выеделение после клика
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

                string scriptPath = SI.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Untitled-3.py");
                string searchText = searchText1;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    Debug.WriteLine("Пустой searchText");
                    MessageBox.Show("Пусто еп твою мать");
                    return;
                }

                string argument = string.Empty;
                string nameWithoutExtension = SI.Path.GetFileNameWithoutExtension(name);

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
                catch { //похуй на ошибки
                        }
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
                        track.ImageData = null; // очистка ImageData
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

        public async void AllTrackView()
        {
            

            try
            {
                Debug.WriteLine("функция запущена");
                //уебище запоVни что путь не начинается с слеша придурок
                string allImgPath = @"ALL\img";
                string allSongPath = @"ALL\song";

                
                Debug.WriteLine($" Полный путь : {allImgPath}");

                songView songView = new songView();
                string[] imgFiles = Directory.GetFiles(SI.Path.GetFullPath(allImgPath), "*.jpg");

                ALLTracks.Clear();

                foreach (string imgFile in imgFiles)
                {

                    Debug.WriteLine(imgFile);

                    string fullImgPath = SI.Path.GetFullPath(imgFile);
                    byte[] imageData = SI.File.ReadAllBytes(fullImgPath);


                    string full = await Task.Run(() => songView.GetNamePart(fullImgPath, 4));

                    string name = await Task.Run(() => songView.GetNamePart(fullImgPath, 1));
                    string artist = await Task.Run(() => songView.GetNamePart(fullImgPath, 2));
                    string duration = await Task.Run(() => songView.GetNamePart(fullImgPath, 3));

                    ALLTracks.Add(new Track
                    {
                        Name = name,
                        Artist = artist,
                        FilePath = full,
                        ImageData = imageData,
                        Duration = duration
                    });

                }
            }

            catch(Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
            }



        }

        public void PlayListVIew()
        {
            try
            {
                PlayLists.Clear();
                //спизженно частично с stackoverflow
                string[] folders = System.IO.Directory.GetDirectories(SI.Path.GetFullPath(SI.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"playlist")), "*", System.IO.SearchOption.AllDirectories);


                foreach (string f in System.IO.Directory.GetDirectories(SI.Path.GetFullPath(SI.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"playlist")), "*", System.IO.SearchOption.AllDirectories))
                {
                    Debug.WriteLine($"ПЛЕЙЛИСТ {f}");

                    var dirName = new DirectoryInfo(f).Name;

                    PlayLists.Add(new PlayList
                    {
                        Name = dirName,
                        Directory = f,
                        

                    });


                }





            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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
                    string fileName = SI.Path.GetFileName(filePath);
                    string fileNameWithoutExtension = SI.Path.GetFileNameWithoutExtension(fileName);
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

        private async void PlayListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                BackToPlaylist.Visibility = Visibility.Visible;
                PlayListView.Visibility = Visibility.Collapsed;
                TracksListView1.Visibility = Visibility.Visible;
                NewPlayList.Visibility = Visibility.Collapsed;

                Debug.WriteLine("Показался листвью");

                if (PlayListView.SelectedItem is PlayList selectedPlayList)
                {
                    Debug.WriteLine("Выделение плейлиста");


                    string path = selectedPlayList.Directory;





                    PlaySong.Source = null;
                    MediaPlayer.Stop();
                    SetDefaultImage();

                    await UnloadImagesFromMemoryAsync();



                    songView songView = new songView();

                    // Получение списка изображений
                    string[] imgFiles = Directory.GetFiles(selectedPlayList.Directory, "*.jpg");
                    PlayListsTrack.Clear();

                    foreach (string imgFile in imgFiles)
                    {
                        string fullImgPath = SI.Path.GetFullPath(imgFile);
                        byte[] imageData = SI.File.ReadAllBytes(fullImgPath);

                        string full = await Task.Run(() => songView.GetNamePart(fullImgPath, 4));

                        string name = await Task.Run(() => songView.GetNamePart(fullImgPath, 1));
                        string artist = await Task.Run(() => songView.GetNamePart(fullImgPath, 2));
                        string duration = await Task.Run(() => songView.GetNamePart(fullImgPath, 3));

                        if (name.Length > 30)
                        {
                            name = name.Substring(0, 30) + "...";
                        }


                        if (artist.Length > 30)
                        {
                            artist = artist.Substring(0, 30) + "...";
                        }


                        PlayListsTrack.Add(new Track
                        {
                            Name = name,
                            Artist = artist,
                            FilePath = full,
                            ImageData = imageData,
                            Duration = duration
                        });

                    }



                }


            }
            finally
            {
                await Dispatcher.InvokeAsync((Action)(() =>
                {
                    PlayListView.UnselectAll();
                }), DispatcherPriority.Background);
            }





        }

        private async void DelPlayList_Click(object sender, RoutedEventArgs e)
        {

            PlayListView.SelectionChanged -= PlayListView_SelectionChanged;

            try
            {

                

                var button = (Button)sender;
                var Playlist = (PlayList)button.DataContext;
                PlayListView.SelectedItem = Playlist;


                Debug.WriteLine("функция удаления трека запущена");


                if (PlayListView.SelectedItem is PlayList selectedPlayList)

                {
                    await ClearDirectory(selectedPlayList.Directory);
                    Debug.WriteLine("выделение выполнено");
                    Debug.WriteLine(selectedPlayList.Directory);
                    Directory.Delete(selectedPlayList.Directory);
                }

               



            }

            catch(Exception ex) 
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                PlayListVIew();
                PlayListView.SelectionChanged += PlayListView_SelectionChanged;
            }


        }


        



        private void Dell_Click(object sender, RoutedEventArgs e)
        {


            try
            {
                
                var button = (Button)sender;
                var track = (Track)button.DataContext;
                TracksListView2.SelectedItem = track;


                Debug.WriteLine("функция удаления трека запущена");


                if (TracksListView2.SelectedItem is Track selectedTrack)
                {

                    Debug.WriteLine("выделение выполнено");


                    string filename = SI.Path.GetFileNameWithoutExtension(SI.Path.GetFullPath(selectedTrack.FilePath));
                    
                    string imgPath = SI.Path.Combine(@"ALL\img" ,$"{filename}.jpg");
                    string SongPath = SI.Path.GetFullPath(SI.Path.Combine(@"ALL\song", $"{filename}.mp3"));
                    Debug.WriteLine($"изображение {imgPath}, песня {SongPath}");
                    Console.WriteLine($"изображение {imgPath}, песня {SongPath}");


                    SI.File.Delete(imgPath);
                    SI.File.Delete(SongPath);



                }
                    
            }





           
            catch(Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
            }
            finally
            {
               
                AllTrackView();
            }
            
        }


        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            myPopup.IsOpen = false;
        }




        //частично нейронка
        //вообще лучше потом сделать одну функцию для удаления/копирования по парметру и пути в функцию
        private async void addALL_Click(object sender, RoutedEventArgs e)
        {
            TracksListView.SelectionChanged -= TracksListView_SelectionChanged;

            var button = (Button)sender;
            var track = (Track)button.DataContext;
            TracksListView.SelectedItem = track;

            Debug.WriteLine("Кнопка 'Все треки' нажата");

            try 
            {
                if (TracksListView.SelectedItem is Track selectedTrack)
                {
                   
                    string searchText = SearchBox.Text.Trim();

                    string searchsolo = $"{selectedTrack.Name} {selectedTrack.Artist}";

                    await PythonScript(1, searchText, selectedTrack.FilePath, selectedTrack.Name);
                    ChangeImage(selectedTrack.ImageData);


                    string filename = SI.Path.GetFileNameWithoutExtension(selectedTrack.FilePath);

                 
                    string sourceImagePath = SI.Path.Combine("temp_img", $"{filename}.jpg");
                    string sourceAudioPath = SI.Path.Combine("temp_song", $"{filename}.mp3");

                   
                    string targetSongDir = @"ALL\song";
                    string targetImgDir = @"ALL\img";

                    
                    Directory.CreateDirectory(targetSongDir);
                    Directory.CreateDirectory(targetImgDir);

                    string targetImagePath = SI.Path.Combine(targetImgDir, $"{filename}.jpg");
                    string targetSongPath = SI.Path.Combine(targetSongDir, $"{filename}.mp3");

                    Debug.WriteLine($"изображение из: {sourceImagePath}");
                    Debug.WriteLine($"изображение в: {targetImagePath}");
                    Debug.WriteLine($"аудио из: {sourceAudioPath}");
                    Debug.WriteLine($"аудио в: {targetSongPath}");

                   
                    if (SI.File.Exists(sourceImagePath))
                    {
                        if (!SI.File.Exists(targetImagePath))
                        {
                            SI.File.Copy(sourceImagePath, targetImagePath);
                            Debug.WriteLine("Изображение скопировано!");
                        }
                        else
                        {
                            Debug.WriteLine("Изображение уже существует!");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Исходное изображение не найдено: {sourceImagePath}");
                    }


                    string path = (SI.Path.GetFullPath(SI.Path.Combine("temp_song", selectedTrack.FilePath)));


                    if (SI.File.Exists(sourceAudioPath))
                    {
                        if (CompareDuration(sourceAudioPath, selectedTrack.Duration))
                        {
                            if (!SI.File.Exists(targetSongPath))
                            {
                                SI.File.Copy(sourceAudioPath, targetSongPath);
                                Debug.WriteLine("Аудио скопировано!");
                            }
                            else
                            {
                                Debug.WriteLine("Аудио уже существует!");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Длительность не совпадает, копирование отменено");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Исходное аудио не найдено: {sourceAudioPath}");
                    }
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                AllTrackView();
                TracksListView.SelectionChanged += TracksListView_SelectionChanged;
            }
        }


        private void NewPlayList_Click(object sender, RoutedEventArgs e)
        {
            myPopup.IsOpen = true;
        }

        //на потом
      


        private void NewPlaylist_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                string text = namePlaylistBox.Text;

                if (text == null)
                {

                    MessageBox.Show("Пустовато");

                }

                string Path = SI.Path.Combine("playlist", $"{text}");

                if(Directory.Exists(Path))
                {
                    MessageBox.Show("есть такой уже");
                }
                else
                {
                    Directory.CreateDirectory(Path);
                }
               

            }

            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
            }
            finally
            { 
                myPopup.IsOpen = false;
                PlayListVIew();
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


        private void NewPlaylist_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private string _currentTrackFilePath;
        private string _Duration;
        private async void AddPlay_Click_1(object sender, RoutedEventArgs e)
        {
            TracksListView.SelectionChanged -= TracksListView_SelectionChanged;

            PopupPlaylist.IsOpen = true;

            
            var button = (Button)sender;
            var track = (Track)button.DataContext;
            TracksListView.SelectedItem = track;

            Debug.WriteLine("Кнопка 'в плейлист' нажата");


            try
            {
                if (TracksListView.SelectedItem is Track selectedTrack)
                {

                     _currentTrackFilePath = selectedTrack.FilePath;
                   

                }
            }
            catch { }
            finally { TracksListView.SelectionChanged += TracksListView_SelectionChanged; }

        }

        private async void PlayListView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("выбран плейлист");

            


            try
            {
                string TrackPath = _currentTrackFilePath;

                if (PlayListView1.SelectedItem is PlayList selectedPlaylist)
                {
                    Debug.WriteLine(TrackPath);
                    Debug.WriteLine("выделение заебца");
                    Debug.WriteLine(selectedPlaylist.Directory);



                    string filename = SI.Path.GetFileNameWithoutExtension(TrackPath);


                    string sourceImagePath = SI.Path.GetFullPath(SI.Path.Combine("temp_img", $"{filename}.jpg"));
                    string sourceAudioPath = SI.Path.GetFullPath(SI.Path.Combine("temp_song", $"{filename}.mp3"));

                    string targetPath = selectedPlaylist.Directory;

                    string targetImagePath = SI.Path.Combine(targetPath, $"{filename}.jpg");
                    string targetSongPath = SI.Path.Combine(targetPath, $"{filename}.mp3");



                    Debug.WriteLine(sourceImagePath);
                    Debug.WriteLine(sourceAudioPath);


                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Debug.WriteLine("Сборка мусора");

                    if (SI.File.Exists(sourceImagePath) | SI.File.Exists(sourceAudioPath))
                    {
                        if (!SI.File.Exists(targetPath))
                        {
                            SI.File.Copy(sourceImagePath, targetImagePath);


                            Debug.WriteLine("Изображение скопировано!");
                        }
                        else
                        {
                            Debug.WriteLine("Изображение уже существует!");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Исходное изображение не найдено: {sourceImagePath}");
                    }



                    try
                    {
                        TracksListView.SelectionChanged -= TracksListView_SelectionChanged;




                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        if (TracksListView.SelectedItem is Track selectedTrack)
                        {


                            string searchText = SearchBox.Text.Trim();

                            Debug.WriteLine("Скачивание трека от плейлиста");
                            await PythonScript(1, searchText, selectedTrack.FilePath, selectedTrack.Name);
                            ChangeImage(selectedTrack.ImageData);

                            if (SI.File.Exists(sourceAudioPath))
                            {
                                if (CompareDuration(sourceAudioPath, selectedTrack.Duration))
                                {
                                    if (!SI.File.Exists(targetSongPath))
                                    {
                                        SI.File.Copy(sourceAudioPath, targetSongPath);
                                        Debug.WriteLine("Аудио скопировано!");
                                    }
                                    else
                                    {
                                        Debug.WriteLine("Аудио уже существует!");
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine("Длительность не совпадает, копирование отменено");
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"Исходное аудио не найдено: {sourceAudioPath}");
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    finally
                    {

                      


                        TracksListView.SelectionChanged += TracksListView_SelectionChanged;
                    }


                }
            }

            catch(Exception ex) 
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {

                await Dispatcher.InvokeAsync((Action)(() =>
                {
                    PlayListView1.UnselectAll();
                }), DispatcherPriority.Background);

                PopupPlaylist.IsOpen = false;
            }
           
                




            }


        

        private void add_Play_Click(object sender, RoutedEventArgs e)
        {
            PopupPlaylist.IsOpen = true;




        }

        private void BackToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            TracksListView1.Visibility = Visibility.Collapsed;
            PlayListView.Visibility = Visibility.Visible;
            NewPlayList.Visibility = Visibility.Visible;
            BackToPlaylist.Visibility = Visibility.Collapsed;
        }
    }




}