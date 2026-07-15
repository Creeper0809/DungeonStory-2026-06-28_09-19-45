using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;

public enum LocalLlmRequestType
{
    Persona,
    MacroGoal,
    MoodImpulse,
    SocialRumor,
    FacilityEvolution,
    CharacterRecord,
    BubbleLine
}

public enum LocalLlmRequestStatus
{
    Succeeded,
    Failed,
    Dropped,
    TimedOut
}

public interface ILocalLlmRuntime
{
    bool GeneratePersonaAsync(string prompt, Action<LocalLlmResult> callback);
    bool GenerateMacroGoalAsync(string prompt, Action<LocalLlmResult> callback);
    bool GenerateMoodImpulseAsync(string prompt, Action<LocalLlmResult> callback);
    bool GenerateSocialRumorAsync(string prompt, Action<LocalLlmResult> callback);
    bool GenerateFacilityEvolutionAsync(string prompt, Action<LocalLlmResult> callback);
    bool GenerateCharacterRecordAsync(string prompt, string originalText, Action<LocalLlmResult> callback);
    bool GenerateBubbleLineAsync(string prompt, string originalText, Action<LocalLlmResult> callback);
}

public readonly struct LocalLlmResult
{
    public LocalLlmResult(
        LocalLlmRequestStatus status,
        string content,
        string error,
        string originalText)
    {
        Status = status;
        Content = content ?? string.Empty;
        Error = error ?? string.Empty;
        OriginalText = originalText ?? string.Empty;
    }

    public LocalLlmRequestStatus Status { get; }
    public string Content { get; }
    public string Error { get; }
    public string OriginalText { get; }
    public bool IsSuccess => Status == LocalLlmRequestStatus.Succeeded;
}

internal sealed class LocalLlmQueuedRequest
{
    public LocalLlmRequestType Type;
    public string Prompt;
    public string OriginalText;
    public float TimeoutSeconds;
    public float EnqueuedAt;
    public Action<LocalLlmResult> Callback;
}

[DisallowMultipleComponent]
[DrawWithUnity]
public sealed class LocalLlmRequestQueue : SerializedMonoBehaviour, ILocalLlmRuntime
{
    private static LocalLlmRequestQueue instance;

    [SerializeField] private string endpointUrl = "http://localhost:11434/v1/chat/completions";
    [SerializeField] private string modelName = "llama3.1";
    [SerializeField] private bool enableJsonMode = true;
    [SerializeField, Min(1)] private int maxQueueSize = 64;
    [SerializeField, Range(1, 2)] private int maxConcurrentRequests = 1;
    [SerializeField, Min(0.1f)] private float personaTimeoutSeconds = 20f;
    [SerializeField, Min(0.1f)] private float macroGoalTimeoutSeconds = 12f;
    [SerializeField, Min(0.1f)] private float moodImpulseTimeoutSeconds = 8f;
    [SerializeField, Min(0.1f)] private float socialRumorTimeoutSeconds = 8f;
    [SerializeField, Min(0.1f)] private float facilityEvolutionTimeoutSeconds = 10f;
    [SerializeField, Min(0.1f)] private float characterRecordTimeoutSeconds = 8f;
    [SerializeField, Min(0.1f)] private float bubbleTimeoutSeconds = 4f;
    [SerializeField, Min(0.1f)] private float bubbleMaxQueueAgeSeconds = 3f;
    [SerializeField, ReadOnly] private int runningCount;
    [SerializeField, ReadOnly] private int droppedBubbleCount;
    [SerializeField, ReadOnly] private int timeoutCount;
    [SerializeField, ReadOnly] private string lastError;
    [SerializeField, ReadOnly] private bool suppressWarningLogsForDebug;

    private readonly List<LocalLlmQueuedRequest> queue = new List<LocalLlmQueuedRequest>();

    public int QueuedCount => queue.Count;
    public int RunningCount => runningCount;
    public int DroppedBubbleCount => droppedBubbleCount;
    public int TimeoutCount => timeoutCount;
    public int MaxQueueSize => maxQueueSize;
    public string LastError => lastError;
    public bool HasConfiguredEndpoint => !string.IsNullOrWhiteSpace(endpointUrl)
        && !string.IsNullOrWhiteSpace(modelName);

    public void ConfigureBubblePolicyForDebug(float timeoutSeconds, float maxQueueAgeSeconds)
    {
        bubbleTimeoutSeconds = Mathf.Max(0.1f, timeoutSeconds);
        bubbleMaxQueueAgeSeconds = Mathf.Max(0.1f, maxQueueAgeSeconds);
    }

    public void ConfigureTimeoutsForDebug(
        float personaSeconds,
        float macroGoalSeconds,
        float socialRumorSeconds,
        float bubbleSeconds,
        float moodImpulseSeconds = 8f,
        float facilityEvolutionSeconds = 10f,
        float characterRecordSeconds = 8f)
    {
        personaTimeoutSeconds = Mathf.Max(0.1f, personaSeconds);
        macroGoalTimeoutSeconds = Mathf.Max(0.1f, macroGoalSeconds);
        moodImpulseTimeoutSeconds = Mathf.Max(0.1f, moodImpulseSeconds);
        socialRumorTimeoutSeconds = Mathf.Max(0.1f, socialRumorSeconds);
        facilityEvolutionTimeoutSeconds = Mathf.Max(0.1f, facilityEvolutionSeconds);
        characterRecordTimeoutSeconds = Mathf.Max(0.1f, characterRecordSeconds);
        bubbleTimeoutSeconds = Mathf.Max(0.1f, bubbleSeconds);
    }

    public void ClearForDebug()
    {
        queue.Clear();
        droppedBubbleCount = 0;
        timeoutCount = 0;
        lastError = string.Empty;
        suppressWarningLogsForDebug = false;
    }

    public void AbortAllForDebug()
    {
        StopAllCoroutines();
        queue.Clear();
        runningCount = 0;
        droppedBubbleCount = 0;
        timeoutCount = 0;
        lastError = string.Empty;
        suppressWarningLogsForDebug = false;
    }

    public void SetWarningLogsSuppressedForDebug(bool value)
    {
        suppressWarningLogsForDebug = value;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Multiple LocalLlmRequestQueue instances exist. Remove the duplicate.", this);
            enabled = false;
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Update()
    {
        DropExpiredBubbleRequests();
        while (runningCount < Mathf.Max(1, maxConcurrentRequests) && queue.Count > 0)
        {
            int index = FindNextRequestIndex();
            LocalLlmQueuedRequest request = queue[index];
            queue.RemoveAt(index);
            StartCoroutine(ProcessRequest(request));
        }
    }

    public bool EnqueuePersona(string prompt, Action<LocalLlmResult> callback)
    {
        return GeneratePersonaAsync(prompt, callback);
    }

    public bool EnqueueMacroGoal(string prompt, Action<LocalLlmResult> callback)
    {
        return GenerateMacroGoalAsync(prompt, callback);
    }

    public bool EnqueueMoodImpulse(string prompt, Action<LocalLlmResult> callback)
    {
        return GenerateMoodImpulseAsync(prompt, callback);
    }

    public bool EnqueueSocialRumor(string prompt, Action<LocalLlmResult> callback)
    {
        return GenerateSocialRumorAsync(prompt, callback);
    }

    public bool EnqueueFacilityEvolution(string prompt, Action<LocalLlmResult> callback)
    {
        return GenerateFacilityEvolutionAsync(prompt, callback);
    }

    public bool EnqueueCharacterRecord(string prompt, string originalText, Action<LocalLlmResult> callback)
    {
        return GenerateCharacterRecordAsync(prompt, originalText, callback);
    }

    public bool EnqueueBubbleLine(string prompt, string originalText, Action<LocalLlmResult> callback)
    {
        return GenerateBubbleLineAsync(prompt, originalText, callback);
    }

    public bool GeneratePersonaAsync(string prompt, Action<LocalLlmResult> callback)
    {
        return Enqueue(LocalLlmRequestType.Persona, prompt, string.Empty, personaTimeoutSeconds, callback);
    }

    public bool GenerateMacroGoalAsync(string prompt, Action<LocalLlmResult> callback)
    {
        return Enqueue(LocalLlmRequestType.MacroGoal, prompt, string.Empty, macroGoalTimeoutSeconds, callback);
    }

    public bool GenerateMoodImpulseAsync(string prompt, Action<LocalLlmResult> callback)
    {
        return Enqueue(LocalLlmRequestType.MoodImpulse, prompt, string.Empty, moodImpulseTimeoutSeconds, callback);
    }

    public bool GenerateSocialRumorAsync(string prompt, Action<LocalLlmResult> callback)
    {
        return Enqueue(LocalLlmRequestType.SocialRumor, prompt, string.Empty, socialRumorTimeoutSeconds, callback);
    }

    public bool GenerateFacilityEvolutionAsync(string prompt, Action<LocalLlmResult> callback)
    {
        return Enqueue(LocalLlmRequestType.FacilityEvolution, prompt, string.Empty, facilityEvolutionTimeoutSeconds, callback);
    }

    public bool GenerateCharacterRecordAsync(
        string prompt,
        string originalText,
        Action<LocalLlmResult> callback)
    {
        return Enqueue(
            LocalLlmRequestType.CharacterRecord,
            prompt,
            originalText,
            characterRecordTimeoutSeconds,
            callback);
    }

    public bool GenerateBubbleLineAsync(string prompt, string originalText, Action<LocalLlmResult> callback)
    {
        return Enqueue(LocalLlmRequestType.BubbleLine, prompt, originalText, bubbleTimeoutSeconds, callback);
    }

    private bool Enqueue(
        LocalLlmRequestType type,
        string prompt,
        string originalText,
        float timeoutSeconds,
        Action<LocalLlmResult> callback)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            callback?.Invoke(new LocalLlmResult(
                LocalLlmRequestStatus.Failed,
                string.Empty,
                "LLM prompt is empty.",
                originalText));
            return false;
        }

        if (queue.Count >= maxQueueSize && !TryDropLowestPriorityBubble())
        {
            if (type == LocalLlmRequestType.BubbleLine)
            {
                droppedBubbleCount++;
                callback?.Invoke(new LocalLlmResult(
                    LocalLlmRequestStatus.Dropped,
                    string.Empty,
                    "Bubble request dropped because the LLM queue is full.",
                    originalText));
                return false;
            }

            lastError = $"{type}: Failed - LLM queue is full and no bubble request can be dropped.";
            callback?.Invoke(new LocalLlmResult(
                LocalLlmRequestStatus.Failed,
                string.Empty,
                "LLM queue is full and no bubble request can be dropped.",
                originalText));
            LogWarningIfAllowed(lastError);
            return false;
        }

        queue.Add(new LocalLlmQueuedRequest
        {
            Type = type,
            Prompt = prompt,
            OriginalText = originalText,
            TimeoutSeconds = Mathf.Max(0.1f, timeoutSeconds),
            EnqueuedAt = Time.time,
            Callback = callback
        });
        return true;
    }

    private IEnumerator ProcessRequest(LocalLlmQueuedRequest request)
    {
        runningCount++;
        try
        {
            if (!HasConfiguredEndpoint)
            {
                Complete(request, new LocalLlmResult(
                    LocalLlmRequestStatus.Failed,
                    string.Empty,
                    "Local LLM endpoint or model is not configured.",
                    request.OriginalText));
                yield break;
            }

            using UnityWebRequest webRequest = BuildRequest(request.Type, request.Prompt);
            webRequest.timeout = Mathf.CeilToInt(request.TimeoutSeconds);
            UnityWebRequestAsyncOperation operation = webRequest.SendWebRequest();
            float timeoutAt = Time.realtimeSinceStartup + Mathf.Max(0.1f, request.TimeoutSeconds);
            while (!operation.isDone && Time.realtimeSinceStartup < timeoutAt)
            {
                yield return null;
            }

            if (!operation.isDone)
            {
                webRequest.Abort();
                Complete(request, new LocalLlmResult(
                    LocalLlmRequestStatus.TimedOut,
                    string.Empty,
                    "Request timeout",
                    request.OriginalText));
                yield break;
            }

            if (webRequest.result == UnityWebRequest.Result.ConnectionError
                || webRequest.result == UnityWebRequest.Result.ProtocolError
                || webRequest.result == UnityWebRequest.Result.DataProcessingError)
            {
                LocalLlmRequestStatus status = webRequest.error != null
                    && webRequest.error.IndexOf("timed out", StringComparison.OrdinalIgnoreCase) >= 0
                        ? LocalLlmRequestStatus.TimedOut
                        : LocalLlmRequestStatus.Failed;
                Complete(request, new LocalLlmResult(
                    status,
                    string.Empty,
                    webRequest.error,
                    request.OriginalText));
                yield break;
            }

            if (!TryExtractContent(webRequest.downloadHandler.text, out string content, out string error))
            {
                Complete(request, new LocalLlmResult(
                    LocalLlmRequestStatus.Failed,
                    string.Empty,
                    error,
                    request.OriginalText));
                yield break;
            }

            Complete(request, new LocalLlmResult(
                LocalLlmRequestStatus.Succeeded,
                content,
                string.Empty,
                request.OriginalText));
        }
        finally
        {
            runningCount = Mathf.Max(0, runningCount - 1);
        }
    }

    private UnityWebRequest BuildRequest(LocalLlmRequestType type, string prompt)
    {
        OpenAiChatRequest payload = new OpenAiChatRequest
        {
            model = modelName,
            temperature = type == LocalLlmRequestType.CharacterRecord ? 0.7f : 0.4f,
            messages = new[]
            {
                new OpenAiChatMessage
                {
                    role = "system",
                    content = "Return exactly one compact JSON object. Do not add markdown."
                },
                new OpenAiChatMessage
                {
                    role = "user",
                    content = prompt
                }
            },
            response_format = enableJsonMode ? new OpenAiResponseFormat { type = "json_object" } : null
        };

        string json = JsonUtility.ToJson(payload);
        UnityWebRequest request = new UnityWebRequest(endpointUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }

    private void Complete(LocalLlmQueuedRequest request, LocalLlmResult result)
    {
        if (result.Status == LocalLlmRequestStatus.TimedOut)
        {
            timeoutCount++;
        }

        if (!result.IsSuccess)
        {
            lastError = $"{request.Type}: {result.Status} - {result.Error}";
            LogWarningIfAllowed(lastError);
        }

        request.Callback?.Invoke(result);
    }

    private void LogWarningIfAllowed(string message)
    {
        if (suppressWarningLogsForDebug)
        {
            return;
        }

        Debug.LogWarning($"{name}: {message}", this);
    }

    private bool TryDropLowestPriorityBubble()
    {
        int index = -1;
        float oldestTime = float.MaxValue;
        for (int i = 0; i < queue.Count; i++)
        {
            LocalLlmQueuedRequest request = queue[i];
            if (request.Type != LocalLlmRequestType.BubbleLine || request.EnqueuedAt >= oldestTime)
            {
                continue;
            }

            oldestTime = request.EnqueuedAt;
            index = i;
        }

        if (index < 0)
        {
            return false;
        }

        LocalLlmQueuedRequest dropped = queue[index];
        queue.RemoveAt(index);
        droppedBubbleCount++;
        dropped.Callback?.Invoke(new LocalLlmResult(
            LocalLlmRequestStatus.Dropped,
            string.Empty,
            "Bubble request dropped by queue pressure.",
            dropped.OriginalText));
        return true;
    }

    private void DropExpiredBubbleRequests()
    {
        if (queue.Count == 0)
        {
            return;
        }

        for (int i = queue.Count - 1; i >= 0; i--)
        {
            LocalLlmQueuedRequest request = queue[i];
            if (request.Type != LocalLlmRequestType.BubbleLine
                || Time.time - request.EnqueuedAt <= bubbleMaxQueueAgeSeconds)
            {
                continue;
            }

            queue.RemoveAt(i);
            droppedBubbleCount++;
            request.Callback?.Invoke(new LocalLlmResult(
                LocalLlmRequestStatus.Dropped,
                string.Empty,
                "Bubble request expired in the LLM queue.",
                request.OriginalText));
        }
    }

    private int FindNextRequestIndex()
    {
        int bestIndex = 0;
        int bestPriority = int.MinValue;
        float bestEnqueuedAt = float.MaxValue;
        for (int i = 0; i < queue.Count; i++)
        {
            int priority = GetPriority(queue[i].Type);
            if (priority > bestPriority
                || (priority == bestPriority && queue[i].EnqueuedAt < bestEnqueuedAt))
            {
                bestIndex = i;
                bestPriority = priority;
                bestEnqueuedAt = queue[i].EnqueuedAt;
            }
        }

        return bestIndex;
    }

    private static int GetPriority(LocalLlmRequestType type)
    {
        return type switch
        {
            LocalLlmRequestType.Persona => 30,
            LocalLlmRequestType.MacroGoal => 20,
            LocalLlmRequestType.MoodImpulse => 18,
            LocalLlmRequestType.FacilityEvolution => 16,
            LocalLlmRequestType.SocialRumor => 15,
            LocalLlmRequestType.CharacterRecord => 12,
            LocalLlmRequestType.BubbleLine => 10,
            _ => 0
        };
    }

    private static bool TryExtractContent(string responseJson, out string content, out string error)
    {
        content = string.Empty;
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            error = "LLM HTTP response is empty.";
            return false;
        }

        OpenAiChatResponse response;
        try
        {
            response = JsonUtility.FromJson<OpenAiChatResponse>(responseJson);
        }
        catch (Exception exception)
        {
            error = $"LLM HTTP response parse failed: {exception.Message}";
            return false;
        }

        if (response == null || response.choices == null || response.choices.Length == 0)
        {
            error = "LLM HTTP response has no choices.";
            return false;
        }

        content = response.choices[0].message != null
            ? response.choices[0].message.content
            : response.choices[0].text;
        if (string.IsNullOrWhiteSpace(content))
        {
            error = "LLM HTTP choice has no content.";
            return false;
        }

        return true;
    }

    [Serializable]
    private sealed class OpenAiChatRequest
    {
        public string model;
        public OpenAiChatMessage[] messages;
        public float temperature;
        public OpenAiResponseFormat response_format;
    }

    [Serializable]
    private sealed class OpenAiChatMessage
    {
        public string role;
        public string content;
    }

    [Serializable]
    private sealed class OpenAiResponseFormat
    {
        public string type;
    }

    [Serializable]
    private sealed class OpenAiChatResponse
    {
        public OpenAiChoice[] choices;
    }

    [Serializable]
    private sealed class OpenAiChoice
    {
        public OpenAiChatMessage message;
        public string text;
    }
}
