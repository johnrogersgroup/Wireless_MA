using Foundation;
using System;
using UIKit;
using MvvmCross.Platforms.Ios.Binding.Views;

namespace UHMS.iOS
{
    public partial class SettingsView : MvxView
    {
        public SettingsView (IntPtr handle) : base (handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            NSBundle.MainBundle.LoadNib("SettingsView", this, null);

            RootView.Frame = Bounds;

            AddSubview(RootView);
        }
    }
}