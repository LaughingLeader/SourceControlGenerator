﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SCG.Modules.DOS2DE.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SCG.Modules.DOS2DE.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .
        /// </summary>
        public static string DefaultAttributes {
            get {
                return ResourceManager.GetString("DefaultAttributes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to $ProjectName Changelog
        ///=======
        ///# $Version
        ///* Initial Release.
        /// </summary>
        public static string DefaultChangelog {
            get {
                return ResourceManager.GetString("DefaultChangelog", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to # SourceControlGenerator #
        ///##########################
        ///SourceControlGenerator.json
        ///
        ///# Levels
        ///######################
        ///Mods/$ModFolderName/Levels/*
        ///Mods/$ModFolderName/Globals/*
        ///Editor/Mods/$ModFolderName/Levels/*
        ///
        ///# Story files        #
        ///######################
        ///*.div
        ///*.raw
        ///story_orphanqueries_ignore.txt
        ///Mods/$ModFolderName/Story/Dialogs/Autosave/*
        ///Mods/$ModFolderName/Story/Dialogs/Recovered/*
        ///
        ///# Asset Folders      #
        ///######################
        ///Public/$ModFolderName/Assets/*
        ///#Public/$ModFolderName [rest of string was truncated]&quot;;.
        /// </summary>
        public static string DefaultGitIgnore {
            get {
                return ResourceManager.GetString("DefaultGitIgnore", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;save&gt;
        ///	&lt;header version=&quot;2&quot; time=&quot;0&quot; /&gt;
        ///	&lt;version major=&quot;3&quot; minor=&quot;6&quot; revision=&quot;4&quot; build=&quot;0&quot; /&gt;
        ///	&lt;region id=&quot;TranslatedStringKeys&quot;&gt;
        ///		&lt;node id=&quot;root&quot;&gt;
        ///			&lt;children&gt;
        ///				&lt;node id=&quot;TranslatedStringKey&quot;&gt;
        ///					&lt;attribute id=&quot;Content&quot; value=&quot;&quot; type=&quot;28&quot; handle=&quot;ls::TranslatedStringRepository::s_HandleUnknown&quot; /&gt;
        ///					&lt;attribute id=&quot;ExtraData&quot; value=&quot;&quot; type=&quot;23&quot; /&gt;
        ///					&lt;attribute id=&quot;Speaker&quot; value=&quot;&quot; type=&quot;22&quot; /&gt;
        ///					&lt;attribute id=&quot;Stub&quot; value=&quot;True&quot; type=&quot;19&quot;  [rest of string was truncated]&quot;;.
        /// </summary>
        public static string DefaultLocaleResource {
            get {
                return ResourceManager.GetString("DefaultLocaleResource", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to $ProjectName for $ModuleName
        ///=======
        ///
        ///# Features:
        ///
        ///# Releases
        ///* [Steam Workshop]() 
        ///* [Nexus]()
        ///
        ///# Attribution
        ///- [Divinity: Original Sin 2](http://store.steampowered.com/app/435150/Divinity_Original_Sin_2/), a game by [Larian Studios](http://larian.com/).
        /// </summary>
        public static string DefaultReadme {
            get {
                return ResourceManager.GetString("DefaultReadme", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ### Keywords ###
        ///# ProjectName = The name of the project, without the GUID.
        ///# ProjectFolder = The folder name of the project. Typically just the project name, though imported projects may differ.
        ///# ModUUID = The UUID of the mod.
        ///# ModFolder = The folder value for the mod. Typically ProjectName_ModUUID
        ///
        ///### Directories to use for junctions ###
        ///
        ///Editor/Mods/ModFolder
        ///Mods/ModFolder
        ///Projects/ProjectFolder
        ///Public/ModFolder.
        /// </summary>
        public static string DirectoryLayout {
            get {
                return ResourceManager.GetString("DirectoryLayout", resourceCulture);
            }
        }
    }
}
