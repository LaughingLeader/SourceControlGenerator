using System.ComponentModel;

namespace SCG
{
	public enum BackupMode
	{
		[Description("Creates an archive by adding all of the project's folders, including the git repo folder (if it exists).")]
		Zip,
		[Description("If the project has a git repository, an archive is created by calling the git archive command. This method is faster, but doesn't include ignored files.")]
		GitArchive
	}
}