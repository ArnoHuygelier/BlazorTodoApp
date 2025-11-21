using System;
using System.Collections.Generic;
using BlazorTodoApp.Components;
using BlazorTodoApp.Models;
using BlazorTodoApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace BlazorTodoApp.Pages;

public sealed partial class TodoDashboard : IDisposable
{
    private readonly TodoFormModel formModel = new();
    private IReadOnlyList<TodoItem> visibleTodos = Array.Empty<TodoItem>();
    private DashboardSummary summary = DashboardSummary.Empty;
    private bool isModalOpen;
    private bool isFormBusy;
    private Guid? editingTodoId;
    private TodoEditMode editMode = TodoEditMode.Create;
    private string? formError;
    private ErrorBoundary? errorBoundary;

    [Inject]
    public TodoStateService StateService { get; set; } = default!;

    [Inject]
    public ILogger<TodoDashboard> Logger { get; set; } = default!;

    private string ModalTitle => editMode == TodoEditMode.Create ? "Add todo" : "Edit todo";

    protected override async Task OnInitializedAsync()
    {
        StateService.StateChanged += HandleStateChanged;
        await StateService.InitializeAsync();
        SyncDerivedState();
    }

    private async void HandleStateChanged(object? sender, EventArgs args)
    {
        SyncDerivedState();
        await InvokeAsync(StateHasChanged);
    }

    private void OpenCreateModal()
    {
        errorBoundary?.Recover();
        editMode = TodoEditMode.Create;
        editingTodoId = null;
        formError = null;
        formModel.LoadFrom(null);
        isModalOpen = true;
    }

    private void HandleEditRequested(TodoItem item)
    {
        errorBoundary?.Recover();
        editMode = TodoEditMode.Edit;
        editingTodoId = item.Id;
        formError = null;
        formModel.LoadFrom(item);
        isModalOpen = true;
    }

    private async Task HandleFormSubmitAsync(TodoFormModel _)
    {
        errorBoundary?.Recover();
        Logger.LogInformation("Submitting todo form in {Mode} mode for TodoId={TodoId}.", editMode, editingTodoId);
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

    private async Task HandleToggleAsync(TodoItem item)
    {
        errorBoundary?.Recover();
        try
        {
            Logger.LogInformation("Toggle requested for todo {TodoId}.", item.Id);
            await StateService.ToggleTodoAsync(item.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to toggle todo {TodoId}.", item.Id);
            throw;
        }
    }

    private async Task HandleDeleteAsync(TodoItem item)
    {
        errorBoundary?.Recover();
        try
        {
            Logger.LogInformation("Delete requested for todo {TodoId}.", item.Id);
            await StateService.DeleteTodoAsync(item.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete todo {TodoId}.", item.Id);
            throw;
        }
    }

    private async Task HandleFilterChanged(TodoFilterOption selection)
    {
        if (StateService.CurrentFilter.Selection == selection)
        {
            return;
        }

        try
        {
            Logger.LogInformation("Filter change requested: {Selection}.", selection);
            await StateService.SetFilterAsync(selection);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to update todo filter to {Selection}.", selection);
        }
        finally
        {
            SyncDerivedState();
            await InvokeAsync(StateHasChanged);
        }
    }

    private void CloseModal()
    {
        isModalOpen = false;
        editingTodoId = null;
        formModel.LoadFrom(null);
    }

    private void SyncDerivedState()
    {
        summary = StateService.Summary;
        visibleTodos = StateService.GetFilteredItems();
    }

    private async Task RecoverFromError()
    {
        Logger.LogWarning("Recovering dashboard after error.");
        errorBoundary?.Recover();
        await StateService.InitializeAsync();
        SyncDerivedState();
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        if (StateService is not null)
        {
            StateService.StateChanged -= HandleStateChanged;
        }
    }
}
