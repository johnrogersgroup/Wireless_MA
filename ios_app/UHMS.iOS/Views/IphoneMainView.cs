using System;
using System.Collections.Generic;
using System.Globalization;
using Acr.UserDialogs;
using CoreFoundation;
using CoreNFC;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Converters;
using MvvmCross.Platforms.Ios.Views;
using Syncfusion.SfChart.iOS;
using UHMS.Core.ViewModels;
using UHMS.iOS.Views.Bluetooth;
using UHMS.iOS.Views;
using UIKit;

namespace UHMS.iOS
{
    public partial class IphoneMainView : MvxViewController<IphoneViewModel>
    {
        public IphoneMainView(IntPtr handle) : base(handle)
        {
        }

        /// <summary>
        /// The current active view.
        /// 0 = Dashboard
        /// 1 = Devices
        /// </summary>
        private int currentActiveView = 0;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            BluetoothViewObj.NFCScanButtonOutlet.TouchDown += Scan;
            NavigationController.NavigationBar.TopItem.Title = "AM Monitor";

            BluetoothButton.SetTitleColor(UIColor.FromRGBA(82, 99, 227, 175), UIControlState.Normal);
            DashboardButton.SetTitleColor(UIColor.FromRGBA(82, 99, 227, 255), UIControlState.Normal);
            DashboardButton.Font = UIFont.BoldSystemFontOfSize(17.0f);
            BluetoothButton.Font = UIFont.SystemFontOfSize(17.0f);

            BluetoothViewObj.Hidden = true;
            DownloadViewObj.Hidden = true;

            var set = this.CreateBindingSet<IphoneMainView, MainViewModel>();
            BluetoothTableViewSource DeviceListSource = new BluetoothTableViewSource(BluetoothViewObj.DevicesTableViewOutlet);
            DownloadTableViewSource DownloadDeviceListSource = new DownloadTableViewSource(DownloadViewObj.DeviceTable);

            set.Bind(DeviceListSource).To(vm => vm.ScannedDevices);
            set.Bind(DownloadDeviceListSource).To(vm => vm.DownloadDevices);
            set.Bind(BluetoothButton).To(vm => vm.ScanRequested);
            set.Bind(BluetoothViewObj.RefreshButtonOutlet).To(vm => vm.ScanRequested);

            SessionStatus1.Text = "-";
            SessionStatus2.Text = "-";
            BatteryLevel1.Text = "-";
            BatteryLevel2.Text = "-";

            SessionStringValueConverter sessionConverter = new SessionStringValueConverter();
            BatteryStringValueConverter batteryConverter = new BatteryStringValueConverter();
            set.Bind(SessionStatus1).To(vm => vm.Slot1.SessionStatus).WithConversion(sessionConverter).For("Text");
            set.Bind(SessionStatus2).To(vm => vm.Slot2.SessionStatus).WithConversion(sessionConverter).For("Text");
            set.Bind(BatteryLevel1).To(vm => vm.Slot1.BatteryLevel).WithConversion(batteryConverter).For("Text");
            set.Bind(BatteryLevel2).To(vm => vm.Slot2.BatteryLevel).WithConversion(batteryConverter).For("Text");

            set.Bind(DownloadViewObj.EmptyStateLabelOutlet)
               .To(vm => vm.DownloadViewModel.SlotsAreEmpty).For("Hidden")
               .WithDictionaryConversion(new Dictionary<bool, bool>
                {
                    { true, false},
                    { false, true},
                });

            ScanStringValueConverter refreshStringConverter = new ScanStringValueConverter();
            set.Bind(BluetoothViewObj.RefreshButtonOutlet)
               .To(vm => vm.IsRefreshing.IsRefreshingList).For("Title")
               .WithConversion(refreshStringConverter).OneWay();
            set.Bind(BluetoothViewObj.RefreshButtonOutlet)
               .To(vm => vm.IsRefreshing.IsRefreshingList).For("Enabled");

            BluetoothViewObj.DevicesTableViewOutlet.RegisterNibForCellReuse(BluetoothTableViewCell.Nib, "BluetoothTableViewCell");
            BluetoothViewObj.DevicesTableViewOutlet.Source = DeviceListSource;

            DownloadViewObj.DeviceTable.RegisterNibForCellReuse(DownloadCell.Nib, "DownloadCell");
            DownloadViewObj.DeviceTable.Source = DownloadDeviceListSource;

            List<SFChart> graphs = new List<SFChart> {
                GraphViewObj1,GraphViewObj3
            };

            foreach (var graph in graphs)
            {
                SetSfChart(graph);
                AddLegend(graph);
                for (int i = 0; i < 3; i++)
                {
                    AddSeries(graph);
                }
                graph.Layer.CornerRadius = 4f;
            }

            // Accelerometer
            set.Bind(GraphViewObj1.SeriesAtIndex(0))
               .To(vm => vm.DataList["0_accl_x"])
               .For(v => v.ItemsSource);
            GraphViewObj1.SeriesAtIndex(0).Color = UIColor.FromRGBA(46, 68, 222, 255);
            GraphViewObj1.SeriesAtIndex(0).Label = "X";

            set.Bind(GraphViewObj1.SeriesAtIndex(1))
               .To(vm => vm.DataList["0_accl_y"])
               .For(v => v.ItemsSource);
            GraphViewObj1.SeriesAtIndex(1).Color = UIColor.FromRGBA(88, 194, 99, 255);
            GraphViewObj1.SeriesAtIndex(1).Label = "Y";

            set.Bind(GraphViewObj1.SeriesAtIndex(2))
               .To(vm => vm.DataList["0_accl_z"])
               .For(v => v.ItemsSource);
            GraphViewObj1.SeriesAtIndex(2).Color = UIColor.FromRGBA(182, 47, 220, 255);
            GraphViewObj1.SeriesAtIndex(2).Label = "Z";

            // Accelerometer
            set.Bind(GraphViewObj3.SeriesAtIndex(0))
               .To(vm => vm.DataList["1_accl_x"])
               .For(v => v.ItemsSource);
            GraphViewObj3.SeriesAtIndex(0).Color = UIColor.FromRGBA(46, 68, 222, 255);
            GraphViewObj3.SeriesAtIndex(0).Label = "X";

            set.Bind(GraphViewObj3.SeriesAtIndex(1))
               .To(vm => vm.DataList["1_accl_y"])
               .For(v => v.ItemsSource);
            GraphViewObj3.SeriesAtIndex(1).Color = UIColor.FromRGBA(88, 194, 99, 255);
            GraphViewObj3.SeriesAtIndex(1).Label = "Y";

            set.Bind(GraphViewObj3.SeriesAtIndex(2))
               .To(vm => vm.DataList["1_accl_z"])
               .For(v => v.ItemsSource);
            GraphViewObj3.SeriesAtIndex(2).Color = UIColor.FromRGBA(182, 47, 220, 255);
            GraphViewObj3.SeriesAtIndex(2).Label = "Z";

            List<UIButton> eventButtons = new List<UIButton> {
                EventButton1, EventButton2, EventButton3, EventButton4,
                EventButton5, EventButton6, EventButton7, EventButton8,
                StartSessionButton, StopSessionButton
            };

            foreach (var button in eventButtons)
            {
                button.Layer.CornerRadius = 4f;
            }
            set.Bind(EventButton1).To(vm => vm.LogEvent1);
            set.Bind(EventButton2).To(vm => vm.LogEvent2);
            set.Bind(EventButton3).To(vm => vm.LogEvent3);
            set.Bind(EventButton4).To(vm => vm.LogEvent4);
            set.Bind(EventButton5).To(vm => vm.LogEvent5);
            set.Bind(EventButton6).To(vm => vm.LogEvent6);
            set.Bind(EventButton7).To(vm => vm.LogEvent7);
            set.Bind(EventButton8).To(vm => vm.LogEvent8);


            set.Bind(StartSessionButton).To(vm => vm.StartSession);
            set.Bind(StopSessionButton).To(vm => vm.StopSession);
            set.Bind(DownloadButton).To(vm => vm.TryStopSession);

            set.Apply();
        }
        
        partial void DashboardButtonClicked(UIButton sender)
        {
            if (currentActiveView == 0)
            {
                return;
            }
            BluetoothViewObj.Hidden = true;
            DownloadViewObj.Hidden = true;

            // Make the current button bold and stand out
            DashboardButton.SetTitleColor(UIColor.FromRGBA(82, 99, 227, 255), UIControlState.Normal);
            DashboardButton.Font = UIFont.BoldSystemFontOfSize(17.0f);

            // Make other fonts smaller
            BluetoothButton.Font = UIFont.SystemFontOfSize(17.0f);
            DownloadButton.Font = UIFont.SystemFontOfSize(17.0f);
            BluetoothButton.SetTitleColor(UIColor.FromRGBA(82, 99, 227, 175), UIControlState.Normal);
            DownloadButton.SetTitleColor(UIColor.FromRGBA(82, 99, 227, 175), UIControlState.Normal);

            currentActiveView = 0;
        }

        partial void DevicesButtonClicked(UIButton sender)
        {
            if (currentActiveView == 1)
            {
                return;
            }
            BluetoothViewObj.Hidden = false;
            DownloadViewObj.Hidden = true;

            // Make the current button bold and stand out
            BluetoothButton.SetTitleColor(UIColor.FromRGBA(82, 99, 227, 255), UIControlState.Normal);
            BluetoothButton.Font = UIFont.BoldSystemFontOfSize(17.0f);

            // Make other fonts smaller
            DashboardButton.Font = UIFont.SystemFontOfSize(17.0f);
            DownloadButton.Font = UIFont.SystemFontOfSize(17.0f);
            DashboardButton.SetTitleColor(UIColor.FromRGBA(82, 99, 227, 175), UIControlState.Normal);
            DownloadButton.SetTitleColor(UIColor.FromRGBA(82, 99, 227, 175), UIControlState.Normal);

            currentActiveView = 1;
        }

        partial void DownloadButtonClicked(UIButton sender)
        {
            if (currentActiveView == 2)
            {
                return;
            }
            BluetoothViewObj.Hidden = true;
            DownloadViewObj.Hidden = false;

            // Make the current button bold and stand out
            DownloadButton.SetTitleColor(UIColor.FromRGBA(82, 99, 227, 255), UIControlState.Normal);
            DownloadButton.Font = UIFont.BoldSystemFontOfSize(17.0f);

            // Make other fonts smaller
            DashboardButton.Font = UIFont.SystemFontOfSize(17.0f);
            BluetoothButton.Font = UIFont.SystemFontOfSize(17.0f);
            DashboardButton.SetTitleColor(UIColor.FromRGBA(82, 99, 227, 175), UIControlState.Normal);
            BluetoothButton.SetTitleColor(UIColor.FromRGBA(82, 99, 227, 175), UIControlState.Normal);
            currentActiveView = 2;
        }

        private void SetSfChart(SFChart chart)
        {
            chart.PrimaryAxis = new SFNumericalAxis();
            chart.PrimaryAxis.ShowMajorGridLines = false;
            chart.PrimaryAxis.Visible = false;
            chart.PrimaryAxis.LabelStyle.Font = UIFont.SystemFontOfSize(0.01f);
            chart.PrimaryAxis.AxisLineStyle.LineColor = UIColor.FromRGBA(0, 0, 0, 0);
            chart.PrimaryAxis.AxisLineStyle.LineWidth = 0;
            chart.SecondaryAxis = new SFNumericalAxis();
            chart.SecondaryAxis.LabelStyle.Font = UIFont.SystemFontOfSize(0.01f);
            chart.SecondaryAxis.ShowMajorGridLines = false;
            chart.SecondaryAxis.ShowMinorGridLines = false;
            chart.SecondaryAxis.Visible = false;
            chart.SecondaryAxis.AxisLineStyle.LineColor = UIColor.FromRGBA(0, 0, 0, 0);
            chart.SecondaryAxis.AxisLineStyle.LineWidth = 0;



            chart.EdgeInsets = new UIEdgeInsets(10f, 10f, 10f, 10f);
            chart.BackgroundColor = UIColor.White;
        }

        private void AddLegend(SFChart chart)
        {
            chart.Legend.Visible = true;
            chart.Legend.DockPosition = SFChartLegendPosition.Float;
            chart.Legend.OffsetX = (float)chart.Bounds.Width / 3;
            chart.Legend.OffsetY = 3;
            chart.Legend.OverflowMode = ChartLegendOverflowMode.Wrap;
            chart.Legend.MaxWidth = 100;
        }
        private void AddSeries(SFChart chart)
        {
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
    }

    public class ScanStringValueConverter : IMvxValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Refresh" : "Scanning";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value;
        }
    }

    public class SessionStringValueConverter : IMvxValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (int)value;
            switch (status)
            {
                case 0:
                    return "INACTIVE";
                case 1:
                    return "ACTIVE";
                default:
                    return "-";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value;
        }
    }

    public class BatteryStringValueConverter : IMvxValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (int)value;
            if (status >= 0) return status + "%";
            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value;
        }
    }

    /// <summary>
    /// This partial class of <c>IphoneMainView</c> contains feature for NFC on iPhone.
    /// </summary>
    [MvxFromStoryboard("IphoneMainView")]
    public partial class IphoneMainView : INFCNdefReaderSessionDelegate
    {
        NFCNdefReaderSession Session;
        void Scan(object sender, EventArgs e)
        {
            try
            {
                Session = new NFCNdefReaderSession(this, null, true);
            }
            catch (ObjCRuntime.RuntimeException)
            {
                UserDialogs.Instance.Alert(new AlertConfig
                {
                    Message = "NFC is not available on this device.",
                    Title = "NFC Inavailability"
                });
            }
            if (Session != null)
            {
                Session.BeginSession();
            }
        }
        public void DidDetect(NFCNdefReaderSession session, NFCNdefMessage[] messages)
        {
            foreach (NFCNdefMessage msg in messages)
            {  // adds the messages to a list view
                UserDialogs.Instance.Alert(new AlertConfig
                {
                    Title = "Success",
                    Message = msg.Description
                });
            }
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                //this.TableView.ReloadData();
            });
        }
        public void DidInvalidate(NFCNdefReaderSession session, NSError error)
        {
            var readerError = (NFCReaderError)(long)error.Code;
            if (readerError != NFCReaderError.ReaderSessionInvalidationErrorFirstNDEFTagRead &&
                readerError != NFCReaderError.ReaderSessionInvalidationErrorUserCanceled)
            {
                UserDialogs.Instance.Alert(new AlertConfig
                {
                    Title = "Fail",
                    Message = readerError.ToString()
                });
            }
        }
    }
}