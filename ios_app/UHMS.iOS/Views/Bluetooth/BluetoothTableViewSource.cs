using System;
using Foundation;
using UIKit;
using CoreGraphics;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;
using UHMS.Core.ViewModels;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using UHMS.Core.Models;

namespace UHMS.iOS.Views.Bluetooth
{
    
	public class BluetoothTableViewSource : MvxTableViewSource
    {
        
        public BluetoothTableViewSource(UITableView tableView) : base(tableView)
        {
        }

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
        {
            return tableView.DequeueReusableCell("BluetoothTableViewCell") as BluetoothTableViewCell;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 44;
        }
    }
}
