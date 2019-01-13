Source Control Generator Changelog
=======
# 1.3.0.3
* Divinity: Original Sin 2 - Definitive Edition Module
	* Projects that are imported from classic should now have the proper naming convention when exported with the pak generator (ModName_OldUUID_NewUUID). This is to stay consistent with the published Steam version.
	* Quick fix to remedy packages being locked after being created.

# 1.3.0.1
* Divinity: Original Sin 2 - Definitive Edition Module
	* Minor update to get the cancel button working properly for backup and packaging progress screens.

# 1.3.0.0
* Divinity: Original Sin 2 - Definitive Edition Module
	* Added a Package Publisher (via [lslib](https://github.com/Norbyte/lslib)) - Create paks from selected projects with the click of a button. Simply select some projects and click "Create Packages...". The paks will be created in your Definitive Edition Local Mods folder.
	* New shortcut for opening the Local Mods folder (File -> Open Local Mods Folder...)
	* Small fix to prevent thumbnails from being locked.
# 1.2.0.2
* Divinity: Original Sin 2 - Definitive Edition Module
	* Switched the modified/created dates for projects to the creation date of the project meta.lsx, and the modified date of the mod meta.lsx.
	* Made the refresh button above the project list refresh all, so it can be used to reload project data (F5 also has the same functionality).

# 1.2.0.1
* Small fix in the automatic data directory path created for the DOS2 DefEd module when running setup for the first time (+1 backslash).

# 1.2.0.0
* New Divinity: Original Sin 2 - Definitive Edition module.
	* Works separately from the previous DOS2 module, so to be sure to copy over relevant project files you'd like to preserve, such as your Readme, Changelog, .git folder, etc.
* New "Text Generator" Tool
	* Allows you to generate text with a series of keywords that are dynamically replaced, saving time when you need similar text with incremental values.
* Fixes and improvements in archival speed.
* Thumbnails and meta files should no longer get "locked".

# 1.1.1.0
* Backup Support
	* Tweaked backup process so it runs asynchronously (as was intended).
	* Projects 1 GB and beyond can now be archived. Note that this may still take a few minutes.
	* Added cancel functionality for the backup genertor. Canceled archives will still remain in the backup folder, and should contain all the files archived before canceling.
	
# 1.0.0.0
* Initial Release
