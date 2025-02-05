﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Browser.Contracts;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Configuration.Contracts;

namespace SafeExamBrowser.Client.Contracts
{
	/// <summary>
	/// Controls the lifetime and is responsible for the event handling of the client application component.
	/// </summary>
	public interface IClientController
	{
		/// <summary>
		/// The global configuration information to be used during application execution.
		/// </summary>
		AppConfig AppConfig { set; }

		/// <summary>
		/// The browser application.
		/// </summary>
		IBrowserApplication Browser { set; }

		/// <summary>
		/// The client host used for communication handling.
		/// </summary>
		IClientHost ClientHost { set; }

		/// <summary>
		/// The session identifier of the currently running session.
		/// </summary>
		Guid SessionId { set; }

		/// <summary>
		/// The settings to be used during application execution.
		/// </summary>
		Settings.ApplicationSettings Settings { set; }

		/// <summary>
		/// Reverts any changes, releases all used resources and terminates the client.
		/// </summary>
		void Terminate();

		/// <summary>
		/// Tries to start the client. Returns <c>true</c> if successful, otherwise <c>false</c>.
		/// </summary>
		bool TryStart();
	}
}
