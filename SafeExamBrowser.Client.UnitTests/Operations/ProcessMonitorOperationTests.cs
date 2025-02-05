﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Client.Operations;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Processes;

namespace SafeExamBrowser.Client.UnitTests.Operations
{
	[TestClass]
	public class ProcessMonitorOperationTests
	{
		private Mock<ILogger> logger;
		private Mock<IProcessMonitor> processMonitor;
		private ApplicationSettings settings;
		private ProcessMonitorOperation sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			processMonitor = new Mock<IProcessMonitor>();
			settings = new ApplicationSettings();

			sut = new ProcessMonitorOperation(logger.Object, processMonitor.Object,settings);
		}

		[TestMethod]
		public void MustObserveExplorerWithDisableExplorerShell()
		{
			var counter = 0;
			var start = 0;
			var stop = 0;

			settings.KioskMode = KioskMode.DisableExplorerShell;
			processMonitor.Setup(p => p.StartMonitoringExplorer()).Callback(() => start = ++counter);
			processMonitor.Setup(p => p.StopMonitoringExplorer()).Callback(() => stop = ++counter);

			sut.Perform();
			sut.Revert();

			processMonitor.Verify(p => p.StartMonitoringExplorer(), Times.Once);
			processMonitor.Verify(p => p.StopMonitoringExplorer(), Times.Once);

			Assert.AreEqual(1, start);
			Assert.AreEqual(2, stop);
		}

		[TestMethod]
		public void MustNotObserveExplorerWithOtherKioskModes()
		{
			settings.KioskMode = KioskMode.CreateNewDesktop;

			sut.Perform();
			sut.Revert();

			settings.KioskMode = KioskMode.None;

			sut.Perform();
			sut.Revert();

			processMonitor.Verify(p => p.StartMonitoringExplorer(), Times.Never);
			processMonitor.Verify(p => p.StopMonitoringExplorer(), Times.Never);
		}
	}
}
