﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Client.Operations
{
	/// <summary>
	/// During application shutdown, it could happen that the client stops its communication host before the runtime had the chance to
	/// disconnect from it. This operation prevents the described race condition by waiting on the runtime to disconnect from the client.
	/// </summary>
	internal class ClientHostDisconnectionOperation : IOperation
	{
		private IClientHost clientHost;
		private ILogger logger;
		private int timeout_ms;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public ClientHostDisconnectionOperation(IClientHost clientHost, ILogger logger, int timeout_ms)
		{
			this.clientHost = clientHost;
			this.logger = logger;
			this.timeout_ms = timeout_ms;
		}

		public OperationResult Perform()
		{
			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			StatusChanged?.Invoke(TextKey.OperationStatus_WaitRuntimeDisconnection);

			if (clientHost.IsConnected)
			{
				var disconnected = false;
				var disconnectedEvent = new AutoResetEvent(false);
				var disconnectedEventHandler = new CommunicationEventHandler(() => disconnectedEvent.Set());

				clientHost.RuntimeDisconnected += disconnectedEventHandler;

				logger.Info("Waiting for runtime to disconnect from client communication host...");
				disconnected = disconnectedEvent.WaitOne(timeout_ms);

				clientHost.RuntimeDisconnected -= disconnectedEventHandler;

				if (disconnected)
				{
					logger.Info("The runtime has successfully disconnected from the client communication host.");
				}
				else
				{
					logger.Error($"The runtime failed to disconnect within {timeout_ms / 1000} seconds!");

					return OperationResult.Failed;
				}
			}
			else
			{
				logger.Info("The runtime has already disconnected from the client communication host.");
			}

			return OperationResult.Success;
		}
	}
}
