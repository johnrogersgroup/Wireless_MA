using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Acr.UserDialogs;
namespace UHMS.Core.ViewModels {

    /// <summary>
    /// Partial class of <c>MainViewModel.</c>
    /// </summary>
    /// <remarks>
    /// Includes implementation related to memo feature.
    /// </remarks>
    public partial class MainViewModel : BaseViewModel {
        public MvxObservableCollection<MemoBox> MemoCollection { get; set; } = new MvxObservableCollection<MemoBox> { };
        public IMvxCommand NotAvailable {
            get {
                return new MvxCommand(() => {
                    _userDialogs.Alert(new AlertConfig {
                        Title = "Unavailable",
                        Message = "This feature is not available yet. Please wait for next version."
                    });
                });
            }
        }
    }

    public class MemoBox {
        public string Title { get; set; }
        public string Details { get; set; }
    }
}
