﻿namespace CosmicJam.Editor {

    using ControlzEx.Theming;
    using CosmicJam.Editor.Properties;
    using CosmicJam.Editor.Views;
    using CosmicJam.Library.Services;
    using log4net;
    using Macabre2D.Wpf.Common;
    using Macabre2D.Wpf.Common.Services;
    using MahApps.Metro.Theming;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using Unity;
    using Unity.Lifetime;

    public partial class App : Application {
        private readonly IUnityContainer _container = new UnityContainer();
        private ILoggingService _loggingService;
        private MainWindow _mainWindow;
        private SettingsManager _settingsManager;

        internal static Theme CurrentTheme { get; private set; }

        protected override void OnExit(ExitEventArgs e) {
            Settings.Default.ClosedSuccessfully = e.ApplicationExitCode == 0;
            this.SaveSettings();
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e) {
            CurrentTheme = ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri("pack://application:,,,/Themes/CosmicTheme.xaml"), MahAppsLibraryThemeProvider.DefaultInstance));
            ThemeManager.Current.ChangeTheme(this, CurrentTheme);

            base.OnStartup(e);
            ViewContainer.Instance = this._container;

            this.RegisterTypes();
            this.RegisterInstances();
            this.LoadMainWindow();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            this._loggingService.LogError(e.Exception?.Message ?? $"Cosmic Jam crashed for unknown reasons, but here's the stack trace: {Environment.NewLine}{Environment.StackTrace}");
            Settings.Default.ClosedSuccessfully = false;
            this.SaveSettings();
        }

        private void FullyLoadAssembly(Assembly assembly) {
            foreach (var assemblyName in assembly.GetReferencedAssemblies()) {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                if (!loadedAssemblies.Any(a => a.FullName == assemblyName.FullName)) {
                    var loadedAssembly = Assembly.Load(assemblyName);
                    this.FullyLoadAssembly(loadedAssembly);
                }
            }
        }

        private void LoadMainWindow() {
            var splashScreen = new DraggableSplashScreen();
            splashScreen.Show();

            this._loggingService = this._container.Resolve<ILoggingService>();
            this._settingsManager = this._container.Resolve<SettingsManager>();

            this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;

            var busyService = this._container.Resolve<IBusyService>();
            this._mainWindow = this._container.Resolve<MainWindow>();
            this._settingsManager.Initialize();
            splashScreen.Close();

            // If the application closes successfully, this will get set to true before settings are
            // saved again.
            Settings.Default.ClosedSuccessfully = false;
            Application.Current.MainWindow = this._mainWindow;

            this._mainWindow.Show();
        }

        private void RegisterInstances() {
            var log = LogManager.GetLogger(typeof(App));
            this._container.RegisterInstance(typeof(ILog), log, new ContainerControlledLifetimeManager());
            this._container.RegisterInstance(typeof(SettingsManager), this._container.Resolve<SettingsManager>(), new ContainerControlledLifetimeManager());
            this._container.RegisterInstance<IChangeDetectionService>(this._container.Resolve<ISongService>());
        }

        private void RegisterTypes() {
            this._container.RegisterType<IAssemblyService, AssemblyService>(new ContainerControlledLifetimeManager());
            this._container.RegisterType<IBusyService, BusyService>(new ContainerControlledLifetimeManager());
            this._container.RegisterType<ILoggingService, LoggingService>();
            this._container.RegisterType<IUndoService, UndoService>(new ContainerControlledLifetimeManager());
            this._container.RegisterType<IValueEditorService, ValueEditorService>(new ContainerControlledLifetimeManager());
            this._container.RegisterType<ICommonDialogService, CommonDialogService>(new ContainerControlledLifetimeManager());
            this._container.RegisterType<ISongService, SongService>(new ContainerControlledLifetimeManager());
        }

        private void SaveSettings() {
        }
    }
}