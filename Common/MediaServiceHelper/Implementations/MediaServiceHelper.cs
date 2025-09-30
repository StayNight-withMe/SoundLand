using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using state = test.ViewModel.enamS.PlayPauseButtonStates;

namespace test.Services
{
    public class MediaServiceHelper
    {


        public static string GetPlayPauseButtonText(state state) => state switch
        {
            state.Play => "Play",
            state.Pause => "Pause",
            _ => "Play"
        };

        public void PlayPouseHandler(ref string ButtonText, IMediaService mediaService)
        {
         

        }
    }
}
