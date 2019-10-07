using System;
using MvvmCross.Platforms.Ios.Views;
using MvvmCross.ViewModels;
using UIKit;

namespace UHMS.iOS
{
    /// <summary>
    /// A custom views container to control which storyboard gets used to create the view
    /// depending on the iOS device.
    /// </summary>
    public class UniversalViewsContainer : MvxIosViewsContainer
    {
        public override IMvxIosView CreateViewOfType(Type viewType, MvxViewModelRequest request)
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
            {
                return (IMvxIosView)UIStoryboard.FromName("IpadMainView", null)
                                                 .InstantiateViewController("IpadMainView");
            }
            else
            {
                return (IMvxIosView)UIStoryboard.FromName("IphoneMainView", null)
                                                 .InstantiateViewController("IphoneMainView");
            }
        }
    }
}
