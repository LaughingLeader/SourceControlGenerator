using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SCG.Data;
using SCG.Extensions;
using ReactiveUI;

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

			ToolTipService.ShowOnDisabledProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(true));
		}

		public string Source
		{
			get
			{
				return (string)GetValue(SourceImageProperty);
			}
			set
			{
				SetValue(SourceImageProperty, value);
			}
		}

		public static readonly DependencyProperty SourceImageProperty = DependencyProperty.Register("Source", typeof(string), typeof(ImageButton), 
			new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, OnSourceChanged));

		private static void OnSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			//if (sender is ImageButton imageButton && !string.IsNullOrEmpty(imageButton.Source))
			//{
			//	imageButton.ButtonImage = new WriteableBitmap(new BitmapImage(new Uri(imageButton.Source, UriKind.RelativeOrAbsolute)));
			//	imageButton.CreateGreyImage();
			//	imageButton.CreateHoverImage();
			//}
		}

		public string Source_Disabled
		{
			get
			{
				return (string)GetValue(SourceDisabledImageProperty);
			}
			set
			{
				SetValue(SourceDisabledImageProperty, value);
			}
		}

		public static readonly DependencyProperty SourceDisabledImageProperty = DependencyProperty.Register("Source_Disabled", typeof(string), typeof(ImageButton),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null));

		public int MaxSize
		{
			get { return (int)GetValue(MaxSizeProperty); }
			set { SetValue(MaxSizeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Size.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MaxSizeProperty =
			DependencyProperty.Register("MaxSize", typeof(int), typeof(ImageButton), new PropertyMetadata(16, OnSizeChanged));

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

		public int InitialSize
		{
			get { return (int)GetValue(InitialSizeProperty); }
			set { SetValue(InitialSizeProperty, value); }
		}

		public static readonly DependencyProperty InitialSizeProperty =
			DependencyProperty.Register("InitialSize", typeof(int), typeof(ImageButton), new PropertyMetadata(16, OnInitialSizeChanged));

		private static void OnInitialSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is ImageButton imageButton)
			{
				imageButton.Width = imageButton.InitialSize;
				imageButton.Height = imageButton.InitialSize;
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

		public static readonly DependencyProperty TintColorProperty =
			DependencyProperty.Register("TintColor", typeof(Color), typeof(ImageButton), new PropertyMetadata(Colors.LightBlue));

		/// <summary>
		/// Allows bypassing the fact that IsMouseOver stops updating when the button is disabled.
		/// Bind this property to a parent control, such as a ContentControl.
		/// </summary>
		public bool IsHovered
		{
			get { return (bool)GetValue(IsHoveredProperty); }
			set { SetValue(IsHoveredProperty, value); }
		}

		public static readonly DependencyProperty IsHoveredProperty =
			DependencyProperty.Register("IsHovered", typeof(bool), typeof(ImageButton), new PropertyMetadata(false));

		public ImageButton() : base()
		{
			this.DefaultStyleKey = typeof(ImageButton);

			IsMouseDirectlyOverChanged += ImageButton_IsMouseDirectlyOverChanged;
		}

		private void ImageButton_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			IsHovered = (bool)e.NewValue;
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
