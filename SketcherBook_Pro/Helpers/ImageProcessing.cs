using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace SketcherBook_Pro.Helpers
{
    public sealed class ImageProcessing
    {
        private MainPage _mainPage;
        private StorageFile imageFile;

        public ImageProcessing(MainPage mainPage)
        {
            _mainPage = mainPage;
        }

        public async void OnSaveAsync(object sender, RoutedEventArgs e)
        {
            if (_mainPage.InkCanvas_Main.InkPresenter.StrokeContainer.GetStrokes().Count > 0)
            {
                var savePicker = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                    
                };

                savePicker.FileTypeChoices.Add(ResourceLoader.GetForViewIndependentUse().GetString("APPNAME_SHORT"), new List<string>
                {
                    ResourceLoader.GetForViewIndependentUse().GetString("FILE_EXTENSION"),

                });

                savePicker.FileTypeChoices.Add(ResourceLoader.GetForViewIndependentUse().GetString("ACTION_SAVE_IMAGE_AS"), new List<string>
                {
                    ResourceLoader.GetForViewIndependentUse().GetString("FILE_EXTENSION_PNG"),

                });

                var file = await savePicker.PickSaveFileAsync();
                if (null != file)
                {
                    try
                    {
                        using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            await _mainPage.InkCanvas_Main.InkPresenter.StrokeContainer.SaveAsync(stream);

                        }

                        var messageDialog =
                            new MessageDialog(
                                ResourceLoader.GetForViewIndependentUse().GetString("MESSAGEBOX_ACTION_SAVE_COMPLETE"));

                        messageDialog.Commands.Add(new UICommand(
                            ResourceLoader.GetForViewIndependentUse().GetString("MESSAGEBOX_ACTION_SHARE"),  command =>
                            {
                                DataTransferManager.ShowShareUI();
                            }));

                        messageDialog.Commands.Add(new UICommand(
                            ResourceLoader.GetForViewIndependentUse().GetString("MESSAGEBOX_ACTION_CLOSE")));

                        messageDialog.CancelCommandIndex = 1;
                        messageDialog.DefaultCommandIndex = 0;
                        await messageDialog.ShowAsync();
                    }
                    catch (Exception)
                    {
                        await
                            new MessageDialog(
                                ResourceLoader.GetForViewIndependentUse().GetString("MESSAGEBOX_ACTION_SAVE_FAILED"))
                                .ShowAsync();
                    }
                }
            }
            else
            {
                await
                    new MessageDialog(
                        ResourceLoader.GetForViewIndependentUse().GetString("MESSAGEBOX_ACTION_SAVE_NOINK"))
                        .ShowAsync();
            }
        }

        public async void OnLoadAsync(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(ResourceLoader.GetForViewIndependentUse().GetString("FILE_EXTENSION"));
            openPicker.FileTypeFilter.Add(ResourceLoader.GetForViewIndependentUse().GetString("FILE_EXTENSION_PNG"));
            var file = await openPicker.PickSingleFileAsync();
            if (null != file)
            {
                using (var stream = await file.OpenSequentialReadAsync())
                {
                    try
                    {
                        await _mainPage.InkCanvas_Main.InkPresenter.StrokeContainer.LoadAsync(stream);
                    }
                    catch (Exception ex)
                    {
                        await
                            new MessageDialog(
                                ResourceLoader.GetForViewIndependentUse().GetString("MESSAGEBOX_ACTION_LOAD_FAILED") ).ShowAsync();
                    }
                }
            }
        }

        private DataTransferManager dataTransferManager;

        private void DataTransfer()
        {
            // Register the current page as a share source.
            dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += OnDataRequested;
        }

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            // Call the scenario specific function to populate the datapackage with the data to be shared.
            if (GetShareContent(e.Request))
            {
                // Out of the datapackage properties, the title is required. If the scenario completed successfully, we need
                // to make sure the title is valid since the sample scenario gets the title from the user.
                if (String.IsNullOrEmpty(e.Request.Data.Properties.Title))
                {
                    e.Request.FailWithDisplayText("Banter");
                }
            }
        }

        private bool GetShareContent(DataRequest request)
        {

            var succeeded = false;

            if (imageFile != null)
            {
                var requestData = request.Data;
                requestData.Properties.Title = "Title";
                requestData.Properties.Description = "Description"; // The description is optional.


                // It's recommended to use both SetBitmap and SetStorageItems for sharing a single image
                // since the target app may only support one or the other.

                var imageItems = new List<IStorageItem> {imageFile};
                requestData.SetStorageItems(imageItems);

                var imageStreamRef = RandomAccessStreamReference.CreateFromFile(imageFile);
                requestData.Properties.Thumbnail = imageStreamRef;
                requestData.SetBitmap(imageStreamRef);
                succeeded = true;
            }
            else
            {
                request.FailWithDisplayText("Select an image you would like to share and try again.");
            }
            return succeeded;
        }
    }
}