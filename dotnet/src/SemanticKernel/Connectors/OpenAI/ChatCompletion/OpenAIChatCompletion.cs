﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Reliability;
using Microsoft.SemanticKernel.Text;

namespace Microsoft.SemanticKernel.Connectors.OpenAI.ChatCompletion;

public class OpenAIChatCompletion : OpenAIClientAbstract, IChatCompletion
{
    // 3P OpenAI REST API endpoint
    private const string OpenaiEndpoint = "https://api.openai.com/v1/chat/completions";

    private readonly string _modelId;
    private readonly string _openaiEndpoint;

    public OpenAIChatCompletion(
        string modelId,
        string endpoint,
        string apiKey,
        string? organization = null,
        ILogger? log = null,
        IDelegatingHandlerFactory? handlerFactory = null
    ) : base(log, handlerFactory)
    {
        Verify.NotEmpty(modelId, "The OpenAI model ID cannot be empty");
        this._modelId = modelId;

        if (!string.IsNullOrEmpty(endpoint))
        {

            this._openaiEndpoint = $"{endpoint.TrimEnd('/')}/v1/chat/completions";
        }
        else
        {
            this._openaiEndpoint = OpenaiEndpoint;
        }

        Verify.NotEmpty(apiKey, "The OpenAI API key cannot be empty");
        this.HTTPClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        if (!string.IsNullOrEmpty(organization))
        {
            this.HTTPClient.DefaultRequestHeaders.Add("OpenAI-Organization", organization);
        }
    }

    /// <inheritdoc/>
    public Task<string> GenerateMessageAsync(
        ChatHistory chat,
        ChatRequestSettings requestSettings,
        CancellationToken cancellationToken = default)
    {
        Verify.NotNull(requestSettings, "Completion settings cannot be empty");
        this.Log.LogDebug("Sending OpenAI completion request to {0}", this._openaiEndpoint);

        if (requestSettings.MaxTokens < 1)
        {
            throw new AIException(
                AIException.ErrorCodes.InvalidRequest,
                $"MaxTokens {requestSettings.MaxTokens} is not valid, the value must be greater than zero");
        }

        var requestBody = Json.Serialize(new ChatCompletionRequest
        {
            Model = this._modelId,
            Messages = ToHttpSchema(chat),
            Temperature = requestSettings.Temperature,
            TopP = requestSettings.TopP,
            PresencePenalty = requestSettings.PresencePenalty,
            FrequencyPenalty = requestSettings.FrequencyPenalty,
            MaxTokens = requestSettings.MaxTokens,
            Stop = requestSettings.StopSequences is { Count: > 0 } ? requestSettings.StopSequences : null,
        });

        return this.ExecuteChatCompletionRequestAsync(this._openaiEndpoint, requestBody, cancellationToken);
    }

    /// <inheritdoc/>
    public ChatHistory CreateNewChat(string instructions = "")
    {
        return new OpenAIChatHistory(instructions);
    }

    /// <summary>
    /// Map chat data to HTTP schema used with LLM
    /// </summary>
    /// <returns>Returns list of chat messages</returns>
    private static IList<ChatCompletionRequest.Message> ToHttpSchema(ChatHistory chat)
    {
        return chat.Messages
            .Select(msg => new ChatCompletionRequest.Message(msg.AuthorRole, msg.Content))
            .ToList();
    }
}
