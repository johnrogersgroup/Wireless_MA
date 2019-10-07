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
    [Register ("IphoneMainView")]
    partial class IphoneMainView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel BatteryLevel1 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel BatteryLevel2 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton BluetoothButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UHMS.iOS.BluetoothView BluetoothViewObj { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView ButtonsView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton DashboardButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton DownloadButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UHMS.iOS.DownloadView DownloadViewObj { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton EventButton1 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton EventButton2 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton EventButton3 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton EventButton4 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton EventButton5 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton EventButton6 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton EventButton7 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton EventButton8 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        Syncfusion.SfChart.iOS.SFChart GraphViewObj1 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        Syncfusion.SfChart.iOS.SFChart GraphViewObj3 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel SessionStatus1 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel SessionStatus2 { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton StartSessionButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton StopSessionButton { get; set; }

        [Action ("DashboardButtonClicked:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void DashboardButtonClicked (UIKit.UIButton sender);

        [Action ("DevicesButtonClicked:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void DevicesButtonClicked (UIKit.UIButton sender);

        [Action ("DownloadButtonClicked:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void DownloadButtonClicked (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (BatteryLevel1 != null) {
                BatteryLevel1.Dispose ();
                BatteryLevel1 = null;
            }

            if (BatteryLevel2 != null) {
                BatteryLevel2.Dispose ();
                BatteryLevel2 = null;
            }

            if (BluetoothButton != null) {
                BluetoothButton.Dispose ();
                BluetoothButton = null;
            }

            if (BluetoothViewObj != null) {
                BluetoothViewObj.Dispose ();
                BluetoothViewObj = null;
            }

            if (ButtonsView != null) {
                ButtonsView.Dispose ();
                ButtonsView = null;
            }

            if (DashboardButton != null) {
                DashboardButton.Dispose ();
                DashboardButton = null;
            }

            if (DownloadButton != null) {
                DownloadButton.Dispose ();
                DownloadButton = null;
            }

            if (DownloadViewObj != null) {
                DownloadViewObj.Dispose ();
                DownloadViewObj = null;
            }

            if (EventButton1 != null) {
                EventButton1.Dispose ();
                EventButton1 = null;
            }

            if (EventButton2 != null) {
                EventButton2.Dispose ();
                EventButton2 = null;
            }

            if (EventButton3 != null) {
                EventButton3.Dispose ();
                EventButton3 = null;
            }

            if (EventButton4 != null) {
                EventButton4.Dispose ();
                EventButton4 = null;
            }

            if (EventButton5 != null) {
                EventButton5.Dispose ();
                EventButton5 = null;
            }

            if (EventButton6 != null) {
                EventButton6.Dispose ();
                EventButton6 = null;
            }

            if (EventButton7 != null) {
                EventButton7.Dispose ();
                EventButton7 = null;
            }

            if (EventButton8 != null) {
                EventButton8.Dispose ();
                EventButton8 = null;
            }

            if (GraphViewObj1 != null) {
                GraphViewObj1.Dispose ();
                GraphViewObj1 = null;
            }

            if (GraphViewObj3 != null) {
                GraphViewObj3.Dispose ();
                GraphViewObj3 = null;
            }

            if (SessionStatus1 != null) {
                SessionStatus1.Dispose ();
                SessionStatus1 = null;
            }

            if (SessionStatus2 != null) {
                SessionStatus2.Dispose ();
                SessionStatus2 = null;
            }

            if (StartSessionButton != null) {
                StartSessionButton.Dispose ();
                StartSessionButton = null;
            }

            if (StopSessionButton != null) {
                StopSessionButton.Dispose ();
                StopSessionButton = null;
            }
        }
    }
}