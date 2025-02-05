﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SafeExamBrowser.Core.Contracts;

namespace SafeExamBrowser.UserInterface.Shared.Utilities
{
	public static class IconResourceLoader
	{
		public static UIElement Load(IIconResource resource)
		{
			try
			{
				if (resource.IsBitmapResource)
				{
					return LoadBitmapResource(resource);
				}
				else if (resource.IsXamlResource)
				{
					return LoadXamlResource(resource);
				}
			}
			catch (Exception)
			{
				return NotFoundSymbol();
			}

			throw new NotSupportedException($"Application icon resource of type '{resource.GetType()}' is not supported!");
		}

		private static UIElement LoadBitmapResource(IIconResource resource)
		{
			return new Image
			{
				Source = new BitmapImage(resource.Uri)
			};
		}

		private static UIElement LoadXamlResource(IIconResource resource)
		{
			using (var stream = Application.GetResourceStream(resource.Uri)?.Stream)
			{
				return XamlReader.Load(stream) as UIElement;
			}
		}

		private static UIElement NotFoundSymbol()
		{
			return new TextBlock(new Run("X") { Foreground = Brushes.Red, FontWeight = FontWeights.Bold })
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
		}
	}
}
