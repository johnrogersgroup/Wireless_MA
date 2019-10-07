// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace UHMS.iOS.Views.Bluetooth
{
    [Register ("BluetoothTableViewCell")]
    partial class BluetoothTableViewCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ConnectButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel DeviceNameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel SlotNameLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ConnectButton != null) {
                ConnectButton.Dispose ();
                ConnectButton = null;
            }

            if (DeviceNameLabel != null) {
                DeviceNameLabel.Dispose ();
                DeviceNameLabel = null;
            }

            if (SlotNameLabel != null) {
                SlotNameLabel.Dispose ();
                SlotNameLabel = null;
            }
        }
    }
}