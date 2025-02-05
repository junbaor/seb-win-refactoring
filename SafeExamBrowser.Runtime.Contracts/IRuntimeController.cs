﻿/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Runtime.Contracts
{
	/// <summary>
	/// Controls the lifetime and is responsible for the event handling of the runtime application component.
	/// </summary>
	public interface IRuntimeController
	{
		/// <summary>
		/// Reverts any changes, releases all used resources and terminates the runtime.
		/// </summary>
		void Terminate();

		/// <summary>
		/// Tries to start the runtime. Returns <c>true</c> if successful, otherwise <c>false</c>.
		/// </summary>
		bool TryStart();
	}
}
