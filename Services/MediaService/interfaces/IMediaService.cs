using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test.Services
{
    public interface IMediaService
    {

        double TotalSeconds { get; }
        double CurrentPosition { get; }

        void Start();
        void Stop();
        void Seek(double seconds);

        event Action<double> PositionChanged;
        event Action<double> DurationChanged;
        

    }
}
