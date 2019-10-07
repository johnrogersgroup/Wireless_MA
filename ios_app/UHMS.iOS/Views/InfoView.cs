using Foundation;
using System;
using UIKit;
using MvvmCross.Platforms.Ios.Binding.Views;

namespace UHMS.iOS
{
    public partial class InfoView : MvxView
    {
        public UIColor TextColor
        {
            get => TypeLabel.TextColor;
            set
            {
                TypeLabel.TextColor = value;
                CurrentValueLabel.TextColor = value;
            }
        }

        public string TypeString
        {
            get => TypeLabel.Text;
            set => TypeLabel.Text = value;
        }

        public string CurrentValueString
        {
            get => CurrentValueLabel.Text;
            set => CurrentValueLabel.Text = value;
        }

        public InfoView (IntPtr handle) : base (handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            NSBundle.MainBundle.LoadNib("InfoView", this, null);

            RootView.Frame = Bounds;

            AddSubview(RootView);
        }
    }
}