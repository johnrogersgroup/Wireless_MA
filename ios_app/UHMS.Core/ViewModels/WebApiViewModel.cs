using System;
using System.Threading.Tasks;
using MvvmCross.Commands;

namespace UHMS.Core.ViewModels {
    #region Web API
    /// <summary>
    /// Partial class of <c>MainViewModel.</c>.
    /// </summary>
    /// <remarks>
    /// Includes implementation of communication with web api server.
    /// </remarks>
    public partial class MainViewModel : BaseViewModel {
        private MvxCommand _logIn;
        private readonly string _defaultBucketName = "johnrogerstestbucket";


        public async Task UploadFile() {
            // TODO: NICU-117 : Can sync data to the cloud server by uploading a file

            ///Removed for iOS. Need reimplementation.

            //await FileManagementModel.SaveBufferedDataAsync();
            //if (_userTokens == null) { _userDialogs.Alert("Authentication is required. Please Log In.", "Log In Required"); return; }
            //var zipTask = Task.Run(() => { return FileManagementModel.MakeZip(); });

            //var deviceInformation = new EasClientDeviceInformation();
            //string deviceID = deviceInformation.Id.ToString();

            //var fetchPresignedUrlTask = ServerCommunicationModel.FetchPresignedURL(_userTokens, _defaultBucketName, deviceID,
            //  "geoffrey", "cogle", "test_file_upload_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ".zip");

            //var uploadFolderpath = await zipTask;
            //var presignedURL = await fetchPresignedUrlTask;

            //if (uploadFolderpath == "") { return; }

            //Stream stream = File.Open(uploadFolderpath, FileMode.Open);

            //var success = await ServerCommunicationModel.ApiUpload(stream, presignedURL);

            //if (success) {
            //  _userDialogs.Alert("Succeeded.", "Done");
            //} else {
            //  _userDialogs.Alert("Failed.", "Ok");
            //}
        }

        public IMvxCommand ZipFiles => new MvxCommand(async () => { await UploadFile(); });

    }
    #endregion

}
