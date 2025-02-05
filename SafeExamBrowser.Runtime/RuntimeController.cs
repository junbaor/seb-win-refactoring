﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Service;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Contracts;
using SafeExamBrowser.Runtime.Operations.Events;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime
{
	internal class RuntimeController : IRuntimeController
	{
		private AppConfig appConfig;
		private ILogger logger;
		private IMessageBox messageBox;
		private IOperationSequence bootstrapSequence;
		private IRepeatableOperationSequence sessionSequence;
		private IRuntimeHost runtimeHost;
		private IRuntimeWindow runtimeWindow;
		private IServiceProxy service;
		private SessionContext sessionContext;
		private ISplashScreen splashScreen;
		private Action shutdown;
		private IText text;
		private IUserInterfaceFactory uiFactory;
		
		private SessionConfiguration Session
		{
			get { return sessionContext.Current; }
		}

		private bool SessionIsRunning
		{
			get { return Session != null; }
		}

		public RuntimeController(
			AppConfig appConfig,
			ILogger logger,
			IMessageBox messageBox,
			IOperationSequence bootstrapSequence,
			IRepeatableOperationSequence sessionSequence,
			IRuntimeHost runtimeHost,
			IServiceProxy service,
			SessionContext sessionContext,
			Action shutdown,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.bootstrapSequence = bootstrapSequence;
			this.logger = logger;
			this.messageBox = messageBox;
			this.runtimeHost = runtimeHost;
			this.sessionSequence = sessionSequence;
			this.service = service;
			this.sessionContext = sessionContext;
			this.shutdown = shutdown;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public bool TryStart()
		{
			logger.Info("Initiating startup procedure...");

			runtimeWindow = uiFactory.CreateRuntimeWindow(appConfig);
			splashScreen = uiFactory.CreateSplashScreen(appConfig);

			bootstrapSequence.ProgressChanged += BootstrapSequence_ProgressChanged;
			bootstrapSequence.StatusChanged += BootstrapSequence_StatusChanged;
			sessionSequence.ActionRequired += SessionSequence_ActionRequired;
			sessionSequence.ProgressChanged += SessionSequence_ProgressChanged;
			sessionSequence.StatusChanged += SessionSequence_StatusChanged;

			splashScreen.Show();

			var initialized = bootstrapSequence.TryPerform() == OperationResult.Success;

			if (initialized)
			{
				RegisterEvents();

				logger.Info("Application successfully initialized.");
				logger.Log(string.Empty);
				logger.Subscribe(runtimeWindow);
				splashScreen.Close();

				StartSession();
			}
			else
			{
				logger.Info("Application startup aborted!");
				logger.Log(string.Empty);

				messageBox.Show(TextKey.MessageBox_StartupError, TextKey.MessageBox_StartupErrorTitle, icon: MessageBoxIcon.Error, parent: splashScreen);
			}

			return initialized && SessionIsRunning;
		}

		public void Terminate()
		{
			DeregisterEvents();

			if (SessionIsRunning)
			{
				StopSession();
			}

			logger.Unsubscribe(runtimeWindow);
			runtimeWindow?.Close();

			splashScreen = uiFactory.CreateSplashScreen(appConfig);
			splashScreen.Show();

			logger.Log(string.Empty);
			logger.Info("Initiating shutdown procedure...");

			var success = bootstrapSequence.TryRevert() == OperationResult.Success;

			if (success)
			{
				logger.Info("Application successfully finalized.");
				logger.Log(string.Empty);
			}
			else
			{
				logger.Info("Shutdown procedure failed!");
				logger.Log(string.Empty);

				messageBox.Show(TextKey.MessageBox_ShutdownError, TextKey.MessageBox_ShutdownErrorTitle, icon: MessageBoxIcon.Error, parent: splashScreen);
			}

			splashScreen.Close();
		}

		private void StartSession()
		{
			runtimeWindow.Show();
			runtimeWindow.BringToForeground();
			runtimeWindow.ShowProgressBar = true;
			logger.Info(AppendDivider("Session Start Procedure"));

			if (SessionIsRunning)
			{
				DeregisterSessionEvents();
			}

			var result = SessionIsRunning ? sessionSequence.TryRepeat() : sessionSequence.TryPerform();

			if (result == OperationResult.Success)
			{
				logger.Info(AppendDivider("Session Running"));

				HandleSessionStartSuccess();
			}
			else if (result == OperationResult.Failed)
			{
				logger.Info(AppendDivider("Session Start Failed"));

				HandleSessionStartFailure();
			}
			else if (result == OperationResult.Aborted)
			{
				logger.Info(AppendDivider("Session Start Aborted"));

				HandleSessionStartAbortion();
			}
		}

		private void HandleSessionStartSuccess()
		{
			RegisterSessionEvents();

			runtimeWindow.ShowProgressBar = false;
			runtimeWindow.ShowLog = Session.Settings.AllowApplicationLogAccess;
			runtimeWindow.TopMost = Session.Settings.KioskMode != KioskMode.None;
			runtimeWindow.UpdateStatus(TextKey.RuntimeWindow_ApplicationRunning);

			if (Session.Settings.KioskMode == KioskMode.DisableExplorerShell)
			{
				runtimeWindow.Hide();
			}
		}

		private void HandleSessionStartFailure()
		{
			if (SessionIsRunning)
			{
				StopSession();

				messageBox.Show(TextKey.MessageBox_SessionStartError, TextKey.MessageBox_SessionStartErrorTitle, icon: MessageBoxIcon.Error, parent: runtimeWindow);

				logger.Info("Terminating application...");
				shutdown.Invoke();
			}
			else
			{
				messageBox.Show(TextKey.MessageBox_SessionStartError, TextKey.MessageBox_SessionStartErrorTitle, icon: MessageBoxIcon.Error, parent: runtimeWindow);
			}
		}

		private void HandleSessionStartAbortion()
		{
			if (SessionIsRunning)
			{
				runtimeWindow.ShowProgressBar = false;
				runtimeWindow.UpdateStatus(TextKey.RuntimeWindow_ApplicationRunning);
				runtimeWindow.TopMost = Session.Settings.KioskMode != KioskMode.None;

				if (Session.Settings.KioskMode == KioskMode.DisableExplorerShell)
				{
					runtimeWindow.Hide();
				}
			}
		}

		private void StopSession()
		{
			runtimeWindow.Show();
			runtimeWindow.BringToForeground();
			runtimeWindow.ShowProgressBar = true;
			logger.Info(AppendDivider("Session Stop Procedure"));

			DeregisterSessionEvents();

			var success = sessionSequence.TryRevert() == OperationResult.Success;

			if (success)
			{
				logger.Info(AppendDivider("Session Terminated"));
			}
			else
			{
				logger.Info(AppendDivider("Session Stop Failed"));
			}
		}

		private void RegisterEvents()
		{
			runtimeHost.ClientConfigurationNeeded += RuntimeHost_ClientConfigurationNeeded;
			runtimeHost.ReconfigurationRequested += RuntimeHost_ReconfigurationRequested;
			runtimeHost.ShutdownRequested += RuntimeHost_ShutdownRequested;
			service.ConnectionLost += ServiceProxy_ConnectionLost;
		}

		private void DeregisterEvents()
		{
			runtimeHost.ClientConfigurationNeeded -= RuntimeHost_ClientConfigurationNeeded;
			runtimeHost.ReconfigurationRequested -= RuntimeHost_ReconfigurationRequested;
			runtimeHost.ShutdownRequested -= RuntimeHost_ShutdownRequested;
			service.ConnectionLost -= ServiceProxy_ConnectionLost;
		}

		private void RegisterSessionEvents()
		{
			sessionContext.ClientProcess.Terminated += ClientProcess_Terminated;
			sessionContext.ClientProxy.ConnectionLost += ClientProxy_ConnectionLost;
		}

		private void DeregisterSessionEvents()
		{
			if (sessionContext.ClientProcess != null)
			{
				sessionContext.ClientProcess.Terminated -= ClientProcess_Terminated;
			}

			if (sessionContext.ClientProxy != null)
			{
				sessionContext.ClientProxy.ConnectionLost -= ClientProxy_ConnectionLost;
			}
		}

		private void BootstrapSequence_ProgressChanged(ProgressChangedEventArgs args)
		{
			MapProgress(splashScreen, args);
		}

		private void BootstrapSequence_StatusChanged(TextKey status)
		{
			splashScreen?.UpdateStatus(status, true);
		}

		private void ClientProcess_Terminated(int exitCode)
		{
			logger.Error($"Client application has unexpectedly terminated with exit code {exitCode}!");

			if (SessionIsRunning)
			{
				StopSession();
			}

			messageBox.Show(TextKey.MessageBox_ApplicationError, TextKey.MessageBox_ApplicationErrorTitle, icon: MessageBoxIcon.Error, parent: runtimeWindow);
			shutdown.Invoke();
		}

		private void ClientProxy_ConnectionLost()
		{
			logger.Error("Lost connection to the client application!");

			if (SessionIsRunning)
			{
				StopSession();
			}

			messageBox.Show(TextKey.MessageBox_ApplicationError, TextKey.MessageBox_ApplicationErrorTitle, icon: MessageBoxIcon.Error, parent: runtimeWindow);
			shutdown.Invoke();
		}

		private void RuntimeHost_ClientConfigurationNeeded(ClientConfigurationEventArgs args)
		{
			args.ClientConfiguration = new ClientConfiguration
			{
				AppConfig = sessionContext.Next.AppConfig,
				SessionId = sessionContext.Next.SessionId,
				Settings = sessionContext.Next.Settings
			};
		}

		private void RuntimeHost_ReconfigurationRequested(ReconfigurationEventArgs args)
		{
			var mode = Session.Settings.ConfigurationMode;

			if (mode == ConfigurationMode.ConfigureClient)
			{
				logger.Info($"Accepted request for reconfiguration with '{args.ConfigurationPath}'.");
				sessionContext.ReconfigurationFilePath = args.ConfigurationPath;

				StartSession();
			}
			else
			{
				logger.Info($"Denied request for reconfiguration with '{args.ConfigurationPath}' due to '{mode}' mode!");
				sessionContext.ClientProxy.InformReconfigurationDenied(args.ConfigurationPath);
			}
		}

		private void RuntimeHost_ShutdownRequested()
		{
			logger.Info("Received shutdown request from the client application.");
			shutdown.Invoke();
		}

		private void ServiceProxy_ConnectionLost()
		{
			if (SessionIsRunning && Session.Settings.Service.Policy == ServicePolicy.Mandatory)
			{
				logger.Error("Lost connection to the service component!");
				StopSession();
				messageBox.Show(TextKey.MessageBox_ApplicationError, TextKey.MessageBox_ApplicationErrorTitle, icon: MessageBoxIcon.Error, parent: runtimeWindow);
				shutdown.Invoke();
			}
			else
			{
				logger.Warn("Lost connection to the service component!");
			}
		}

		private void SessionSequence_ActionRequired(ActionRequiredEventArgs args)
		{
			switch (args)
			{
				case ConfigurationCompletedEventArgs a:
					AskIfConfigurationSufficient(a);
					break;
				case MessageEventArgs m:
					ShowMessageBox(m);
					break;
				case PasswordRequiredEventArgs p:
					AskForPassword(p);
					break;
			}
		}

		private void AskIfConfigurationSufficient(ConfigurationCompletedEventArgs args)
		{
			var message = TextKey.MessageBox_ClientConfigurationQuestion;
			var title = TextKey.MessageBox_ClientConfigurationQuestionTitle;
			var result = messageBox.Show(message, title, MessageBoxAction.YesNo, MessageBoxIcon.Question, runtimeWindow);

			args.AbortStartup = result == MessageBoxResult.Yes;
		}

		private void AskForPassword(PasswordRequiredEventArgs args)
		{
			var isStartup = !SessionIsRunning;
			var isRunningOnDefaultDesktop = SessionIsRunning && Session.Settings.KioskMode == KioskMode.DisableExplorerShell;

			if (isStartup || isRunningOnDefaultDesktop)
			{
				TryGetPasswordViaDialog(args);
			}
			else
			{
				TryGetPasswordViaClient(args);
			}
		}

		private void ShowMessageBox(MessageEventArgs args)
		{
			var isStartup = !SessionIsRunning;
			var isRunningOnDefaultDesktop = SessionIsRunning && Session.Settings.KioskMode == KioskMode.DisableExplorerShell;
			var message = text.Get(args.Message);
			var title = text.Get(args.Title);

			foreach (var placeholder in args.MessagePlaceholders)
			{
				message = message.Replace(placeholder.Key, placeholder.Value);
			}

			foreach (var placeholder in args.TitlePlaceholders)
			{
				title = title.Replace(placeholder.Key, placeholder.Value);
			}

			if (isStartup || isRunningOnDefaultDesktop)
			{
				messageBox.Show(message, title, MessageBoxAction.Confirm, args.Icon, runtimeWindow);
			}
			else
			{
				ShowMessageBoxViaClient(message, title, MessageBoxAction.Confirm, args.Icon);
			}
		}

		private MessageBoxResult ShowMessageBoxViaClient(string message, string title, MessageBoxAction action, MessageBoxIcon icon)
		{
			var requestId = Guid.NewGuid();
			var result = MessageBoxResult.None;
			var response = default(MessageBoxReplyEventArgs);
			var responseEvent = new AutoResetEvent(false);
			var responseEventHandler = new CommunicationEventHandler<MessageBoxReplyEventArgs>((args) =>
			{
				if (args.RequestId == requestId)
				{
					response = args;
					responseEvent.Set();
				}
			});

			runtimeHost.MessageBoxReplyReceived += responseEventHandler;

			var communication = sessionContext.ClientProxy.ShowMessage(message, title, (int) action, (int) icon, requestId);

			if (communication.Success)
			{
				responseEvent.WaitOne();
				result = (MessageBoxResult) response.Result;
			}

			runtimeHost.MessageBoxReplyReceived -= responseEventHandler;

			return result;
		}

		private void TryGetPasswordViaDialog(PasswordRequiredEventArgs args)
		{
			var message = default(TextKey);
			var title = default(TextKey);

			switch (args.Purpose)
			{
				case PasswordRequestPurpose.LocalAdministrator:
					message = TextKey.PasswordDialog_LocalAdminPasswordRequired;
					title = TextKey.PasswordDialog_LocalAdminPasswordRequiredTitle;
					break;
				case PasswordRequestPurpose.LocalSettings:
					message = TextKey.PasswordDialog_LocalSettingsPasswordRequired;
					title = TextKey.PasswordDialog_LocalSettingsPasswordRequiredTitle;
					break;
				case PasswordRequestPurpose.Settings:
					message = TextKey.PasswordDialog_SettingsPasswordRequired;
					title = TextKey.PasswordDialog_SettingsPasswordRequiredTitle;
					break;
			}

			var dialog = uiFactory.CreatePasswordDialog(text.Get(message), text.Get(title));
			var result = dialog.Show(runtimeWindow);

			args.Password = result.Password;
			args.Success = result.Success;
		}

		private void TryGetPasswordViaClient(PasswordRequiredEventArgs args)
		{
			var requestId = Guid.NewGuid();
			var response = default(PasswordReplyEventArgs);
			var responseEvent = new AutoResetEvent(false);
			var responseEventHandler = new CommunicationEventHandler<PasswordReplyEventArgs>((a) =>
			{
				if (a.RequestId == requestId)
				{
					response = a;
					responseEvent.Set();
				}
			});

			runtimeHost.PasswordReceived += responseEventHandler;

			var communication = sessionContext.ClientProxy.RequestPassword(args.Purpose, requestId);

			if (communication.Success)
			{
				responseEvent.WaitOne();
				args.Password = response.Password;
				args.Success = response.Success;
			}
			else
			{
				args.Password = default(string);
				args.Success = false;
			}

			runtimeHost.PasswordReceived -= responseEventHandler;
		}

		private void SessionSequence_ProgressChanged(ProgressChangedEventArgs args)
		{
			MapProgress(runtimeWindow, args);
		}

		private void SessionSequence_StatusChanged(TextKey status)
		{
			runtimeWindow?.UpdateStatus(status, true);
		}

		private void MapProgress(IProgressIndicator progressIndicator, ProgressChangedEventArgs args)
		{
			if (args.CurrentValue.HasValue)
			{
				progressIndicator?.SetValue(args.CurrentValue.Value);
			}

			if (args.IsIndeterminate == true)
			{
				progressIndicator?.SetIndeterminate();
			}

			if (args.MaxValue.HasValue)
			{
				progressIndicator?.SetMaxValue(args.MaxValue.Value);
			}

			if (args.Progress == true)
			{
				progressIndicator?.Progress();
			}

			if (args.Regress == true)
			{
				progressIndicator?.Regress();
			}
		}

		private string AppendDivider(string message)
		{
			var dashesLeft = new String('-', 48 - message.Length / 2 - message.Length % 2);
			var dashesRight = new String('-', 48 - message.Length / 2);

			return $"### {dashesLeft} {message} {dashesRight} ###";
		}
	}
}
