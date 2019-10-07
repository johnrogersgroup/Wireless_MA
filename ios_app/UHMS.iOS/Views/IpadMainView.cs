using System;
using System.Collections.Generic;
using CoreGraphics;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Views;
using MvvmCross.ViewModels;
using Syncfusion.SfChart.iOS;
using UHMS.Core.Models.Data;
using UHMS.Core.ViewModels;
using UIKit;

namespace UHMS.iOS.Views
{
    [MvxFromStoryboard("IpadMainView")]
    public partial class IpadMainView : MvxViewController<MainViewModel>
    {
        UIButton SelectedButton = null;
        UIView SelectedView = null;

        public IpadMainView(IntPtr handle) : base(handle)
        {
            this.Request = MvxViewModelRequest<MainViewModel>.GetDefaultRequest();
        }

        partial void BluetoothSelected(UIButton sender)
        {
            ToggleLeftTab(sender, BluetoothViewObj);
        }

        private void ToggleLeftTab(UIButton sender, UIView view)
        {
            if (SelectedButton == sender)
            {
                SelectedButton.BackgroundColor = UIColor.Clear;
                view.Hidden = true;

                SelectedButton = null;
                SelectedView = null;
            }
            else if (SelectedButton != null)
            {
                sender.BackgroundColor = UIColor.FromRGB(0.65f, 0.65f, 0.65f);
                SelectedButton.BackgroundColor = UIColor.Clear;
                view.Hidden = false;
                SelectedView.Hidden = true;

                SelectedButton = sender;
                SelectedView = view;
            }
            else if (SelectedButton == null)
            {
                sender.BackgroundColor = UIColor.FromRGB(0.65f, 0.65f, 0.65f);
                view.Hidden = false;

                SelectedButton = sender;
                SelectedView = view;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationController.SetNavigationBarHidden(true, false);

            GraphViewObj1.TypeString = "Accelerometer 1";
            GraphViewObj1.TextColor = UIColor.LightGray;

            GraphViewObj2.TypeString = "Gyroscope 1";
            GraphViewObj2.TextColor = UIColor.LightGray;

            GraphViewObj3.TypeString = "Accelerometer 2";
            GraphViewObj3.TextColor = UIColor.LightGray;

            GraphViewObj4.TypeString = "Gyroscope 2";
            GraphViewObj4.TextColor = UIColor.LightGray;


            var set = this.CreateBindingSet<IpadMainView, MainViewModel>();
            Bluetooth.BluetoothTableViewSource DeviceListSource = new Bluetooth.BluetoothTableViewSource(BluetoothViewObj.DevicesTableViewOutlet);

            set.Bind(DeviceListSource).To(vm => vm.ScannedDevices);
            set.Bind(BluetoothButton).To(vm => vm.ScanRequested);

            BluetoothViewObj.DevicesTableViewOutlet.RegisterNibForCellReuse(Bluetooth.BluetoothTableViewCell.Nib, "BluetoothTableViewCell");
            BluetoothViewObj.DevicesTableViewOutlet.Source = DeviceListSource;

            List<GraphView> graphs = new List<GraphView> {
                GraphViewObj1,GraphViewObj2,GraphViewObj3,GraphViewObj4
            };

            foreach (var graph in graphs) {
                SetSfChart(graph.chart);
                for (int i = 0; i < 3; i++)
                {
                    AddSeries(graph.chart);
                }
            }
           
            // Accelerometer
            set.Bind(GraphViewObj1.chart.SeriesAtIndex(0))      
               .To(vm => vm.DataList["0_accl_x"])      
               .For(v => v.ItemsSource);
            GraphViewObj1.chart.SeriesAtIndex(0).Color = UIColor.Green;

            set.Bind(GraphViewObj1.chart.SeriesAtIndex(1))
               .To(vm => vm.DataList["0_accl_y"])
               .For(v => v.ItemsSource);
            GraphViewObj1.chart.SeriesAtIndex(1).Color = UIColor.Purple;

            set.Bind(GraphViewObj1.chart.SeriesAtIndex(2))
               .To(vm => vm.DataList["0_accl_z"])
               .For(v => v.ItemsSource);
            GraphViewObj1.chart.SeriesAtIndex(2).Color = UIColor.Blue;

            // Gyroscope
            set.Bind(GraphViewObj2.chart.SeriesAtIndex(0))
               .To(vm => vm.DataList["0_gyro_x"])
               .For(v => v.ItemsSource);
            GraphViewObj2.chart.SeriesAtIndex(0).Color = UIColor.Green;

            set.Bind(GraphViewObj2.chart.SeriesAtIndex(1))
               .To(vm => vm.DataList["0_gyro_y"])
               .For(v => v.ItemsSource);
            GraphViewObj2.chart.SeriesAtIndex(1).Color = UIColor.Purple;

            set.Bind(GraphViewObj2.chart.SeriesAtIndex(2))
               .To(vm => vm.DataList["0_gyro_z"])
               .For(v => v.ItemsSource);
            GraphViewObj2.chart.SeriesAtIndex(2).Color = UIColor.Blue;

            // Accelerometer
            set.Bind(GraphViewObj3.chart.SeriesAtIndex(0))
               .To(vm => vm.DataList["1_accl_x"])
               .For(v => v.ItemsSource);
            GraphViewObj3.chart.SeriesAtIndex(0).Color = UIColor.Green;

            set.Bind(GraphViewObj3.chart.SeriesAtIndex(1))
               .To(vm => vm.DataList["1_accl_y"])
               .For(v => v.ItemsSource);
            GraphViewObj3.chart.SeriesAtIndex(1).Color = UIColor.Purple;

            set.Bind(GraphViewObj3.chart.SeriesAtIndex(2))
               .To(vm => vm.DataList["1_accl_z"])
               .For(v => v.ItemsSource);
            GraphViewObj3.chart.SeriesAtIndex(2).Color = UIColor.Blue;

            // Gyroscope
            set.Bind(GraphViewObj4.chart.SeriesAtIndex(0))
               .To(vm => vm.DataList["1_gyro_x"])
               .For(v => v.ItemsSource);
            GraphViewObj4.chart.SeriesAtIndex(0).Color = UIColor.Green;

            set.Bind(GraphViewObj4.chart.SeriesAtIndex(1))
               .To(vm => vm.DataList["1_gyro_y"])
               .For(v => v.ItemsSource);
            GraphViewObj4.chart.SeriesAtIndex(1).Color = UIColor.Purple;

            set.Bind(GraphViewObj4.chart.SeriesAtIndex(2))
               .To(vm => vm.DataList["1_gyro_z"])
               .For(v => v.ItemsSource);
            GraphViewObj4.chart.SeriesAtIndex(2).Color = UIColor.Blue;

            set.Apply();
        }

        private void SetSfChart(SFChart chart)
        {
            chart.PrimaryAxis = new SFNumericalAxis();
            chart.PrimaryAxis.ShowMajorGridLines = false;
            chart.PrimaryAxis.LabelStyle.Font = UIFont.SystemFontOfSize(0.01f);
            chart.PrimaryAxis.AxisLineStyle.LineColor = UIColor.FromRGBA(0, 0, 0, 0);
            chart.PrimaryAxis.AxisLineStyle.LineWidth = 0;
            chart.SecondaryAxis = new SFNumericalAxis();
            chart.SecondaryAxis.LabelStyle.Font = UIFont.SystemFontOfSize(0.01f);
            chart.SecondaryAxis.ShowMajorGridLines = false;
            chart.SecondaryAxis.ShowMinorGridLines = false;
            chart.SecondaryAxis.AxisLineStyle.LineColor = UIColor.FromRGBA(0, 0, 0, 0);
            chart.SecondaryAxis.AxisLineStyle.LineWidth = 0;


        }

        private void AddSeries(SFChart chart) {
            SFFastLineSeries series = new SFFastLineSeries()
            {
                XBindingPath = "Index",
                YBindingPath = "Value",
                LineWidth = 1,
                ListenPropertyChange = true,
                EnableAntiAliasing = false
            };
            chart.Series.Add(series);
        }

        public UIImage ScaleImageToSize(UIImage image, CGSize newSize)
        {
            double widthRatio = newSize.Width / image.Size.Width;
            double heightRatio = newSize.Height / image.Size.Height;
            double aspectRatio = Math.Min(widthRatio, heightRatio);

            return image.Scale(new CGSize(image.Size.Width * aspectRatio, image.Size.Height * aspectRatio));
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
