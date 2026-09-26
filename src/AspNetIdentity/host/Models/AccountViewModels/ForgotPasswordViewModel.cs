// Copyright (c) Yaroslav Tatarenko. All rights reserved.
// Part of a fork of IdentityServer4 (Copyright (c) Brock Allen & Dominick Baier).
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
