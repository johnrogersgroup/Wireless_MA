using System;
using System.Globalization;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Converters;
using MvvmCross.Platforms.Ios.Binding.Views;
using UHMS.Core.ViewModels;
using UIKit;

namespace UHMS.iOS.Views.Bluetooth
{
    /// <summary>
    /// The<c> BluetoothTableViewCell</c> class.
    /// </summary>
    public partial class BluetoothTableViewCell : MvxTableViewCell
    {
        public static readonly NSString Key = new NSString("BluetoothTableViewCell");
        public static readonly UINib Nib;

        public BluetoothTableViewCell(IntPtr handle) : base(handle)
        {
            this.DelayBind(() =>
            {
                var set = this.CreateBindingSet<BluetoothTableViewCell, DeviceViewModel>();

                this.ClearBindings(ConnectButton);
                set.Bind(DeviceNameLabel).For(v => v.Text).To(vm => vm.DeviceName);
                set.Bind(SlotNameLabel).For(v => v.Text).To(vm => vm.SlotName);
                set.Bind(ConnectButton).To(vm => vm.ConnectOrDisposeRequested);
                set.Bind(ConnectButton).To(vm => vm.ConnectOrDisposeActionString).For("Title");

                StringConverter stringConverter = new StringConverter();
                set.Bind(ConnectButton)
                   .To(vm => vm.ConnectOrDisposeActionString).For("BackgroundColor")
                   .WithConversion(stringConverter).OneWay();

                set.Apply();

                ConnectButton.Layer.CornerRadius = 4f;
            });
        }

        static BluetoothTableViewCell()
        {
            Nib = UINib.FromName("BluetoothTableViewCell", NSBundle.MainBundle);
        }
    }

    public class StringConverter : IMvxValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals("Disconnect") ? UIColor.Red : UIColor.FromRGBA(82, 99, 227, 255);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return UIColor.FromRGBA(82, 99, 227, 255);
        }
    }
}
