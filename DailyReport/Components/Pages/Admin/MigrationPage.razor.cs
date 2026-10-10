using DailyReport.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace DailyReport.Components.Pages.Admin;

public partial class MigrationPage
{
    [Inject] private ApplicationContext Context { get; set; } = default!;

    public bool IsRunning { get; set; }

    public string? Message { get; set; }

    private async Task RunMigration()
    {
        if (IsRunning) return;
        IsRunning = true;
        Message = null;
        try
        {
            var pending = Context.Database.GetPendingMigrations().ToList();
            Context.Database.Migrate();
            var applied = Context.Database.GetAppliedMigrations().Count();
            Message = pending.Count == 0
                ? $"Миграций для применения нет. Всего применено: {applied}."
                : $"Применено миграций: {pending.Count}. Всего применено: {applied}.";
        }
        catch (Exception ex)
        {
            Message = $"Ошибка при применении миграций: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
        }
    }
}