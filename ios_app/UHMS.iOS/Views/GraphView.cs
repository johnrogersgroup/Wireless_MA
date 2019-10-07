using Foundation;
using System;
using UIKit;
using Syncfusion.SfChart.iOS;
using MvvmCross.Platforms.Ios.Binding.Views;

namespace UHMS.iOS
{
    public partial class GraphView : MvxView
    {
        public UIColor TextColor
        {
            get => TypeLabel.TextColor;
            set => TypeLabel.TextColor = value;
        }

        public string TypeString
        {
            get => TypeLabel.Text;
            set => TypeLabel.Text = value;
        }

        public SFChart chart
        {
            get => SfChart;
            set => SfChart = value;
        }

        public GraphView (IntPtr handle) : base (handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            NSBundle.MainBundle.LoadNib("GraphView", this, null);

            RootView.Frame = Bounds;

            AddSubview(RootView);
        }
    }
}