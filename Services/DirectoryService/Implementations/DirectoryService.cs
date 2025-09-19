using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using test.ViewModel.CollectionClass;




namespace test.Services
{

  
    public class DirectoryService : IDirectoryService
    {
        public async Task ClearDirectory(string path)
        {

            try
            {
                if (!Directory.Exists(path)) return;


                await Task.Run(() =>
                {

                    DirectoryInfo di = new DirectoryInfo(path);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                });
            }
            catch (ArgumentNullException ex) { Console.WriteLine(ex.Message); }
            catch (ArgumentException ex) { Console.WriteLine(ex.Message); }
            catch (Exception ex) { Console.WriteLine(ex.Message);  }
            

        }


        public async Task CopyFileToDerictory(string sourceDir, string targetDir)
        {
            try
            {
                //Directory.CreateDirectory(targetDir);

                if (!File.Exists(sourceDir))
                {
                    Debug.WriteLine($"Исходный файл не найден: {sourceDir}");
                    return; // Выходим из метода
                }

                if (File.Exists(targetDir))
                {
                    Debug.WriteLine($"существует уже файл");
                    return; // Выходим из метода
                }


                FileInfo file = new FileInfo(sourceDir);

                if (File.Exists(sourceDir))
                {

                    await Task.Run(() =>
                    {
                        file.CopyTo(targetDir);
                        Debug.WriteLine("файл скопирован!");
                    });

                }
                else
                {
                    Debug.WriteLine($"Исходный файд не найдено: {sourceDir}");
                }
            }

            catch(FileFormatException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch(FileNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch(FileLoadException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            catch(Exception ex) { Debug.WriteLine(ex.Message); }
        }


        public async Task DellFile(string FilePath) 
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    Debug.WriteLine("файл удаляется");
                    await Task.Run(() => { File.Delete(FilePath); });                    
                }
                else
                {
                    Debug.WriteLine("файл не удаляется");
                }

            }
            catch (FileNotFoundException ex) { Debug.WriteLine(ex.Message); }
            catch (DirectoryNotFoundException ex) { Debug.WriteLine(ex.Message); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }


        public int LenghtDirectory(string directory)
        {
            string[] folders = Directory.GetDirectories(directory);

            return folders.Length; 

        }

        public int LenghtDirectory(string directory, string name)
        {
            int count = Directory.GetDirectories(directory)
                .Select(Path.GetFileName)  
                .Count(Name => Name.StartsWith(name));

            return count;

        }



    }
}
