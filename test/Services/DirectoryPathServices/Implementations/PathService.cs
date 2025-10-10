using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;





namespace test.Services
{

    public class PathService : IPathService
    {
        public GetPath ParseAll()
        {

            string BasePath = AppDomain.CurrentDomain.BaseDirectory;

            return new GetPath
            {
                TempSongPath = Path.Combine(BasePath ,@"temp_song"),
                TempImgPath = Path.Combine(BasePath, @"temp_img"),
                AllImgPath = Path.Combine(BasePath, @"ALL\img"),
                AllSongPath = Path.Combine(BasePath, @"ALL\song"),
                PlayListPath = Path.Combine(BasePath ,@"PlayList"),
                BasePath = BasePath,
            };
        
        }

    }
}
