﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Shell.Events;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Delegates;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi
{
	public class TouchActivator : IActionCenterActivator
	{
		private HookDelegate hookDelegate;
		private IntPtr handle;
		private bool isDown;
		private ILogger logger;

		public event ActivatorEventHandler Activated;
		public event ActivatorEventHandler Deactivated { add { } remove { } }
		public event ActivatorEventHandler Toggled { add { } remove { } }

		public TouchActivator(ILogger logger)
		{
			this.logger = logger;
		}

		public void Start()
		{
			var hookReadyEvent = new AutoResetEvent(false);
			var hookThread = new Thread(() =>
			{
				var sleepEvent = new AutoResetEvent(false);
				var process = System.Diagnostics.Process.GetCurrentProcess();
				var module = process.MainModule;
				var moduleHandle = Kernel32.GetModuleHandle(module.ModuleName);

				// IMPORTANT:
				// Ensures that the hook delegate does not get garbage collected prematurely, as it will be passed to unmanaged code.
				// Not doing so will result in a <c>CallbackOnCollectedDelegate</c> error and subsequent application crash!
				hookDelegate = new HookDelegate(LowLevelMouseProc);

				handle = User32.SetWindowsHookEx(HookType.WH_MOUSE_LL, hookDelegate, moduleHandle, 0);
				hookReadyEvent.Set();

				while (true)
				{
					sleepEvent.WaitOne();
				}
			});

			hookThread.SetApartmentState(ApartmentState.STA);
			hookThread.IsBackground = true;
			hookThread.Start();

			hookReadyEvent.WaitOne();
		}

		public void Stop()
		{
			User32.UnhookWindowsHookEx(handle);
		}

		private IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0 && !Ignore(wParam.ToInt32()))
			{
				var message = wParam.ToInt32();
				var mouseData = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
				var position = $"{mouseData.Point.X}/{mouseData.Point.Y}";
				var extraInfo = mouseData.DwExtraInfo.ToUInt32();
				var isTouch = (extraInfo & Constant.MOUSEEVENTF_MASK) == Constant.MOUSEEVENTF_FROMTOUCH;
				var inActivationArea = 0 < mouseData.Point.X && mouseData.Point.X < 100;

				if (isTouch)
				{
					if (message == Constant.WM_LBUTTONUP)
					{
						isDown = false;
					}

					if (message == Constant.WM_LBUTTONDOWN && inActivationArea)
					{
						isDown = true;
						Task.Delay(100).ContinueWith(_ => CheckPosition());
					}
				}
			}

			return User32.CallNextHookEx(handle, nCode, wParam, lParam);
		}

		private void CheckPosition()
		{
			var position = new POINT();
			User32.GetCursorPos(ref position);
			var hasMoved = position.X > 200;

			if (isDown && hasMoved)
			{
				logger.Debug("Detected activation gesture for action center.");
				Activated?.Invoke();
			}
		}

		private bool Ignore(int wParam)
		{
			// For performance reasons, ignore mouse movement and wheel rotation...
			return wParam == Constant.WM_MOUSEMOVE || wParam == Constant.WM_MOUSEWHEEL;
		}
	}
}
