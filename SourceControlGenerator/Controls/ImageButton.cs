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

		public event PropertyChangedEventHandler PropertyChanged;

		public void RaisePropertyChanged(string propertyName)
		{
			OnPropertyChanged(propertyName);
		}

		private void OnPropertyChanged(String property)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;

			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(property);
				handler(this, e);
			}
		}

		private WriteableBitmap buttonImage;

		public WriteableBitmap ButtonImage
		{
			get { return buttonImage; }
			set
			{
				buttonImage = value;
				RaisePropertyChanged("ButtonImage");
			}
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
			if (sender is ImageButton imageButton && !string.IsNullOrEmpty(imageButton.Source))
			{
				imageButton.ButtonImage = new WriteableBitmap(new BitmapImage(new Uri(imageButton.Source, UriKind.RelativeOrAbsolute)));
			}
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

		public string Tooltip_Enabled
		{
			get { return (string)GetValue(Tooltip_EnabledProperty); }
			set { SetValue(Tooltip_EnabledProperty, value); }
		}

		public static readonly DependencyProperty Tooltip_EnabledProperty =
			DependencyProperty.Register("Tooltip_Enabled", typeof(string), typeof(ImageButton), new PropertyMetadata(""));

		public string Tooltip_Disabled
		{
			get { return (string)GetValue(Tooltip_DisabledProperty); }
			set { SetValue(Tooltip_DisabledProperty, value); }
		}

		public static readonly DependencyProperty Tooltip_DisabledProperty =
			DependencyProperty.Register("Tooltip_Disabled", typeof(string), typeof(ImageButton), new PropertyMetadata(""));


		public ImageButton() : base()
		{
			IsEnabledChanged += OnEnabledChanged;
		}

		private void OnEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if(ButtonImage != null)
			{
				if(IsEnabled)
				{
					ButtonImage = new WriteableBitmap(new BitmapImage(new Uri(Source, UriKind.RelativeOrAbsolute)));

					foreach(var image in this.FindVisualChildren<Image>())
					{
						image.Source = null;
						image.Source = ButtonImage;
					}
				}
				else
				{
					byte[] orgPixels = new byte[ButtonImage.PixelHeight * ButtonImage.PixelWidth * 4];
					byte[] newPixels = new byte[orgPixels.Length];
					ButtonImage.CopyPixels(orgPixels, ButtonImage.PixelWidth * 4, 0);

					for (int i = 3; i < orgPixels.Length; i += 4)
					{
						int grayVal = ((int)orgPixels[i - 3] + (int)orgPixels[i - 2] + (int)orgPixels[i - 1]);
						if (grayVal != 0) grayVal = grayVal / 3;
						newPixels[i] = orgPixels[i]; //Set AlphaChannel
						newPixels[i - 3] = (byte)grayVal;
						newPixels[i - 2] = (byte)grayVal;
						newPixels[i - 1] = (byte)grayVal;
					}

					ButtonImage.WritePixels(new Int32Rect(0, 0, ButtonImage.PixelWidth, ButtonImage.PixelHeight), newPixels, ButtonImage.PixelWidth * 4, 0);
				}
				
			}
		}
	}
}
