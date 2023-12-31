﻿// ----------------------------------------------------------------------------
// Filename: IoTCoreOptions.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp
{
    public class IoTCoreOptions
    {
        public const string IoTCoreSettings = "IoTCoreSettings";

        public string AdapterLocation { get; set; } = string.Empty;
        public string IoTCoreURI { get; set; } = string.Empty;
    }
}
