﻿// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace SemanticKernel.Service.Config;

/// <summary>
/// Configuration options for Azure speech recognition.
/// </summary>
public sealed class AzureSpeechOptions
{
    public const string PropertyName = "AzureSpeech";

    /// <summary>
    /// Location of the Azure speech service to use (e.g. "South Central US")
    /// </summary>
    [Required, NotEmptyOrWhitespace]
    public string? Region { get; set; } = string.Empty;

    /// <summary>
    /// Key to access the Azure speech service.
    /// </summary>
    [Required, NotEmptyOrWhitespace]
    public string Key { get; set; } = string.Empty;
}
