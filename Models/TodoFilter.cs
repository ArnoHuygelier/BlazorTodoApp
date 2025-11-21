namespace BlazorTodoApp.Models;

public enum TodoFilterOption
{
    All = 0,
    Active = 1,
    Completed = 2
}

public sealed record TodoFilter(TodoFilterOption Selection)
{
    public static TodoFilter Default { get; } = new(TodoFilterOption.All);

    public static bool IsValid(TodoFilterOption selection) =>
        Enum.IsDefined(typeof(TodoFilterOption), selection);

    public static TodoFilter FromSelection(TodoFilterOption selection) =>
        new(IsValid(selection) ? selection : TodoFilterOption.All);

    public TodoFilter WithSelection(TodoFilterOption selection) =>
        FromSelection(selection);
}
