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
    [Register ("InfoView")]
    partial class InfoView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CurrentValueLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView RootView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TypeLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CurrentValueLabel != null) {
                CurrentValueLabel.Dispose ();
                CurrentValueLabel = null;
            }

            if (RootView != null) {
                RootView.Dispose ();
                RootView = null;
            }

            if (TypeLabel != null) {
                TypeLabel.Dispose ();
                TypeLabel = null;
            }
        }
    }
}