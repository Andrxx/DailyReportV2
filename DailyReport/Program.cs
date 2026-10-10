using DailyReport.Components;
using DailyReport.Models;
using DailyReport.Models.Reports;
using DailyReport.Services.Reports;
using DailyReport.Services.Personnel;
using DailyReport.Services.Reporting;
using DailyReport.Services.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlite(
    builder.Configuration.GetConnectionString("localDb")));

builder.Services.AddScoped<DepReportService>();
builder.Services.AddScoped<SummaryService>();
builder.Services.AddScoped<PersonelService>();
builder.Services.AddScoped<DutyService>();
builder.Services.AddScoped<OutPatientService>();

// пароли и права доступа: секция "Security" конфигурации, переопределяется переменными окружения
builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection("Security"));
builder.Services.AddScoped<IAccessControl, PasswordAccessControl>();

// отчётные сутки: секция "Reporting" конфигурации, переопределяется переменными окружения
builder.Services.Configure<ReportingOptions>(builder.Configuration.GetSection("Reporting"));
builder.Services.AddSingleton<ReportingCalendar>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var reports = app.MapGroup("/api/reports");

reports.MapGet("/{depNumber}", (DepReportService svc, ReportingCalendar cal, int depNumber, DateOnly? reportDate) =>
{
    var day = reportDate ?? cal.Today;
    return svc.GetByNumber(depNumber, day) is { } rep
        ? Results.Ok(rep)
        : Results.NotFound($"Отчёт для отделения {depNumber} за сутки {day:dd.MM.yyyy} не найден");
}).WithName("GetDepReport");

reports.MapPost("/", (DepReportService svc, DepReport report) =>
{
    svc.AddOrUpdate(report);
    return Results.Ok(report);
}).WithName("SaveDepReport");

app.Run();