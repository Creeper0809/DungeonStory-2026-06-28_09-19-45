using System;
using System.Collections.Generic;
using System.Linq;

public enum EventAlertImportance
{
    Low,
    Medium,
    High
}

public class EventAlertChoice
{
    public string Label { get; }
    public string Description { get; }
    public Action Callback { get; }

    public EventAlertChoice(string label, string description = "", Action callback = null)
    {
        Label = string.IsNullOrWhiteSpace(label) ? "Choice" : label;
        Description = description ?? string.Empty;
        Callback = callback;
    }
}

public class EventAlertRequest
{
    public string Title { get; }
    public string Detail { get; }
    public EventAlertImportance Importance { get; }
    public string Category { get; }
    public IReadOnlyList<EventAlertChoice> Choices { get; }

    public EventAlertRequest(
        string title,
        string detail,
        EventAlertImportance importance,
        string category = "",
        IEnumerable<EventAlertChoice> choices = null)
    {
        Title = string.IsNullOrWhiteSpace(title) ? "Event" : title;
        Detail = detail ?? string.Empty;
        Importance = importance;
        Category = category ?? string.Empty;
        Choices = NormalizeChoices(choices);
    }

    private static IReadOnlyList<EventAlertChoice> NormalizeChoices(IEnumerable<EventAlertChoice> choices)
    {
        return choices?
            .Where((choice) => choice != null)
            .Take(3)
            .ToList()
            ?? new List<EventAlertChoice>();
    }
}

public class EventAlertRecord
{
    public int Id { get; }
    public string Title { get; }
    public string Detail { get; }
    public EventAlertImportance Importance { get; }
    public string Category { get; }
    public int Count { get; private set; }
    public IReadOnlyList<EventAlertChoice> Choices { get; }

    public EventAlertRecord(int id, EventAlertRequest request)
    {
        Id = id;
        Title = request.Title;
        Detail = request.Detail;
        Importance = request.Importance;
        Category = request.Category;
        Choices = request.Choices;
        Count = 1;
    }

    public void Increment()
    {
        Count++;
    }

    public string ButtonText => Count > 1 ? $"{Title} x{Count}" : Title;

    public string ToDetailText()
    {
        List<string> lines = new List<string>
        {
            Title,
            $"중요도: {GetImportanceName(Importance)}",
        };

        if (!string.IsNullOrWhiteSpace(Category))
        {
            lines.Add($"분류: {Category}");
        }

        if (Count > 1)
        {
            lines.Add($"반복: {Count}");
        }

        if (!string.IsNullOrWhiteSpace(Detail))
        {
            lines.Add(string.Empty);
            lines.Add(Detail);
        }

        if (Choices.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("선택지:");
            foreach (EventAlertChoice choice in Choices)
            {
                lines.Add(string.IsNullOrWhiteSpace(choice.Description)
                    ? $"- {choice.Label}"
                    : $"- {choice.Label}: {choice.Description}");
            }
        }

        return string.Join("\n", lines);
    }

    private static string GetImportanceName(EventAlertImportance importance)
    {
        return importance switch
        {
            EventAlertImportance.Low => "낮음",
            EventAlertImportance.Medium => "중간",
            EventAlertImportance.High => "높음",
            _ => importance.ToString()
        };
    }
}
