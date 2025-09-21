using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;


namespace test.Services
{



    public class PythonScriptService : IPythonScriptService
    {
        private readonly Dictionary<string, IPythonScriptService> _scripts;

        public PythonScriptService()
        {
            _scripts = new Dictionary<string, IPythonScriptService>();


        }


        public async Task PythonScript(string ScriptFile, int arg, string searchText, string start, string end)
        {
            Process process = null;

            try
            {

                Debug.WriteLine($"Начало выполнения PythonScript: arg={arg}, searchText={searchText}, start={start}, end={end}");

                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ScriptFile);


                if (string.IsNullOrWhiteSpace(searchText))
                {
                    Debug.WriteLine("Пустой searchText");
                    MessageBox.Show("Пусто еп твою мать");
                    return;
                }

                string argument = string.Empty;
                string nameWithoutExtension = Path.GetFileNameWithoutExtension(start);

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

                
                process = new Process(); // Создаем процесс
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



                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                Debug.WriteLine("Ожидание завершения процесса");


                string output = await outputTask;
                string error = await errorTask;

                Debug.WriteLine($"Python процесс завершен. Код выхода: {process.ExitCode}");
                Debug.WriteLine($"Вывод Python: {output}");
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.WriteLine($"Ошибка Python: {error}");
                }

                Debug.WriteLine($"Python output: {output}");
                Debug.WriteLine($"Python error: {error}");

            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Выполнение PythonScript отменено");
                Console.WriteLine("Выполнение отменено.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка выполнения PythonScript: {ex.Message}");
                Debug.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                Debug.WriteLine("Освобождение слота");
                process.Kill(); //так неправильно делать, просто я бездарь ебаный

            }

        

           

        }
           

        
        
    }
}





