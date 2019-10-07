using Foundation;
using System;
using UIKit;
using MvvmCross.Platforms.Ios.Binding.Views;

namespace UHMS.iOS
{
    public partial class BluetoothView : MvxView
    {
        public UITableView DevicesTableViewOutlet
        {
            get => DevicesTableView;
            set => DevicesTableView = value;
        }

        public UIButton RefreshButtonOutlet
        {
            get => RefreshButton;
            set => RefreshButton = value;
        }

        public UIButton NFCScanButtonOutlet
        {
            get => NFCScanButton;
            set => NFCScanButton = value;
        }

        public BluetoothView(IntPtr handle) : base(handle)
        { }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            NSBundle.MainBundle.LoadNib("BluetoothView", this, null);

            RootView.Frame = Bounds;
            AddSubview(RootView);
        }
    }
}