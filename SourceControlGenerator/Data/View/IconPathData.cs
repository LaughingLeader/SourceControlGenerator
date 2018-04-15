﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.SCG.Data.View
{
	public class IconPathData : PropertyChangedBase
	{
		public string Archive => @"pack://application:,,,/SourceControlGenerator;component/Resources/Icons/7zipIcon.png";
		public string Folder => @"pack://application:,,,/SourceControlGenerator;component/Resources/Icons/Folder.png";
		public string GitLogoEnabled => @"pack://application:,,,/SourceControlGenerator;component/Resources/Icons/GitLogo.png";
		public string GitLogoDisabled => @"pack://application:,,,/SourceControlGenerator;component/Resources/Icons/GitLogo_Disabled.png";
		public string LogMouseOver => @"pack://application:,,,/SourceControlGenerator;component/Resources/Icons/Log_MouseOver.png";
		public string LogNormal => @"pack://application:,,,/SourceControlGenerator;component/Resources/Icons/Log_Normal.png";
	}
}
