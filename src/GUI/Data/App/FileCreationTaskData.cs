namespace SCG.Data.App;

public enum FileCreationTaskResult
{
	None,
	Success,
	Skipped,
	Error
}

public struct FileCreationTaskData
{
	public string TargetPath { get; set; }
	public FileCreationTaskResult Result { get; set; }
	public string ID { get; set; }
}
