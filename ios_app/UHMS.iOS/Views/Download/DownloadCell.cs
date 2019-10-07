using System;
using System.Globalization;
using System.Collections.Generic;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Converters;
using MvvmCross.Platforms.Ios.Binding.Views;
using Foundation;
using UHMS.Core.ViewModels;
using UIKit;

namespace UHMS.iOS.Views
{
    public partial class DownloadCell : MvxTableViewCell
    {
        public static readonly NSString Key = new NSString("DownloadCell");
        public static readonly UINib Nib;

        private static UIColor blueColor = UIColor.FromRGBA(82, 99, 227, 255);
        static DownloadCell()
        {
            Nib = UINib.FromName("DownloadCell", NSBundle.MainBundle);
        }

        public DownloadCell(IntPtr handle) : base(handle)
        {
            this.DelayBind(() =>
            {
                var set = this.CreateBindingSet<DownloadCell, DownloadSlotViewModel>();

                this.ClearBindings(DownloadButton);
                set.Bind(SlotNameLabel).For(v => v.Text).To(vm => vm.SlotName);
                set.Bind(NameLabel).For(v => v.Text).To(vm => vm.DeviceName);
                set.Bind(IdLabel).For(v => v.Text).To(vm => vm.Id); 
                set.Bind(DownloadButton).To(vm => vm.DownloadRequested);

                PercentageConverter percentageConverter = new PercentageConverter();
                set.Bind(DownloadButton)
                    .To(vm => vm.DownloadProgress)
                    .For("Title")
                    .WithConversion(percentageConverter);

                set.Bind(DownloadButton)
                    .To(vm => vm.IsDownloading)
                    .For("BackgroundColor")
                    .WithDictionaryConversion(new Dictionary<bool, UIColor>
                    {
                        { true, UIColor.LightGray},
                        { false, blueColor},
                    });

                set.Bind(DownloadButton)
                    .To(vm => vm.IsDownloading)
                    .For("Enabled")
                    .WithDictionaryConversion(new Dictionary<bool, bool>
                    {
                        { true, false},
                        { false, true},
                    });

                // Progress Bar
                set.Bind(DownloadProgressBar)
                    .To(vm => vm.IsDownloading)
                    .For("Hidden")
                    .WithDictionaryConversion(new Dictionary<bool, bool>
                    {
                        { true, false},
                        { false, true},
                    });

                set.Bind(DownloadProgressBar)
                    .To(vm => vm.DownloadProgress)
                    .For("Progress");

                set.Apply();

                DownloadButton.Layer.CornerRadius = 4f;
                AddShadowToView(SlotNameLabel);
            }); 
        }

        private void AddShadowToView(UIView View)
        {
            View.Layer.ShadowColor = UIColor.Black.CGColor;
            View.Layer.ShadowOpacity = 0.1f;
            View.Layer.ShadowRadius = 1.0f;
            View.Layer.ShadowOffset = new System.Drawing.SizeF(1f, 1f);
        }

        public class PercentageConverter : IMvxValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var progress = (float) value;

                if (progress < 0)
                {
                    return "Download";
                }
                else if ((int)(progress * 100) < 1)
                {
                    return "Preparing...";
                }
                else if ((int)(progress * 100) > 99)
                {
                    return "Saving...";
                }
                else
                {
                    return $"{(int)(progress * 100)} %";
                }

            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return 0;
            }
        }
    }
}
