using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.Services;
using State = test.ViewModel.enamS.PlayPauseButtonStates;


namespace test
{
    public abstract class BaseMediaServiceHelper
    {

        public void PlayPouseHandler(ref State state, IMediaService mediaService)
        {
            if (state == State.Pause)
            {
                mediaService.Stop();
                state = State.Play;

            }
            else if (state == State.Play)
            {
                mediaService.Start();
                state = State.Pause;

            }


        }


        public abstract void SelectedTrack();




    }
}
