using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.ViewModels;
using UHMS.Core.Models;
using UHMS.Core.Services;
using UHMS.Core.ViewModels;

namespace UHMS.Core
{
    public class App : MvxApplication
	{
		public override void Initialize()
		{
			CreatableTypes()
				.EndingWith("Service")
				.AsInterfaces()
				.RegisterAsLazySingleton();

            // NOTE: This is where we register the Services we use throughout the application as singletons using IOC/Dep
            // For more information please read about MvvmCross's implementation of Inversion of Control (https://www.mvvmcross.com/documentation/fundamentals/inversion-of-control-ioc?scroll=25)
            // And about how to use these registered services using Dependency Injection (https://www.mvvmcross.com/documentation/fundamentals/dependency-injection?scroll=25)

            // External Services
            Mvx.RegisterSingleton<IUserDialogs>(() => UserDialogs.Instance);

            // Internal Services
            Mvx.ConstructAndRegisterSingleton<IDeviceSlotService, DeviceSlotService>();
            Mvx.ConstructAndRegisterSingleton<IDataLoggingService, DataLoggingService>();
            Mvx.ConstructAndRegisterSingleton<ISensorDataService, SensorDataService>();
            Mvx.ConstructAndRegisterSingleton<IBluetoothService, BluetoothService>();

            ToastConfig.DefaultPosition = ToastPosition.Top;

            RegisterAppStart<MainViewModel>();
		}
	}
}
