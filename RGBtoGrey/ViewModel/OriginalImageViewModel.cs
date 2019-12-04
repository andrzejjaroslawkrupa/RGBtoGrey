﻿using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Prism.Commands;
using RGBtoGrey.FileDialog;
using RGBtoGrey.Helpers;

namespace RGBtoGrey.ViewModel
{
	public class OriginalImageViewModel : ObservableObject
	{
		private string _filename;
		private BitmapImage _originalImage;

		public string Filename
		{
			get => _filename;
			private set
			{
				_filename = value;
				OnPropertyChanged(() => Filename);
			}
		}

		public BitmapImage OriginalImage
		{
			get => _originalImage;
			private set
			{
				_originalImage = value;
				OnPropertyChanged(() => OriginalImage);
			}
		}

		public IFileDialog FileDialog { get; set; } = new FileDialog.FileDialog(new Microsoft.Win32.OpenFileDialog());

		public ICommand OpenFileDialogCommand => new DelegateCommand(ShowOpenFileDialog);

		private void ShowOpenFileDialog()
		{
			if (FileDialog.ShowDialog() != true || ChangeImageFromPath(FileDialog.FilePath) != true) return;
			ChangeFilenameFromPath(FileDialog.FilePath);
			Presenter.FilePath = FileDialog.FilePath;
		}

		private void ChangeFilenameFromPath(string path)
		{
			if (path == null) return;
			try
			{
				var directorySplit = path.Split('\\');
				Filename = directorySplit[directorySplit.Length - 1];
			}
			catch (Exception e)
			{
				throw new ApplicationException("ChangeFilename: ", e);
			}
		}

		private bool? ChangeImageFromPath(string path)
		{
			if (path == null) return false;
			try
			{
				OriginalImage = new BitmapImage(new Uri(path));
				return true;
			}
			catch (Exception e)
			{
				throw new ApplicationException("ChangeImage: ", e);
			}
		}
	}
}