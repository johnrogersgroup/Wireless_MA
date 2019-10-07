using Foundation;
using System;
using UIKit;
using MvvmCross.Platforms.Ios.Binding.Views;

namespace UHMS.iOS
{
    public partial class NotesView : MvxView
    {
        public NotesView (IntPtr handle) : base (handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            NSBundle.MainBundle.LoadNib("NotesView", this, null);

            RootView.Frame = Bounds;

            AddSubview(RootView);
        }
    }
}