using System.ComponentModel.DataAnnotations;
using BlazorTodoApp.Models;

namespace BlazorTodoApp.Pages;

public sealed class TodoFormModel
{
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Note { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? DueDay { get; set; }

    public void LoadFrom(TodoItem? item)
    {
        if (item is null)
        {
            Title = string.Empty;
            Note = null;
            DueDay = null;
            return;
        }

        Title = item.Title;
        Note = item.Note;
        DueDay = item.DueDay;
    }
}
