using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using test.Services;
using test.SongTabControl;

namespace test
{
    public static class ViewModelRegistration
    {

        public static void RegisterCoreServices(DependencyInjection services)
        {
            // ✅ Регистрируем ОДИН экземпляр TrackCollectionService как Singleton:
            services.RegisterSingleton<ITrackCollectionService, TrackCollectionService>();

            // ✅ Регистрируем общие сервисы (один раз для всех):
            services.Register<IPathService, PathService>();
            services.Register<IAudioFileNameParser, AudioFileNameParser>();
            services.Register<IDirectoryService, DirectoryService>();
            services.Register<IPythonScriptService, PythonScriptService>();
        }

        // ✅ Регистрация для AllTrackTab (минимальная):
        public static void RegisterAllTrackTab(DependencyInjection services)
        {
            services.Register<IPlayListService, PlayListServiceForAllTrack>();
            // Остальные сервисы уже зарегистрированы в RegisterCoreServices
        }

        // ✅ Регистрация для MainSongTab:
        public static void RegisterMainSongTab(DependencyInjection services)
        {
            // ✅ Только специфичные сервисы:
            services.Register<IPlayListService, PlayListServiceForSearchTrack>();
            // Остальные сервисы уже зарегистрированы в RegisterCoreServices
        }

        // ✅ Регистрация для PlayListTab:
        public static void RegisterPlayListTab(DependencyInjection services)
        {
            services.Register<IPlayListService, PlayListServiceForPlayListDirectory>();
            
        }

        // ✅ Регистрация для TrackOfPlayList:
        public static void RegisterTrackOfPlayList(DependencyInjection services)
        {
     
        }
    }
}