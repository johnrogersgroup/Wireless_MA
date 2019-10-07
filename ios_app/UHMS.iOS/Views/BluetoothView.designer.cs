// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace UHMS.iOS
{
    [Register ("BluetoothView")]
    partial class BluetoothView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView DevicesTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton NFCScanButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton RefreshButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView RootView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (DevicesTableView != null) {
                DevicesTableView.Dispose ();
                DevicesTableView = null;
            }

            if (NFCScanButton != null) {
                NFCScanButton.Dispose ();
                NFCScanButton = null;
            }

            if (RefreshButton != null) {
                RefreshButton.Dispose ();
                RefreshButton = null;
            }

            if (RootView != null) {
                RootView.Dispose ();
                RootView = null;
            }
        }
    }
}