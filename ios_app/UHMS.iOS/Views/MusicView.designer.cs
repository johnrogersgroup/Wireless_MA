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
    [Register ("MusicView")]
    partial class MusicView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView RootView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (RootView != null) {
                RootView.Dispose ();
                RootView = null;
            }
        }
    }
}