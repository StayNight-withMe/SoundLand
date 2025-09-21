using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test.Services
{
    public interface IPythonScriptService
    {
        async Task PythonScript(string ScriptFile, int arg, string searchText, string start, string end) { }
    }
}
