using System;
using Foundation;
using MvvmCross.Platforms.Ios.Core;
using Serilog;
using UHMS.Core;
using UIKit;

namespace UHMS.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register("AppDelegate")]
	public class AppDelegate : MvxApplicationDelegate<Setup, App>
	{
        /// <summary>
        /// Xamarin-specific community license key generated for Sibel Health.
        /// </summary>
        private const string SyncfusionLicenseKey = "MTY5ODRAMzEzNjJlMzIyZTMwV3RCRk94MWhrcWIxY1QwbldTZkF6WkVudWlDc3VldkJpZkZrMEduZXNraz0=";

        /// <summary>
        /// Method invoked after the application has launched to configure the Main Window and View Controller.
        /// </summary>
        /// <param name="application">Reference to the UIApplication that invoked this delegate method.</param>
        /// <param name="launchOptions">An NSDictionary with the launch options, can be null. Possible key values are UIApplication's LaunchOption static properties.</param>
		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(SyncfusionLicenseKey);

            // Prevent application from from sleeping
            UIApplication.SharedApplication.IdleTimerDisabled = true;

            // Attach app loader with options
            var result = base.FinishedLaunching(application, launchOptions);

            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
            {
                Log.Fatal(e.ExceptionObject.ToString());
                Log.CloseAndFlush();
            };

            return result;
        }
    }
}
