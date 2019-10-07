using System;
using System.IO;
using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Logging;
using MvvmCross.Platforms.Ios.Core;
using MvvmCross.Platforms.Ios.Views;
using MvvmCross.ViewModels;
using Plugin.BLE;
using Serilog;
using UHMS.Core;

namespace UHMS.iOS
{
	public class Setup : MvxIosSetup<App>
	{
		public Setup() : base()
		{

		}

		protected override IMvxApplication CreateApp()
		{
			return new UHMS.Core.App();
		}

		protected override void InitializeIoC()
		{
			base.InitializeIoC();
			Mvx.RegisterSingleton(() => UserDialogs.Instance);
			Mvx.RegisterSingleton(() => CrossBluetoothLE.Current);
			Mvx.RegisterSingleton(() => CrossBluetoothLE.Current.Adapter);
		}

        /// Use our own views container so we can control view creation logic
        protected override IMvxIosViewsContainer CreateIosViewsContainer()
        {
            return new iOS.UniversalViewsContainer();
        }

        public override MvxLogProviderType GetDefaultLogProviderType() => MvxLogProviderType.Serilog;

        protected override IMvxLogProvider CreateLogProvider()
        {
            var documentFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Logs";
            var path = Path.Combine(documentFolderPath, "AMMonitorLog.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.NSLog(outputTemplate: "[{Level}] ({SourceContext}.{Class}) {Message}{Exception}")
                .WriteTo.File(path, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Class}) {Message}{NewLine}{Exception}", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Serilog.Debugging.SelfLog.Enable(msg => Log.Debug(msg));

            return base.CreateLogProvider();
        }
    }
}