// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace UHMS.iOS.Views
{
    [Register ("DownloadCell")]
    partial class DownloadCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton DownloadButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIProgressView DownloadProgressBar { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel IdLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel NameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel SlotNameLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (DownloadButton != null) {
                DownloadButton.Dispose ();
                DownloadButton = null;
            }

            if (DownloadProgressBar != null) {
                DownloadProgressBar.Dispose ();
                DownloadProgressBar = null;
            }

            if (IdLabel != null) {
                IdLabel.Dispose ();
                IdLabel = null;
            }

            if (NameLabel != null) {
                NameLabel.Dispose ();
                NameLabel = null;
            }

            if (SlotNameLabel != null) {
                SlotNameLabel.Dispose ();
                SlotNameLabel = null;
            }
        }
    }
}