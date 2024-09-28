namespace SCG.Modules.DOS2DE.Core;
internal static class Resources
{
	public static readonly string DefaultGitAttributes = "";
	public static readonly string DefaultGitIgnore = @"# SourceControlGenerator #
##########################
SourceControlGenerator.json

# Levels
######################
Mods/$ModFolderName/Levels/*
Mods/$ModFolderName/Globals/*
Editor/Mods/$ModFolderName/Levels/*

# Story files        #
######################
*.div
*.raw
story_orphanqueries_ignore.txt
Mods/$ModFolderName/Story/Dialogs/Autosave/*
Mods/$ModFolderName/Story/Dialogs/Recovered/*

# Asset Folders      #
######################
Public/$ModFolderName/Assets/*
#Public/$ModFolderName/GUI/*

# Editor files       #
######################
RootTemplateSelectorConfig_*
ResourceSelectorConfig_*
StatsEditorConfig.xml
EffectEditorConfig.xml
DockPanel.config
EditorBrowserMetadata_*
EditorBrowserMetadata.xml
Autosave/*
Recovered/*
ReConHistory.txt
story.debugInfo

# Assets             #
######################
*.gr2
*.dae
*.swf
*.bullet
*.dds
*.tga
*.png
*.ttf
*.lsb
*.lsf
*.lsfx
*.lsefx
#*.lsx
*.lsmg

# Logs               #
######################
*.log
*.ailog
log.txt
errors.txt
dialoglog.txt
personallog.txt

# Binary Files       #
######################
*.dat
*.data
*.bik
*.bnk
*.bshd
*.cur
*.fnt
*.iggy
*.iggytex
*.osi
*.patch
*.tmpl

# Dump files         #
######################
*.stackdump
*.dmp

# Compiled source #
###################
*.com
*.class
*.dll
*.exe
*.o
*.so

# Packages #
############
*.7z
*.dmg
*.gz
*.iso
*.jar
*.rar
*.tar
*.zip

# VS Code #
############
launch.json

# OS generated files #
######################
.DS_Store
.DS_Store?
._*
.Spotlight-V100
.Trashes
ehthumbs.db
Thumbs.db
*.lnk";
	public static readonly string DefaultReadme = @"$ProjectName for $ModuleName
=======

# Features:

# Releases
* [Steam Workshop]() 
* [Nexus]()

# Attribution
- [Divinity: Original Sin 2](http://store.steampowered.com/app/435150/Divinity_Original_Sin_2/), a game by [Larian Studios](http://larian.com/)";

	public static readonly string DefaultChangelog = @"$ProjectName Changelog
=======
# $Version
* Initial Release";

	public static readonly string DirectoryLayout = @"### Keywords ###
# ProjectName = The name of the project, without the GUID.
# ProjectFolder = The folder name of the project. Typically just the project name, though imported projects may differ.
# ModUUID = The UUID of the mod.
# ModFolder = The folder value for the mod. Typically ProjectName_ModUUID

### Directories to use for junctions ###

Editor/Mods/ModFolder
Mods/ModFolder
Projects/ProjectFolder
Public/ModFolder";

	public static readonly string DefaultLocaleResource = @"<?xml version=""1.0"" encoding=""utf-8""?>
<save>
	<header version=""2"" time=""0"" />
	<version major=""3"" minor=""6"" revision=""4"" build=""0"" />
	<region id=""TranslatedStringKeys"">
		<node id=""root"">
			<children>
				<node id=""TranslatedStringKey"">
					<attribute id=""Content"" value="""" type=""28"" handle=""ls::TranslatedStringRepository::s_HandleUnknown"" />
					<attribute id=""ExtraData"" value="""" type=""23"" />
					<attribute id=""Speaker"" value="""" type=""22"" />
					<attribute id=""Stub"" value=""True"" type=""19"" />
					<attribute id=""UUID"" value=""NewKey"" type=""22"" />
				</node>
			</children>
		</node>
	</region>
</save>";
}
