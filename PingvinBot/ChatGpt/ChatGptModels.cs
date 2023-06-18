using System.Text.Json.Serialization;

namespace PingvinBot.ChatGpt;

public record ChatCompletionCreateRequest
{
    /// <summary>
    /// Gets or sets iD of the model to use.
    /// https://platform.openai.com/docs/api-reference/chat/create#chat/create-model.
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = null!;

    /// <summary>
    /// Gets or sets the messages to generate chat completions for, in the chat format.
    /// https://platform.openai.com/docs/api-reference/chat/create#chat/create-messages.
    /// </summary>
    [JsonPropertyName("messages")]
    public ChatCompletionMessage[] Messages { get; set; } = null!;

    /// <summary>
    /// Gets or sets what sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.
    /// We generally recommend altering this or top_p but not both.
    /// Defaults to 1.
    /// https://platform.openai.com/docs/api-reference/chat/create#chat/create-temperature.
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    /// <summary>
    ///     Gets or sets an alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the
    ///     tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are
    ///     considered.
    ///     We generally recommend altering this or temperature but not both.
    ///     Defaults to 1.
    ///     https://platform.openai.com/docs/api-reference/chat/create#chat/create-top_p.
    /// </summary>
    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }

    /// <summary>
    ///     Gets or sets how many chat completion choices to generate for each input message.
    ///     Defaults to 1.
    ///     https://platform.openai.com/docs/api-reference/chat/create#chat/create-n.
    /// </summary>
    [JsonPropertyName("n")]
    public int? N { get; set; }

    /// <summary>
    ///     Gets or sets up to 4 sequences where the API will stop generating further tokens. The returned text will not contain the stop
    ///     sequence. Defaults to null.
    ///     https://platform.openai.com/docs/api-reference/chat/create#chat/create-stop.
    /// </summary>
    [JsonPropertyName("stop")]
    public string? Stop { get; set; }

    /// <summary>
    /// Gets or sets if set, partial message deltas will be sent, like in ChatGPT. Tokens will be sent as data-only server-sent events as they become available,
    /// with the stream terminated by a data: [DONE] message.
    /// Defaults to false.
    /// https://platform.openai.com/docs/api-reference/chat/create#chat/create-stream.
    /// </summary>
    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of tokens to generate in the chat completion.
    /// The total length of input tokens and generated tokens is limited by the model's context length.
    /// Defaults to inf.
    /// https://platform.openai.com/docs/api-reference/chat/create#chat/create-max_tokens.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    ///     Gets or sets number between -2.0 and 2.0. Positive values penalize new tokens based on whether they appear in the text so far,
    ///     increasing the model's likelihood to talk about new topics. Defaults to 0
    ///     https://platform.openai.com/docs/api-reference/chat/create#chat/create-presence_penalty.
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public float? PresencePenalty { get; set; }

    /// <summary>
    ///    Gets or sets number between -2.0 and 2.0. Positive values penalize new tokens based on their existing frequency in the text so far,
    ///    decreasing the model's likelihood to repeat the same line verbatim. Defaults to 0
    ///    https://platform.openai.com/docs/api-reference/chat/create#chat/create-frequency_penalty.
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public float? FrequencyPenalty { get; set; }

    /// <summary>
    /// Gets or sets a unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.
    /// https://platform.openai.com/docs/api-reference/chat/create#chat/create-user.
    /// </summary>
    [JsonPropertyName("user")]
    public string? User { get; set; }
}

public record ChatCompletionMessage
{
    /// <summary>
    /// Gets or sets can be either “system”, “user”, or “assistant”.
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = null!;

    /// <summary>
    /// Gets or sets the content of the message.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = null!;
}

public record ChatCompletionCreateResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("object")]
    public string Object { get; set; } = null!;

    [JsonPropertyName("created")]
    public int Created { get; set; }

    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = new List<Choice>();

    [JsonPropertyName("usage")]
    public Usage Usage { get; set; } = null!;
}

public record Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public record Choice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public ChatCompletionMessage Message { get; set; } = null!;

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = null!;
}

public record ChatCompletionResponseErrorWrapper
{
    [JsonPropertyName("error")]
    public ChatCompletionResponseError Error { get; set; } = null!;
}

public record ChatCompletionResponseError
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;

    [JsonPropertyName("message")]
    public string Message { get; set; } = null!;

    [JsonPropertyName("param")]
    public string Param { get; set; } = null!;

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;
}
