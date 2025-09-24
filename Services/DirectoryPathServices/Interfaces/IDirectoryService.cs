using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test.Services
{
    public interface IDirectoryService
    {
        async Task ClearDirectory(string directoryPath) { }
        async Task CopyFileToDerictory(string sourceDir, string targetDir) { }
        async Task DellFile(string FilePath) { }
        public int LenghtDirectory(string directory);
        public int LenghtDirectory(string directory, string name);


    }
}
