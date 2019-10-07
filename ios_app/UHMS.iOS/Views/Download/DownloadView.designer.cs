// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace UHMS.iOS
{
    [Register ("DownloadView")]
    partial class DownloadView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView DownloadTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel EmptyStateLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView RootView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (DownloadTableView != null) {
                DownloadTableView.Dispose ();
                DownloadTableView = null;
            }

            if (EmptyStateLabel != null) {
                EmptyStateLabel.Dispose ();
                EmptyStateLabel = null;
            }

            if (RootView != null) {
                RootView.Dispose ();
                RootView = null;
            }
        }
    }
}