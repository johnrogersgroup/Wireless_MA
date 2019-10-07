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
    [Register ("GraphView")]
    partial class GraphView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView RootView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        Syncfusion.SfChart.iOS.SFChart SfChart { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TypeLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (RootView != null) {
                RootView.Dispose ();
                RootView = null;
            }

            if (SfChart != null) {
                SfChart.Dispose ();
                SfChart = null;
            }

            if (TypeLabel != null) {
                TypeLabel.Dispose ();
                TypeLabel = null;
            }
        }
    }
}