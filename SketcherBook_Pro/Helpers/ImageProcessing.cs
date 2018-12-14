using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace SketcherBook_Pro.Helpers
{
	public sealed class ImageProcessing
	{
		public ImageProcessing()
		{
		}

		public async void OnSaveAsync(InkStrokeContainer inkStrokeContainer)
		{
			if (inkStrokeContainer.GetStrokes().Count > 0)
			{
				FileSavePicker savePicker = new FileSavePicker
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

				StorageFile file = await savePicker.PickSaveFileAsync();
				if (null != file)
				{
					try
					{
						using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
						{
							await inkStrokeContainer.SaveAsync(stream);
						}

						MessageDialog messageDialog = new MessageDialog(ResourceLoader.GetForViewIndependentUse().GetString("MESSAGEBOX_ACTION_SAVE_COMPLETE"));

						messageDialog.Commands.Add(new UICommand(
							ResourceLoader.GetForViewIndependentUse().GetString("MESSAGEBOX_ACTION_SHARE"),
							command =>
							{
								DataTransferManager.ShowShareUI();
							})
						);

						messageDialog.Commands.Add(new UICommand(ResourceLoader.GetForViewIndependentUse().GetString("MESSAGEBOX_ACTION_CLOSE")));

						messageDialog.CancelCommandIndex = 1;
						messageDialog.DefaultCommandIndex = 0;
						await messageDialog.ShowAsync();
					}
					catch (Exception)
					{
						await new MessageDialog(ResourceLoader.GetForViewIndependentUse().GetString("MESSAGEBOX_ACTION_SAVE_FAILED")).ShowAsync();
					}
				}
			}
			else
			{
				await new MessageDialog(ResourceLoader.GetForViewIndependentUse().GetString("MESSAGEBOX_ACTION_SAVE_NOINK")).ShowAsync();
			}
		}

		public async void OnLoadAsync(object sender, RoutedEventArgs e)
		{
			//FileOpenPicker fileOpenPicker = new FileOpenPicker
			//{
			//	SuggestedStartLocation = PickerLocationId.PicturesLibrary
			//};
			//fileOpenPicker.FileTypeFilter.Add(ResourceLoader.GetForViewIndependentUse().GetString("FILE_EXTENSION"));
			//fileOpenPicker.FileTypeFilter.Add(ResourceLoader.GetForViewIndependentUse().GetString("FILE_EXTENSION_PNG"));
			//StorageFile storageFile = await fileOpenPicker.PickSingleFileAsync();

			//if (null != storageFile)
			//{
			//	using (IInputStream inputStream = await storageFile.OpenSequentialReadAsync())
			//	{
			//		try
			//		{
			//			await mainPage.InkCanvas.InkPresenter.StrokeContainer.LoadAsync(inputStream);
			//		}
			//		catch (Exception)
			//		{
			//			await new MessageDialog(ResourceLoader.GetForViewIndependentUse().GetString("MESSAGEBOX_ACTION_LOAD_FAILED")).ShowAsync();
			//		}
			//	}
			//}
		}
	}
}