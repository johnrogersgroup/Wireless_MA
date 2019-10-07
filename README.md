# UHMS-Host
[![Build Status](https://dev.azure.com/sibelhealth/NICU-Xamarin/_apis/build/status/NICU-Xamarin-Xamarin.iOS-CI)](https://dev.azure.com/developer0635/NICU-Xamarin/_build/latest?definitionId=1)

UHMS-Host is a Xamarin application that will be deployed as the *Advanced Neonatal Epidermal System (ANNE) Base Unit* for the NICU project. A Xamarin cross-platform application, with a deployment environment of iOS and planned deployment environemnts of Android and UWP. The current wireframes for the application can be found [here](https://sibelhealth-nicu-anne-ui.netlify.com/).

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. If you need more info or help, please refer to the [sibel general documentation](http://developer.sibelhealth.com/docs/) (or use the [mirror](https://sibelhealth-sibeldocs-mirror.netlify.com/)) or use the #xamarin channel in the JohnRogersGroup Slack. Feel free to send slack messages to the individual contributors of the project as necessary.


### Prerequisites

#### IDE

- [Visual Studio](https://visualstudio.microsoft.com/)

#### Nuget Packages

- Syncfusion - Add the nuget package source by following directions in this [link](https://help.syncfusion.com/uwp/nuget-packages).

### Project Structure

Using the MvvmCross framework, project follows Model-view-view-Model pattern. As in a standard Xamarin application, The view layer code is managed independently for each platform.

```
UHMS-Xamarin
│
├── UHMS.Core             The shared core class-platform code
│   ├── Models
│   ├── Services
│   └── ViewModels
├── UHMS.Core.Tests       Unit testing for the shared code
│
├──  UHMS.iOS_Left        Contains View code for the iOS Application
│   ├── Assets.xcassets   Asset catalogue for the iOS Application
│   ├── Properties
│   ├── Resources         View assets not managed by the iOS Asset Catalogue
│   └── Views             Contains View Controller code for the iOS UI view components
│       └── Helper        Contains View Controller code for sub view components
│
├──  UHMS.iOS_Bot
├──  UHMS.iOS_Android
└──  UHMS.iOS_UWP

```

## Deployment

The current application is still in active development and does not have a release version. To deploy to your local iOS device, please make sure that you have the developer signing setup correctly. If you choose to deploy to the iOS Simulator, please note that the bluetooth function will not work properly.

### iOS Device Deployment Instructions

#### OSX

1. Make sure that you have the latest version of Visual Studio (VS will prompt you for any new updates)and Xcode (use the AppStore to check this). Open up both application to go though any additional installation steps.
2. In Visual Studio, right click on the iOS deployment target in the Solution Explorer and click *Options*
3. *Under Build> iOS Bundle Signing*, make sure you have the signing identity setup
4. Check the `Info.Plist` file for the iOS View code. Confirm that the signing has been setup correctly.
5. Build and run the iOS project.

#### Windows

The current deployment environment does not support iOS deployment in windows. If you have access to mac device, but actively develop on a windows machine, you can follow these [instructions](https://docs.microsoft.com/en-us/xamarin/ios/get-started/installation/windows/connecting-to-mac/) to pair your windows machine to the mac.

## Built With

### Packages
- [MvvmCross](https://www.mvvmcross.com/) - A Model-View-ViewModel (MVVM) design pattern framework for the Xamarin ecosystem.
- [Syncfusion](https://help.syncfusion.com/) - Enterprise-grade UI components for each platforms.
- [Bluetooth LE plugin for Xamarin](https://github.com/xabre/xamarin-bluetooth-le) - Xamarin and MvvMCross plugin for accessing the bluetooth functionality
- [ACR User Dialogs](https://github.com/aritchie/userdialogs) - 
A cross platform library that allows you to call for standard user dialogs from a shared/portable library.

## Contributing

Please refer to the [contributing guidelines](http://developer.sibelhealth.com/docs/contributing/) (If that link does not work use the [mirror](https://sibelhealth-sibeldocs-mirror.netlify.com/)).

## Versioning
The current project is in an unreleased state. 

## Contributors

Sibel Health

- Jongyoon Lee (jongyoon.lee@sibelhealth.com)
- JooHee Lee (joohee.lee@sibelhealth.com)

UIUC Students

- Junbin Park
- Edward Kim
- Jason Hahm
- Dominic Grande
- Yerim Park