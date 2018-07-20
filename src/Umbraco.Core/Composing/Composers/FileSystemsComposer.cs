﻿using LightInject;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Composing.Composers
{
    public static class FileSystemsComposer
    {
        public static IServiceRegistry ComposeFileSystems(this IServiceRegistry registry)
        {
            // register FileSystems, which manages all filesystems
            registry.RegisterSingleton<FileSystems>();

            // register IFileSystems, which gives access too all filesystems
            registry.RegisterSingleton<IFileSystems>(factory => factory.GetInstance<FileSystems>());

            // register MediaFileSystem, which can be injected directly
            registry.RegisterSingleton(factory => factory.GetInstance<IFileSystems>().MediaFileSystem);

            // register MediaFileSystem, so that FileSystems can create it
            registry.Register<IFileSystem, MediaFileSystem>((f, wrappedFileSystem)
                => new MediaFileSystem(wrappedFileSystem, f.GetInstance<IContentSection>(), f.GetInstance<IMediaPathScheme>(), f.GetInstance<ILogger>()));

            return registry;
        }
    }
}