namespace SCG.Commands;

public class OpenFileCommand : BaseCommand
{
	private readonly Action<string> onLoad;

	public override bool CanExecute(object parameter)
	{
		if (base.CanExecute(parameter) && parameter != null && FileCommands.Load != null && parameter is string filePath)
		{
			return FileCommands.IsValidPath(filePath);
		}

		return false;
	}

	public override void Execute(object parameter)
	{
		var filePath = (String)parameter;

		if (!String.IsNullOrEmpty(filePath) && File.Exists(filePath))
		{
			Log.Here().Important("Attempting to open file: {0}", filePath);
			try
			{
				onLoad?.Invoke(File.ReadAllText(filePath));
			}
			catch (Exception ex)
			{
				Log.Here().Error("Error opening file {0}: {1}", filePath, ex.ToString());
			}
		}
	}

	public OpenFileCommand(Action<string> OnLoad)
	{
		onLoad = OnLoad;
	}
}
