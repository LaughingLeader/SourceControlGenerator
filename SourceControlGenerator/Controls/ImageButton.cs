using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SCG.Data;
using SCG.Extensions;

namespace SCG.Controls
{
	public class ImageButton : Button
	{
		/// <summary>
		/// Constructor for ImageButton
		/// </summary>
		static ImageButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(typeof(ImageButton)));
		}

		public string Source
		{
			get
			{
				return (string)GetValue(ImageProperty);
			}
			set
			{
				SetValue(ImageProperty, value);
			}
		}

		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Source", typeof(string), typeof(ImageButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, OnSourceChanged));

		private static void OnSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			//if (sender is ImageButton imageButton && !string.IsNullOrEmpty(imageButton.Source))
			//{
			//	imageButton.ButtonImage = new WriteableBitmap(new BitmapImage(new Uri(imageButton.Source, UriKind.RelativeOrAbsolute)));
			//	imageButton.CreateGreyImage();
			//	imageButton.CreateHoverImage();
			//}
		}

		public int MaxSize
		{
			get { return (int)GetValue(SizeProperty); }
			set { SetValue(SizeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Size.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SizeProperty =
			DependencyProperty.Register("MaxSize", typeof(int), typeof(ImageButton), new PropertyMetadata(0, OnSizeChanged));

		private static void OnSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is ImageButton imageButton)
			{
				imageButton.MinWidth = imageButton.MaxSize;
				imageButton.MaxWidth = imageButton.MaxSize;
				imageButton.MinHeight = imageButton.MaxSize;
				imageButton.MaxHeight = imageButton.MaxSize;
			}
		}

		public string ToolTip_Enabled
		{
			get { return (string)GetValue(ToolTip_EnabledProperty); }
			set { SetValue(ToolTip_EnabledProperty, value); }
		}

		public static readonly DependencyProperty ToolTip_EnabledProperty =
			DependencyProperty.Register("ToolTip_Enabled", typeof(string), typeof(ImageButton), new PropertyMetadata(null));

		public string ToolTip_Disabled
		{
			get { return (string)GetValue(ToolTip_DisabledProperty); }
			set { SetValue(ToolTip_DisabledProperty, value); }
		}

		public static readonly DependencyProperty ToolTip_DisabledProperty =
			DependencyProperty.Register("ToolTip_Disabled", typeof(string), typeof(ImageButton), new PropertyMetadata(null));



		public Color TintColor
		{
			get { return (Color)GetValue(TintColorProperty); }
			set { SetValue(TintColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TintColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TintColorProperty =
			DependencyProperty.Register("TintColor", typeof(Color), typeof(ImageButton), new PropertyMetadata(Colors.LightBlue));

		public ImageButton() : base()
		{
			
		}

		//public void CreateGreyImage()
		//{
		//	GreyImage = new WriteableBitmap(new BitmapImage(new Uri(Source, UriKind.RelativeOrAbsolute)));

		//	byte[] orgPixels = new byte[GreyImage.PixelHeight * GreyImage.PixelWidth * 4];
		//	byte[] newPixels = new byte[orgPixels.Length];
		//	GreyImage.CopyPixels(orgPixels, GreyImage.PixelWidth * 4, 0);

		//	for (int i = 3; i < orgPixels.Length; i += 4)
		//	{
		//		int grayVal = ((int)orgPixels[i - 3] + (int)orgPixels[i - 2] + (int)orgPixels[i - 1]);
		//		if (grayVal != 0) grayVal = grayVal / 3;
		//		newPixels[i] = orgPixels[i]; //Set AlphaChannel
		//		newPixels[i - 3] = (byte)grayVal;
		//		newPixels[i - 2] = (byte)grayVal;
		//		newPixels[i - 1] = (byte)grayVal;
		//	}

		//	GreyImage.WritePixels(new Int32Rect(0, 0, GreyImage.PixelWidth, GreyImage.PixelHeight), newPixels, GreyImage.PixelWidth * 4, 0);
		//}
	}
}
