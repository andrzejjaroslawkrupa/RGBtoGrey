﻿using NUnit.Framework;
using RGBtoGrey.ViewModel;
using Moq;
using System;
using System.IO;
using System.Windows.Media.Imaging;
using ImgProcLib;
using RGBtoGrey.FileDialog;

namespace RGBtoGreyTests
{
	public class ConvertedImageTests
	{
		private readonly string _testFilesDirectory = TestContext.CurrentContext.TestDirectory + @"\\TestFiles\\testImage.jpg";
		private Mock<IImageProcessingAdapter> _imageProcessingMock;

		[TestFixture]
		public class ConvertImageTests : ConvertedImageTests
		{
			[SetUp]
			public void Setup()
			{
				_imageProcessingMock = new Mock<IImageProcessingAdapter>();
				Presenter.FilePath = _testFilesDirectory;
			}

			private ConvertedImageViewModel GetSut()
			{
				return new ConvertedImageViewModel
				{
					ImageProcessingAdapter = _imageProcessingMock.Object
				};
			}

			[Test]
			public void ConvertImage_ConvertCommandExecuted_ConvertImageUsedOnce()
			{
				var convertedImageViewModel = GetSut();

				convertedImageViewModel.ConvertCommand.Execute(null);

				_imageProcessingMock.Verify(m => m.ConvertImage(It.IsAny<string>()), Times.Once);
			}


			[Test]
			public void ConvertImage_ConvertCommandExecuted_ConvertedImageSet()
			{
				var bitmapImage = new BitmapImage(new Uri(_testFilesDirectory));
				_imageProcessingMock.Setup(m => m.ConvertImage(It.IsAny<string>()))
					.Returns(bitmapImage);
				var readConvertedImage = GetSut();
				var expected = ImageProcessing.GetBitmapPixels(bitmapImage);

				readConvertedImage.ConvertCommand.Execute(null);
				var actual = ImageProcessing.GetBitmapPixels(readConvertedImage.ConvertedImage);

				Assert.That(actual, Is.EqualTo(expected));
			}

			[Test]
			public void ConvertImage_ConvertCommandExecuted_ConversionTimeSet()
			{
				var convertedImageViewModel = GetSut();

				convertedImageViewModel.ConvertCommand.Execute(null);

				Assert.That(convertedImageViewModel.ConversionTime, Is.Not.EqualTo(null));
			}

			[Test]
			public void ConvertImage_ConvertCommandIsNotExecuted_IsImageConvertedSetToFalse()
			{
				var convertedImageViewModel = GetSut();

				Assert.That(convertedImageViewModel.IsImageConverted, Is.False);
			}

			[Test]
			public void ConvertImage_ConvertCommandExecuted_IsImageConvertedSetToTrue()
			{
				var convertedImageViewModel = GetSut();

				convertedImageViewModel.ConvertCommand.Execute(null);

				Assert.That(convertedImageViewModel.IsImageConverted, Is.True);
			}
		}

		[TestFixture]
		public class SaveAsTests : ConvertedImageTests
		{
			private readonly string _outputFileWithoutExt = TestContext.CurrentContext.TestDirectory + @"\\outputFile";
			private Mock<IFileDialog> _fileDialogMock;
			private Mock<IBitmapImageFileExporting> _bitmapFileExportingMock;

			[SetUp]
			public void Setup()
			{
				_imageProcessingMock = new Mock<IImageProcessingAdapter>();
				_fileDialogMock = new Mock<IFileDialog>();
				_bitmapFileExportingMock = new Mock<IBitmapImageFileExporting>();
			}

			private ConvertedImageViewModel GetSutWithExtension(string extension)
			{
				_fileDialogMock.Setup(m => m.FilePath).Returns(_outputFileWithoutExt + extension);
				Presenter.FilePath = _outputFileWithoutExt + extension;
				var convertedImageViewModel = new ConvertedImageViewModel
				{
					ImageProcessingAdapter = _imageProcessingMock.Object,
					FileDialog = _fileDialogMock.Object,
					BitmapImageFileExporting = _bitmapFileExportingMock.Object
				};
				return convertedImageViewModel;
			}

			[Test]
			public void SaveAs_SaveAsCommandExecuted_ImageSavedAsJPG()
			{
				var convertedImageViewModel = GetSutWithExtension(".jpg");

				convertedImageViewModel.ConvertCommand.Execute(null);
				convertedImageViewModel.SaveAsCommand.Execute(null);

				_bitmapFileExportingMock.Verify(
				m => m.ExportImageAsFile(It.IsAny<BitmapImage>(), ImageFileFormats.jpg, It.IsAny<string>()), Times.Once);
			}

			[Test]
			public void SaveAs_SaveAsCommandExecuted_ImageSavedAsPNG()
			{
				var convertedImageViewModel = GetSutWithExtension(".png");

				convertedImageViewModel.ConvertCommand.Execute(null);
				convertedImageViewModel.SaveAsCommand.Execute(null);

				_bitmapFileExportingMock.Verify(
				m => m.ExportImageAsFile(It.IsAny<BitmapImage>(), ImageFileFormats.png, It.IsAny<string>()), Times.Once);
			}

			[Test]
			public void SaveAs_SaveAsCommandExecuted_ImageSavedAsBMP()
			{
				var convertedImageViewModel = GetSutWithExtension(".bmp");

				convertedImageViewModel.ConvertCommand.Execute(null);
				convertedImageViewModel.SaveAsCommand.Execute(null);

				_bitmapFileExportingMock.Verify(
				m => m.ExportImageAsFile(It.IsAny<BitmapImage>(), ImageFileFormats.bmp, It.IsAny<string>()), Times.Once);
			}

			[Test]
			public void SaveAs_SaveAsCommandExecutedWithoutOutputPathSet_DoNothing()
			{
				_fileDialogMock.Setup(m => m.FilePath).Returns<string>(null);
				var convertedImageViewModel = new ConvertedImageViewModel
				{
					ImageProcessingAdapter = _imageProcessingMock.Object,
					FileDialog = _fileDialogMock.Object,
					BitmapImageFileExporting = _bitmapFileExportingMock.Object
				};

				convertedImageViewModel.ConvertCommand.Execute(null);
				convertedImageViewModel.SaveAsCommand.Execute(null);

				_bitmapFileExportingMock.Verify(
				m => m.ExportImageAsFile(It.IsAny<BitmapImage>(), It.IsAny<ImageFileFormats>(), It.IsAny<string>()), Times.Never);
			}

			[Test]
			public void SaveAs_SaveAsCommandExecutedWithInvalidOutputPathSet_ExceptionThrown()
			{
				var convertedImageViewModel = GetSutWithExtension("");

				convertedImageViewModel.ConvertCommand.Execute(null);

				Assert.That(() => convertedImageViewModel.SaveAsCommand.Execute(null), Throws.Exception.TypeOf<FileFormatException>());
			}
		}
	}
}