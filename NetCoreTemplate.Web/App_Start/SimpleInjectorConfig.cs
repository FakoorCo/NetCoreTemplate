﻿namespace NetCoreTemplate.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.DotNet.PlatformAbstractions;

    using NetCoreTemplate.Authentication;
    using NetCoreTemplate.Authentication.Client;
    using NetCoreTemplate.DAL.Configuration;
    using NetCoreTemplate.DAL.Managers;
    using NetCoreTemplate.DAL.PersistenceLayer;
    using NetCoreTemplate.FileManager;
    using NetCoreTemplate.FileManager.Interfaces;
    using NetCoreTemplate.Providers.General;
    using NetCoreTemplate.Providers.Interfaces;
    using NetCoreTemplate.Providers.Interfaces.General;
    using NetCoreTemplate.Services.General;
    using NetCoreTemplate.Services.Interfaces;
    using NetCoreTemplate.Services.Interfaces.General;
    using NetCoreTemplate.SharedKernel.Interfaces.Managers;
    using NetCoreTemplate.SharedKernel.Interfaces.PersistenceLayer;
    using NetCoreTemplate.SharedKernel.ServiceContainer;
    using NetCoreTemplate.ViewModelProcessors.Controllers.User;
    using NetCoreTemplate.ViewModelProcessors.Interfaces;
    using NetCoreTemplate.ViewModels.General;

    using RazorLight;

    using SimpleInjector;

    public static class SimpleInjectorConfig
    {
        public static void Register(Container container)
        {
            var serviceContainer = new SimpleInjectorServiceContainer(container);
            container.RegisterSingleton(typeof(IServiceContainer), () => serviceContainer);
            ServiceContainer.Register(serviceContainer);

            RegisterGeneralDependecies(container);
            RegisterViewModelProcessors(container);
            RegisterProviders(container);
            RegisterServices(container);
            OverrideViewModelProcessors(container);
            RegisterViewEngine(container);
        }

        private static void RegisterGeneralDependecies(Container container)
        {
            container.Register<IPersistenceLayer, PersistenceLayer>(Lifestyle.Scoped);
            container.Register<ITranslationManager, TranslationManager>();
            container.Register(typeof(IBaseService<>), new List<Assembly> { typeof(IBaseService<>).Assembly }, Lifestyle.Scoped);
            container.Register(typeof(IBaseProvider<>), new List<Assembly> { typeof(IBaseProvider<>).Assembly }, Lifestyle.Scoped);
            container.Register<IDatabaseConfiguration, DatabaseConfiguration>();
            container.Register(typeof(IHtmlHelper<>), typeof(HtmlHelper<>));
            container.Register<IAuthenticationClient, AuthenticationClient>();
            container.Register<IFileEncrypter, FileEncrypter>();
            container.Register<IFileWriter, FileWriter>();
            container.Register<IFileManager, FileManager>();
        }

        private static void RegisterViewModelProcessors(Container container)
        {
            var assembly = Assembly.Load("NetCoreTemplate.ViewModelProcessors");

            container.Register(typeof(ILoader<>), assembly);
            container.Register(typeof(ILoader<,>), assembly);
            container.Register(typeof(IListLoader<>), assembly);
            container.Collection.Register(typeof(IReloader<>), assembly);
            container.Register(typeof(IValidator<>), assembly);
            container.Register(typeof(IProcessor<>), assembly);
        }

        private static void RegisterProviders(Container container)
        {
            container.Register<IUserProvider, UserProvider>(Lifestyle.Scoped);
            container.Register<IPermissionProvider, PermissionProvider>(Lifestyle.Scoped);
            container.Register<IRoleProvider, RoleProvider>(Lifestyle.Scoped);
            container.Register<IMailQueueProvider, MailQueueProvider>(Lifestyle.Scoped);
        }

        private static void RegisterServices(Container container)
        {
            container.Register<IMailQueueService, MailQueueService>(Lifestyle.Scoped);
        }

        private static void OverrideViewModelProcessors(Container container)
        {
            container.Register<IReloader<UserViewModel>, UserLoader>();
        }

        private static void RegisterViewEngine(Container container)
        {
            var basePath = ApplicationEnvironment.ApplicationBasePath;
            var directorySeparator = Path.DirectorySeparatorChar;
            var index = basePath.IndexOf("bin", StringComparison.Ordinal);
            basePath = basePath.Substring(0, index <= 0 ? basePath.Length : index) + $"Views{directorySeparator}";

            var engine = new RazorLightEngineBuilder()
                .UseFilesystemProject(basePath)
                .UseMemoryCachingProvider()
                .Build();

            container.RegisterSingleton<IRazorLightEngine>(() => engine);
        }
    }
}
