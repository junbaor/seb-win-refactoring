﻿/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Runtime.Operations;

namespace SafeExamBrowser.Runtime.UnitTests.Operations
{
	[TestClass]
	public class ClientOperationTests
	{
		private Action clientReady;
		private Action terminated;
		private AppConfig appConfig;
		private Mock<IConfigurationRepository> configuration;
		private Mock<IClientProxy> proxy;
		private Mock<ILogger> logger;
		private Mock<IProcess> process;
		private Mock<IProcessFactory> processFactory;
		private Mock<IProxyFactory> proxyFactory;
		private Mock<IRuntimeHost> runtimeHost;
		private Mock<ISessionData> session;
		private ClientOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			appConfig = new AppConfig();
			configuration = new Mock<IConfigurationRepository>();
			clientReady = new Action(() => runtimeHost.Raise(h => h.ClientReady += null));
			logger = new Mock<ILogger>();
			process = new Mock<IProcess>();
			processFactory = new Mock<IProcessFactory>();
			proxy = new Mock<IClientProxy>();
			proxyFactory = new Mock<IProxyFactory>();
			runtimeHost = new Mock<IRuntimeHost>();
			session = new Mock<ISessionData>();
			terminated = new Action(() =>
			{
				runtimeHost.Raise(h => h.ClientDisconnected += null);
				process.Raise(p => p.Terminated += null, 0);
			});

			configuration.SetupGet(c => c.CurrentSession).Returns(session.Object);
			configuration.SetupGet(c => c.AppConfig).Returns(appConfig);
			proxyFactory.Setup(f => f.CreateClientProxy(It.IsAny<string>())).Returns(proxy.Object);
			session.SetupGet(s => s.ClientProcess).Returns(process.Object);
			session.SetupGet(s => s.ClientProxy).Returns(proxy.Object);

			sut = new ClientOperation(configuration.Object, logger.Object, processFactory.Object, proxyFactory.Object, runtimeHost.Object, 0);
		}

		[TestMethod]
		public void MustStartClientWhenPerforming()
		{
			var result = default(OperationResult);
			var response = new AuthenticationResponse { ProcessId = 1234 };
			var communication = new CommunicationResult<AuthenticationResponse>(true, response);

			process.SetupGet(p => p.Id).Returns(response.ProcessId);
			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process.Object).Callback(clientReady);
			proxy.Setup(p => p.RequestAuthentication()).Returns(communication);
			proxy.Setup(p => p.Connect(It.IsAny<Guid>(), true)).Returns(true);

			result = sut.Perform();

			session.VerifySet(s => s.ClientProcess = process.Object, Times.Once);
			session.VerifySet(s => s.ClientProxy = proxy.Object, Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustStartClientWhenRepeating()
		{
			var result = default(OperationResult);
			var response = new AuthenticationResponse { ProcessId = 1234 };
			var communication = new CommunicationResult<AuthenticationResponse>(true, response);

			process.SetupGet(p => p.Id).Returns(response.ProcessId);
			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process.Object).Callback(clientReady);
			proxy.Setup(p => p.RequestAuthentication()).Returns(communication);
			proxy.Setup(p => p.Connect(It.IsAny<Guid>(), true)).Returns(true);

			result = sut.Repeat();

			session.VerifySet(s => s.ClientProcess = process.Object, Times.Once);
			session.VerifySet(s => s.ClientProxy = proxy.Object, Times.Once);

			Assert.AreEqual(OperationResult.Success, result);
		}

		[TestMethod]
		public void MustFailStartupIfClientNotStartedWithinTimeout()
		{
			var result = default(OperationResult);

			result = sut.Perform();

			session.VerifySet(s => s.ClientProcess = process.Object, Times.Never);
			session.VerifySet(s => s.ClientProxy = proxy.Object, Times.Never);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void MustFailStartupIfConnectionToClientNotEstablished()
		{
			var result = default(OperationResult);

			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process.Object).Callback(clientReady);
			proxy.Setup(p => p.Connect(It.IsAny<Guid>(), true)).Returns(false);

			result = sut.Perform();

			session.VerifySet(s => s.ClientProcess = process.Object, Times.Once);
			session.VerifySet(s => s.ClientProxy = proxy.Object, Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void MustFailStartupIfAuthenticationNotSuccessful()
		{
			var result = default(OperationResult);
			var response = new AuthenticationResponse { ProcessId = -1 };
			var communication = new CommunicationResult<AuthenticationResponse>(true, response);

			process.SetupGet(p => p.Id).Returns(1234);
			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process.Object).Callback(clientReady);
			proxy.Setup(p => p.RequestAuthentication()).Returns(communication);
			proxy.Setup(p => p.Connect(It.IsAny<Guid>(), true)).Returns(true);

			result = sut.Perform();

			session.VerifySet(s => s.ClientProcess = process.Object, Times.Once);
			session.VerifySet(s => s.ClientProxy = proxy.Object, Times.Once);

			Assert.AreEqual(OperationResult.Failed, result);
		}

		[TestMethod]
		public void MustStopClientWhenReverting()
		{
			proxy.Setup(p => p.Disconnect()).Callback(terminated);

			PerformNormally();
			sut.Revert();

			proxy.Verify(p => p.InitiateShutdown(), Times.Once);
			proxy.Verify(p => p.Disconnect(), Times.Once);
			process.Verify(p => p.Kill(), Times.Never);
			session.VerifySet(s => s.ClientProcess = null, Times.Once);
			session.VerifySet(s => s.ClientProxy = null, Times.Once);
		}

		[TestMethod]
		public void MustKillClientIfStoppingFailed()
		{
			process.Setup(p => p.Kill()).Callback(() => process.SetupGet(p => p.HasTerminated).Returns(true));

			PerformNormally();
			sut.Revert();

			process.Verify(p => p.Kill(), Times.AtLeastOnce);
			session.VerifySet(s => s.ClientProcess = null, Times.Once);
			session.VerifySet(s => s.ClientProxy = null, Times.Once);
		}

		[TestMethod]
		public void MustAttemptToKillFiveTimesThenAbort()
		{
			PerformNormally();
			sut.Revert();

			process.Verify(p => p.Kill(), Times.Exactly(5));
			session.VerifySet(s => s.ClientProcess = null, Times.Never);
			session.VerifySet(s => s.ClientProxy = null, Times.Never);
		}

		[TestMethod]
		public void MustNotStopClientIfAlreadyTerminated()
		{
			process.SetupGet(p => p.HasTerminated).Returns(true);

			sut.Revert();

			proxy.Verify(p => p.InitiateShutdown(), Times.Never);
			proxy.Verify(p => p.Disconnect(), Times.Never);
			process.Verify(p => p.Kill(), Times.Never);
			session.VerifySet(s => s.ClientProcess = null, Times.Never);
			session.VerifySet(s => s.ClientProxy = null, Times.Never);
		}

		private void PerformNormally()
		{
			var response = new AuthenticationResponse { ProcessId = 1234 };
			var communication = new CommunicationResult<AuthenticationResponse>(true, response);

			process.SetupGet(p => p.Id).Returns(response.ProcessId);
			processFactory.Setup(f => f.StartNew(It.IsAny<string>(), It.IsAny<string[]>())).Returns(process.Object).Callback(clientReady);
			proxy.Setup(p => p.RequestAuthentication()).Returns(communication);
			proxy.Setup(p => p.Connect(It.IsAny<Guid>(), true)).Returns(true);

			sut.Perform();
		}
	}
}