﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Configuration.Contracts.Settings.Browser;
using SafeExamBrowser.Configuration.Contracts.Settings.Monitoring;
using SafeExamBrowser.Configuration.Contracts.Settings.Service;
using SafeExamBrowser.Configuration.Contracts.Settings.SystemComponents;
using SafeExamBrowser.Configuration.Contracts.Settings.UserInterface;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.Contracts.Settings
{
	/// <summary>
	/// Defines all configuration options for the application.
	/// </summary>
	[Serializable]
	public class Settings
	{
		/// <summary>
		/// All action center-related settings.
		/// </summary>
		public ActionCenterSettings ActionCenter { get; set; }

		/// <summary>
		/// The hash code of the administrator password for the settings.
		/// </summary>
		public string AdminPasswordHash { get; set; }

		/// <summary>
		/// Determines whether any log information will be accessible via the user interface.
		/// </summary>
		public bool AllowApplicationLogAccess { get; set; }

		/// <summary>
		/// All audio-related settings.
		/// </summary>
		public AudioSettings Audio { get; set; }

		/// <summary>
		/// All browser-related settings.
		/// </summary>
		public BrowserSettings Browser { get; set; }

		/// <summary>
		/// The mode which determines the configuration behaviour.
		/// </summary>
		public ConfigurationMode ConfigurationMode { get; set; }

		/// <summary>
		/// All keyboard-related settings.
		/// </summary>
		public KeyboardSettings Keyboard { get; set; }

		/// <summary>
		/// The kiosk mode which determines how the computer is locked down.
		/// </summary>
		public KioskMode KioskMode { get; set; }

		/// <summary>
		/// The global log severity to be used.
		/// </summary>
		public LogLevel LogLevel { get; set; }

		/// <summary>
		/// All mouse-related settings.
		/// </summary>
		public MouseSettings Mouse { get; set; }

		/// <summary>
		/// The hash code of the quit password.
		/// </summary>
		public string QuitPasswordHash { get; set; }

		/// <summary>
		/// All service-related settings.
		/// </summary>
		public ServiceSettings Service { get; set; }

		/// <summary>
		/// All taskbar-related settings.
		/// </summary>
		public TaskbarSettings Taskbar { get; set; }

		/// <summary>
		/// The mode which determines the look &amp; feel of the user interface.
		/// </summary>
		public UserInterfaceMode UserInterfaceMode { get; set; }

		public Settings()
		{
			ActionCenter = new ActionCenterSettings();
			Audio = new AudioSettings();
			Browser = new BrowserSettings();
			Keyboard = new KeyboardSettings();
			Mouse = new MouseSettings();
			Service = new ServiceSettings();
			Taskbar = new TaskbarSettings();
		}
	}
}