using System;
using System.Collections.Generic;
using BlazorTodoApp.Components;
using BlazorTodoApp.Models;
using BlazorTodoApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazorTodoApp.Pages;

public sealed partial class TodoDashboard : IDisposable
{
    private readonly TodoFormModel formModel = new();
    private IReadOnlyList<TodoItem> visibleTodos = Array.Empty<TodoItem>();
    private bool isModalOpen;
    private bool isFormBusy;
    private Guid? editingTodoId;
    private TodoEditMode editMode = TodoEditMode.Create;
    private string? formError;

    [Inject]
    public TodoStateService StateService { get; set; } = default!;

    [Inject]
    public ILogger<TodoDashboard> Logger { get; set; } = default!;

    private string ModalTitle => editMode == TodoEditMode.Create ? "Add todo" : "Edit todo";

    protected override async Task OnInitializedAsync()
    {
        StateService.StateChanged += HandleStateChanged;
        await StateService.InitializeAsync();
        visibleTodos = StateService.GetFilteredItems();
    }

    private async void HandleStateChanged(object? sender, EventArgs args)
    {
        visibleTodos = StateService.GetFilteredItems();
        await InvokeAsync(StateHasChanged);
    }

    private void OpenCreateModal()
    {
        editMode = TodoEditMode.Create;
        editingTodoId = null;
        formError = null;
        formModel.LoadFrom(null);
        isModalOpen = true;
    }

    private void HandleEditRequested(TodoItem item)
    {
        editMode = TodoEditMode.Edit;
        editingTodoId = item.Id;
        formError = null;
        formModel.LoadFrom(item);
        isModalOpen = true;
    }

    private async Task HandleFormSubmitAsync(TodoFormModel _)
    {
        isFormBusy = true;
        formError = null;

        try
        {
            if (editingTodoId is null)
            {
                await StateService.AddTodoAsync(formModel.Title, formModel.Note, formModel.DueDay);
            }
            else
            {
                await StateService.UpdateTodoAsync(editingTodoId.Value, formModel.Title, formModel.Note, formModel.DueDay);
            }

            CloseModal();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to save todo.");
            formError = ex.Message;
        }
        finally
        {
            isFormBusy = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private Task HandleToggleAsync(TodoItem item) =>
        StateService.ToggleTodoAsync(item.Id);

    private Task HandleDeleteAsync(TodoItem item) =>
        StateService.DeleteTodoAsync(item.Id);

    private void CloseModal()
    {
        isModalOpen = false;
        editingTodoId = null;
        formModel.LoadFrom(null);
    }

    public void Dispose()
    {
        if (StateService is not null)
        {
            StateService.StateChanged -= HandleStateChanged;
        }
    }
}
