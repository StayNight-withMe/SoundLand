using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test.ViewModel.CollectionClass
{
    public class Track
    {
        
        //все светитя начало после перехода с .net framework на .net т.к. у них нет значения, ну и похуй, нахуя мне им нули присваивать
        public string Name { get; set; }
        public string Artist { get; set; }
        public string FileName { get; set; }
        public string ImgFilePath {  get; set; }
        public string? SongFilePath { get; set; }
        public byte[] ImageData { get; set; } // Изображение в памяти ибо по иному хуево + нужны абсолютные пути, без них тоже поебота
        public string Duration { get; set; }
    }
}
