﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class ActionCenterNotificationButton : UserControl, INotificationControl
	{
		private INotificationController controller;

		public ActionCenterNotificationButton(INotificationController controller, INotificationInfo info)
		{
			this.controller = controller;

			InitializeComponent();
			InitializeNotificationIcon(info);
		}

		private void IconButton_Click(object sender, RoutedEventArgs e)
		{
			controller.Activate();
		}

		private void InitializeNotificationIcon(INotificationInfo info)
		{
			Icon.Content = IconResourceLoader.Load(info.IconResource);
			IconButton.ToolTip = info.Tooltip;
			Text.Text = info.Tooltip;
		}
	}
}
