using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using test.Services;
using test.Services.PlayListService.Implementations;
using test.Services.PlayListService.Interfaces;
using test.SongTabControl;

namespace test
{
    public static class ViewModelRegistration
    {

        public static void RegisterCoreServices(DependencyInjection services)
        {

            services.RegisterSingleton<ITrackCollectionService, TrackCollectionService>();
            services.RegisterSingleton<IMessenger, WeakReferenceMessenger>();
            services.Register<IPathService, PathService>();
            services.Register<IAudioFileNameParser, AudioFileNameParser>();
            services.Register<IDirectoryService, DirectoryService>();
            services.Register<IPythonScriptService, PythonScriptService>();
        }


        public static void RegisterAllTrackTab(DependencyInjection services)
        {
            services.Register<IPlayListService, PlayListServiceForAllTrack>();

        }


        public static void RegisterMainSongTab(DependencyInjection services)
        {

            services.Register<IPlayListService, PlayListServiceForSearchTrack>();

        }


        public static void RegisterPlayListTab(DependencyInjection services)
        {
            services.Register<ICommonPlayListService, PlayListServiceForAllTrack>();

        }


        public static void RegisterTrackOfPlayList(DependencyInjection services)
        {
            services.Register<IPlayListServiceInside, PlayListServiceForTrackOdPlayList>();
        }
    }
}