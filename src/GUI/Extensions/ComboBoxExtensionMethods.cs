using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SCG;

public static class ComboBoxExtensionMethods
{
	public static void SetWidthFromItems(this ComboBox comboBox)
	{
		double comboBoxWidth = 19;// comboBox.DesiredSize.Width;

		// Create the peer and provider to expand the comboBox in code behind. 
		var peer = new ComboBoxAutomationPeer(comboBox);
		var provider = (IExpandCollapseProvider)peer.GetPattern(PatternInterface.ExpandCollapse);
		EventHandler eventHandler = null;
		eventHandler = new EventHandler(delegate
		{
			if (comboBox.IsDropDownOpen &&
				comboBox.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{
				double width = 0;
				foreach (var item in comboBox.Items)
				{
					var comboBoxItem = comboBox.ItemContainerGenerator.ContainerFromItem(item) as ComboBoxItem;
					comboBoxItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
					if (comboBoxItem.DesiredSize.Width > width)
					{
						width = comboBoxItem.DesiredSize.Width;
					}
				}
				comboBox.Width = comboBoxWidth + width;
				// Remove the event handler. 
				comboBox.ItemContainerGenerator.StatusChanged -= eventHandler;
				comboBox.DropDownOpened -= eventHandler;
				provider.Collapse();
			}
		});
		comboBox.ItemContainerGenerator.StatusChanged += eventHandler;
		comboBox.DropDownOpened += eventHandler;
		// Expand the comboBox to generate all its ComboBoxItem's. 
		provider.Expand();
	}
}
