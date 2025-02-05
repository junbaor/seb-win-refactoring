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
	/// Defines all settings for the browser engine of the application.
	/// </summary>
	[Serializable]
	public class BrowserSettings
	{
		/// <summary>
		/// The settings to be used for additional browser windows.
		/// </summary>
		public BrowserWindowSettings AdditionalWindow { get; set; }

		/// <summary>
		/// Determines whether the user will be allowed to download configuration files.
		/// </summary>
		public bool AllowConfigurationDownloads { get; set; }

		/// <summary>
		/// Determines whether the user will be allowed to download files (excluding configuration files).
		/// </summary>
		public bool AllowDownloads { get; set; }

		/// <summary>
		/// Determines whether the user will be allowed to zoom webpages.
		/// </summary>
		public bool AllowPageZoom { get; set; }

		/// <summary>
		/// Determines whether popup windows will be opened or not.
		/// </summary>
		public bool AllowPopups { get; set; }

		/// <summary>
		/// The custom user agent to optionally be used for all requests.
		/// </summary>
		public string CustomUserAgent { get; set; }

		/// <summary>
		/// The settings to be used for the browser request filter.
		/// </summary>
		public BrowserFilterSettings Filter { get; set; }

		/// <summary>
		/// The settings to be used for the main browser window.
		/// </summary>
		public BrowserWindowSettings MainWindow { get; set; }
		
		/// <summary>
		/// The URL with which the main browser window will be loaded.
		/// </summary>
		public string StartUrl { get; set; }

		/// <summary>
		/// Determines whether a custom user agent will be used for all requests, see <see cref="CustomUserAgent"/>.
		/// </summary>
		public bool UseCustomUserAgent { get; set; }

		public BrowserSettings()
		{
			AdditionalWindow = new BrowserWindowSettings();
			Filter = new BrowserFilterSettings();
			MainWindow = new BrowserWindowSettings();
		}
	}
}
