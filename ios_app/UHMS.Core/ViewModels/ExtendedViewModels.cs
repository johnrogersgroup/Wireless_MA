using System;
using System.Collections.Generic;
using MvvmCross.Commands;
using MvvmCross.ViewModels;

namespace UHMS.Core.ViewModels {
    /// <summary>
    /// Partial class of <c>MainViewModel.</c>
    /// </summary>
    /// <remarks>
    /// Includes implementation of User Interface and User Experience components.
    /// </remarks>

    // Notes to developers. This section was mainly used for UWP Project. Since due to focus on iOS, this section is not being used.
    public partial class MainViewModel : BaseViewModel {
        private MvxCommand<int> updateNumberPane { get; set; }
        private MvxCommand toggleDevicePage;
        private MvxCommand<string> paneButtonOnClick;
        private bool optionsPaneOpen { get; set; } = false;

        public delegate void ToggleScanRequestReceivedHandler();
        public static event ToggleScanRequestReceivedHandler ToggleScanRequested;

        public MvxObservableCollection<string> WideView { get; set; } = new MvxObservableCollection<string>() { "Collapsed", "Collapsed", "Collapsed", "Collapsed", "Collapsed" };
        public MvxObservableCollection<string> SmallView { get; set; } = new MvxObservableCollection<string>() { "Visible", "Visible", "Visible", "Visible", "Visible" };
        public MvxObservableCollection<bool> NumberPane { get; set; } = new MvxObservableCollection<bool>() { true, true, true, true, true };
        public MvxCommand<int> UpdateNumberPane { get { return updateNumberPane = new MvxCommand<int>(index => OnUpdateNumberPane(index)); } }
        public string GraphBackground { get; set; }
        public string HeaderBackground { get; set; }
        public string FontColor { get; set; }
        public string Background { get; set; }
        public string Separator { get; set; }
        public string IfDeviceConn { get; set; }
        public string IfDeviceConnected {
            get {
                //if (EmptyDevices)
                //    return "Collapsed";
                //else
                return "Visible";
            }
        }

        private void SetUserInterface() {
            ChangeColorScheme.Execute("day");
        }

        public Dictionary<string, string> VisibilityCollection { get; set; } = new Dictionary<string, string>() {
            { "settings" , "Collapsed"},
            { "note" , "Collapsed"},
            { "chartRender", "Collapsed"},
            { "noDevice", "Visible"},
            { "logIn", "Visible" },
            { "logInStatus", "Collapsed" },
            { "connectdevices", "Collapsed" },
            { "NoConnectedDevices", "Visible" }
        };
        public Dictionary<string, Boolean> OpenCollection { get; set; } = new Dictionary<string, Boolean>() {
            {"options", false },
            {"connectdevices", false }
        };
        public bool OptionsPaneOpen {
            get { return optionsPaneOpen; }
            set { optionsPaneOpen = value; RaisePropertyChanged(() => OptionsPaneOpen); }
        }

        public void OnUpdateNumberPane(int index) {
            NumberPane[index] = NumberPane[index] ? false : true;
            SmallView[index] = SmallView[index] == "Visible" ? "Collapsed" : "Visible";
            WideView[index] = WideView[index] == "Visible" ? "Collapsed" : "Visible";
        }

        public MvxCommand<string> ChangeColorScheme {
            get {
                return new MvxCommand<string>(color => {
                    if (color == "day") {
                        FontColor = "#9c000000";
                        GraphBackground = "#f2f2f2";
                        HeaderBackground = "#f7d0cb";
                        Background = "#4c4c4c";
                        Separator = "#19000000";
                        IfDeviceConn = "#81858b";
                    } else if (color == "night") {
                        FontColor = "#9cffffff";
                        GraphBackground = "#2E353F";
                        HeaderBackground = "#B76E79";
                        Background = "#e6e6e6";
                        Separator = "19ffffff";
                        IfDeviceConn = "#424952";
                    }
                    RaiseAllPropertiesChanged();
                });
            }
        }

        public MvxCommand ToggleDevicePage {
            get {
                return toggleDevicePage = new MvxCommand(() => {
                    VisibilityCollection["connectdevices"] = VisibilityCollection["connectdevices"] == "Collapsed" ? "Visible" : "Collapsed";
                    RaisePropertyChanged(() => VisibilityCollection); ToggleScanRequested.Invoke();
                });
            }
        }

        public void PaneOnClick(string pane) {
            if (VisibilityCollection[pane] == "Visible")
                OptionsPaneOpen = false;
            VisibilityCollection[pane] = VisibilityCollection[pane] == "Collapsed" ? "Visible" : "Collapsed";
            if (VisibilityCollection[pane] == "Visible")
                OptionsPaneOpen = true;
            if (VisibilityCollection["settings"] == "Visible" && VisibilityCollection["note"] == "Visible") {
                OptionsPaneOpen = true;
                if (pane == "settings")
                    VisibilityCollection["note"] = "Collapsed";
                else if (pane == "note")
                    VisibilityCollection["settings"] = "Collapsed";
            }
            RaisePropertyChanged(() => VisibilityCollection);
        }

        public MvxCommand<string> PaneButtonOnClick { get { return paneButtonOnClick = new MvxCommand<string>((pane) => PaneOnClick(pane)); } }

    }
}
