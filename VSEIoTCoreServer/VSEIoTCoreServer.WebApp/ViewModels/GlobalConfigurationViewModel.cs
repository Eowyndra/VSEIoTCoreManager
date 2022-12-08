// ----------------------------------------------------------------------------
// Filename: GlobalConfigurationViewModel.cs
// Copyright (c) 2022 ifm diagnostic GmbH - All rights reserved.
// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace VSEIoTCoreServer.WebApp.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class GlobalConfigurationViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [Range(1, 65535)]
        public int GlobalIoTCorePort { get; set; }
    }
}
