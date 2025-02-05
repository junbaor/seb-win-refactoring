﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell.Events;

namespace SafeExamBrowser.UserInterface.Contracts.Shell
{
	/// <summary>
	/// The action center is a user interface element via which the user can access and control various aspects of the application.
	/// </summary>
	public interface IActionCenter
	{
		/// <summary>
		/// Controls the visibility of the clock.
		/// </summary>
		bool ShowClock { set; }

		/// <summary>
		/// Event fired when the user clicked the quit button.
		/// </summary>
		event QuitButtonClickedEventHandler QuitButtonClicked;

		/// <summary>
		/// Adds the given application control to the action center.
		/// </summary>
		void AddApplicationControl(IApplicationControl control);

		/// <summary>
		/// Adds the given notification control to the action center.
		/// </summary>
		void AddNotificationControl(INotificationControl control);

		/// <summary>
		/// Adds the given system control to the action center.
		/// </summary>
		void AddSystemControl(ISystemControl control);

		/// <summary>
		/// Closes the action center.
		/// </summary>
		void Close();

		/// <summary>
		/// Makes the action center invisible.
		/// </summary>
		void Hide();
		
		/// <summary>
		/// Moves the action center to the left of the screen and resizes it accordingly.
		/// </summary>
		void InitializeBounds();

		/// <summary>
		/// Initializes all text elements in the action center.
		/// </summary>
		void InitializeText(IText text);

		/// <summary>
		/// Registers the specified activator to control the visibility of the action center.
		/// </summary>
		void Register(IActionCenterActivator activator);

		/// <summary>
		/// Makes the action center visible.
		/// </summary>
		void Show();
	}
}
