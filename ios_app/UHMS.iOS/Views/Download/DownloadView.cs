using System;
using Foundation;
using MvvmCross.Platforms.Ios.Binding.Views;
using UIKit;
using UHMS.iOS.Views;

namespace UHMS.iOS
{
    public partial class DownloadView : MvxView
    {
        public UILabel EmptyStateLabelOutlet
        {
            get => EmptyStateLabel;
            set => EmptyStateLabel = value;
        }


        public DownloadView(IntPtr handle) : base(handle)
        {
        }

        public UITableView DeviceTable
        {
            get => DownloadTableView;
            set => DownloadTableView = value;
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            NSBundle.MainBundle.LoadNib("DownloadView", this, null);

            RootView.Frame = Bounds;
            AddSubview(RootView);
        }
    }

    public class DownloadTableViewSource : MvxTableViewSource
    {

        public DownloadTableViewSource(UITableView tableView) : base(tableView)
        {
        }

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
        {
            return tableView.DequeueReusableCell("DownloadCell") as DownloadCell;
        }
    }
}

