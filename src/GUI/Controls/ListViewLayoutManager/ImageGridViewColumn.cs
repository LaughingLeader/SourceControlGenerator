// -- FILE ------------------------------------------------------------------
// name       : ImageGridViewColumn.cs
// created    : Jani Giannoudis - 2008.03.27
// language   : c#
// environment: .NET 3.0
// copyright  : (c) 2008-2012 by Itenso GmbH, Switzerland
// --------------------------------------------------------------------------
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Itenso.Windows.Controls.ListViewLayout;


// ------------------------------------------------------------------------
public abstract class ImageGridViewColumn : GridViewColumn, IValueConverter
{

	// ----------------------------------------------------------------------
	protected ImageGridViewColumn() :
		this(Stretch.None)
	{
	} // ImageGridViewColumn

	// ----------------------------------------------------------------------
	protected ImageGridViewColumn(Stretch imageStretch)
	{
		var imageElement = new FrameworkElementFactory(typeof(Image));

		// image source
		var imageSourceBinding = new Binding
		{
			Converter = this,
			Mode = BindingMode.OneWay
		};
		imageElement.SetBinding(Image.SourceProperty, imageSourceBinding);

		// image stretching
		var imageStretchBinding = new Binding
		{
			Source = imageStretch
		};
		imageElement.SetBinding(Image.StretchProperty, imageStretchBinding);

		var template = new DataTemplate
		{
			VisualTree = imageElement
		};
		CellTemplate = template;
	} // ImageGridViewColumn

	// ----------------------------------------------------------------------
	protected abstract ImageSource GetImageSource(object value);

	// ----------------------------------------------------------------------
	object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return GetImageSource(value);
	} // Convert

	// ----------------------------------------------------------------------
	object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	} // ConvertBack

} // class ImageGridViewColumn

// namespace Itenso.Windows.Controls.ListViewLayout
// -- EOF -------------------------------------------------------------------
