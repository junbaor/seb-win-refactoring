﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.Browser
{
	/// <summary>
	/// Defines all settings for a window of the browser engine.
	/// </summary>
	[Serializable]
	public class BrowserWindowSettings
	{
		/// <summary>
		/// Determines whether the user will be allowed to change the URL in the address bar.
		/// </summary>
		public bool AllowAddressBar { get; set; }

		/// <summary>
		/// Determines whether the user will be allowed to navigate backwards.
		/// </summary>
		public bool AllowBackwardNavigation { get; set; }
		
		/// <summary>
		/// Determines whether the user will be allowed to open the developer console.
		/// </summary>
		public bool AllowDeveloperConsole { get; set; }

		/// <summary>
		/// Determines whether the user will be allowed to navigate forwards.
		/// </summary>
		public bool AllowForwardNavigation { get; set; }

		/// <summary>
		/// Determines whether the user will be allowed to reload webpages.
		/// </summary>
		public bool AllowReloading { get; set; }

		/// <summary>
		/// Determines whether the browser window will be rendered in fullscreen mode, i.e. without window frame.
		/// </summary>
		public bool FullScreenMode { get; set; }

		/// <summary>
		/// Determines whether the user will need to confirm every reload attempt.
		/// </summary>
		public bool ShowReloadWarning { get; set; }
	}
}
