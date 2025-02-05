﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Contracts.Data;

namespace SafeExamBrowser.Communication.Contracts.Proxies
{
	/// <summary>
	/// Defines the functionality for a proxy to the communication host of the runtime application component.
	/// </summary>
	public interface IRuntimeProxy : ICommunicationProxy
	{
		/// <summary>
		/// Retrieves the application configuration from the runtime.
		/// </summary>
		CommunicationResult<ConfigurationResponse> GetConfiguration();

		/// <summary>
		/// Informs the runtime that the client is ready.
		/// </summary>
		CommunicationResult InformClientReady();

		/// <summary>
		/// Requests the runtime to shut down the application.
		/// </summary>
		CommunicationResult RequestShutdown();

		/// <summary>
		/// Requests the runtime to reconfigure the application with the specified configuration.
		/// </summary>
		CommunicationResult RequestReconfiguration(string filePath);

		/// <summary>
		/// Submits the result of a message box input previously requested by the runtime.
		/// </summary>
		CommunicationResult SubmitMessageBoxResult(Guid requestId, int result);

		/// <summary>
		/// Submits the result of a password input previously requested by the runtime. If the procedure was aborted by the user,
		/// the password parameter will be <c>null</c>!
		/// </summary>
		CommunicationResult SubmitPassword(Guid requestId, bool success, string password = null);
	}
}
