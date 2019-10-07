using Foundation;
using System;
using UIKit;
using MvvmCross.Platforms.Ios.Binding.Views;

namespace UHMS.iOS
{
    public partial class MusicView : MvxView
    {
        public MusicView (IntPtr handle) : base (handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            NSBundle.MainBundle.LoadNib("MusicView", this, null);

            RootView.Frame = Bounds;

            AddSubview(RootView);
        }
    }
}