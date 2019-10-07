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
    [Register ("IpadMainView")]
    partial class IpadMainView
    {
        [Outlet]
        UIKit.UIButton Button { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton BluetoothButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UHMS.iOS.BluetoothView BluetoothViewObj { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UHMS.iOS.GraphView GraphViewObj1 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UHMS.iOS.GraphView GraphViewObj2 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UHMS.iOS.GraphView GraphViewObj3 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UHMS.iOS.GraphView GraphViewObj4 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UHMS.iOS.MusicView MusicViewObj { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UHMS.iOS.NotesView NotesViewObj { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UHMS.iOS.SettingsView SettingsViewObj { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView TopBar { get; set; }

        [Action ("BluetoothSelected:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void BluetoothSelected (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (BluetoothButton != null) {
                BluetoothButton.Dispose ();
                BluetoothButton = null;
            }

            if (BluetoothViewObj != null) {
                BluetoothViewObj.Dispose ();
                BluetoothViewObj = null;
            }

            if (GraphViewObj1 != null) {
                GraphViewObj1.Dispose ();
                GraphViewObj1 = null;
            }

            if (GraphViewObj2 != null) {
                GraphViewObj2.Dispose ();
                GraphViewObj2 = null;
            }

            if (GraphViewObj3 != null) {
                GraphViewObj3.Dispose ();
                GraphViewObj3 = null;
            }

            if (GraphViewObj4 != null) {
                GraphViewObj4.Dispose ();
                GraphViewObj4 = null;
            }

            if (MusicViewObj != null) {
                MusicViewObj.Dispose ();
                MusicViewObj = null;
            }

            if (NotesViewObj != null) {
                NotesViewObj.Dispose ();
                NotesViewObj = null;
            }

            if (SettingsViewObj != null) {
                SettingsViewObj.Dispose ();
                SettingsViewObj = null;
            }

            if (TopBar != null) {
                TopBar.Dispose ();
                TopBar = null;
            }
        }
    }
}