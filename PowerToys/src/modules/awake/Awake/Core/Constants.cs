﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Awake.Core
{
    internal static class Constants
    {
        internal const string AppName = "Awake";
        internal const string FullAppName = "阻止睡眠"; // 这个 FullAppName 好像也只有悬停显示才用上... //internal const string FullAppName = "PowerToys " + AppName;
        internal const string TrayWindowId = "WindowsForms10.Window.0.app.0.";
        internal const string BuildRegistryLocation = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
    }
}
