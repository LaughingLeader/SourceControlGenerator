using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.SCG.Core
{
	static public class TooltipText
	{
		//Git
		static public string GitAuthor => "The name to use for git commits. Optional.";
		static public string GitEmail => "The email address to use for git commits. Optional.";

		static public string GitIgnore => ".gitignore files are used to prevent certain files from being tracked.\nThis is useful for preventing large asset or binary files from slowing down the git source control.";
		static public string GitAttributes => "This file is used to give attributes to pathnames.\nBy default, ignore attributes are added to the git-related files, so they are ignored when creating backups.";
		static public string Readme => "A basic readme file, detailing relevant information about the project.\nDisplays on the repository main page.";
		static public string Changelog => "A changelog file with details on each update.";
		static public string CustomLicense => "A custom license to use instead of the more common three (MIT, Apache, GNU).";
		static public string TemplateKeywords => "Words that get replaced when generating templates.";
		static public string Generation_Templates => "These files are created when generating the repository files.\nKeywords will be replaced with the appropriate values.";
		static public string Generation_License => "Select a license to generate.";
		static public string Generation_Confirm => "Generate git repository files for selected projects.";
		static public string Generation_Cancel => "Cancel git repository creation.";

		//Log
		static public string Log_Button_SearchClear => "Clear Search";
		static public string Log_Checkbox_Activity => "Show Activity Logs";
		static public string Log_Checkbox_Important => "Show Important Logs";
		static public string Log_Checkbox_Warning => "Show Warning Logs";
		static public string Log_Checkbox_Error => "Show Error Logs";

		//Settings
		static public string DOS2DataDirectory => "The Divinity: Original Sin 2 data directory. This is used to scan for mods to add.";
		static public string GitDetection => "Git is used to create repositories, enabling you to track and backup changes in your projects.";
		static public string GitInstalled => "Git is installed. You're good to go!";
		static public string GitNotInstalled => "Git is not installed. You won't be able to create git repositories!";
		static public string ProjectRootDirectory => "The root directory where git projects will be stored. This directory will be created if it does not exist already.";
		static public string BackupRootDirectory => "The root directory where project backups will be stored. This directory will be created if it does not exist already.";
	}
}
