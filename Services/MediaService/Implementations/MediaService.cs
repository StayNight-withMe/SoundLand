using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace test.Services
{
    public class MediaService : IMediaService
    {//

        private MediaElement _mediaElement;

        private DispatcherTimer _timer;
        public MediaElement MediaElement
        {
        get {  return _mediaElement; }

        set { if(_mediaElement != null)
                {
                    _mediaElement.MediaEnded -= OnMediaEnded;
                    _mediaElement.MediaOpened -= OnMediaOpened;
                }

        _mediaElement = value;

                if (_mediaElement != null)
                {

                    _mediaElement.MediaEnded += OnMediaEnded;
                    _mediaElement.MediaOpened += OnMediaOpened;

                }
           }
        }

       //
        public double CurrentPosition => _mediaElement?.Position.TotalSeconds ?? 0;

        public double TotalSeconds => _mediaElement.NaturalDuration.HasTimeSpan
          ? _mediaElement.NaturalDuration.TimeSpan.TotalSeconds
          : 0;



        public MediaService() 
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }   


        private void OnTimerTick(object sender, EventArgs e)
        {
            PositionChanged?.Invoke(CurrentPosition);
        }

        private void OnMediaOpened(object sender, RoutedEventArgs e)
        {
            DurationChanged?.Invoke(TotalSeconds);
        }

       

        private void OnMediaEnded(object sender, RoutedEventArgs e)
        {
            DurationChanged?.Invoke(TotalSeconds);
            PositionChanged?.Invoke(0);
            MediaEndedChanged.Invoke();
            //_timer.Stop();
        }


        public void Start() => _mediaElement?.Play();
        public void Stop() => _mediaElement?.Pause(); 
        public void Seek(double seconds) => _mediaElement.Position = TimeSpan.FromSeconds(seconds);

        public event Action<double> DurationChanged;
        public event Action<double> PositionChanged;
        public event Action MediaEndedChanged;

        public void SetMediaElement(MediaElement mediaElement)
        {
            MediaElement = mediaElement;
        }


    }
}
