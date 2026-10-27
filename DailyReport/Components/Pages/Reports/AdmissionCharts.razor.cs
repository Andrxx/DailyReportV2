using System.Globalization;
using ApexCharts;
using DailyReport.Services.Reports;
using DailyReport.Services.Reporting;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DailyReport.Components.Pages.Reports
{
    public partial class AdmissionCharts : IAsyncDisposable
    {
        private static readonly DateOnly MinReportDate = new(2000, 1, 1);

        [Inject] private AdmissionStatsService StatsService { get; set; } = null!;
        [Inject] private ReportingCalendar Calendar { get; set; } = null!;
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private ILogger<AdmissionCharts> Logger { get; set; } = null!;

        [SupplyParameterFromQuery(Name = "date")]
        public string? Date { get; set; }

        private DateOnly AsOf { get; set; } = new(2000, 1, 1);
        private AdmissionStats? Stats { get; set; }
        private bool _ready;
        private bool _loading;
        private bool _isDark;
        private string? _error;
        private DotNetObjectReference<AdmissionCharts>? _dotNetRef;
        private ApexChartOptions<AdmissionDayPoint> _monthOptions = new();
        private ApexChartOptions<AdmissionDayPoint> _weekOptions = new();
        private ApexChartOptions<NozologySlice> _nozologyOptions = new();

        private string MonthKey => $"{AsOf:yyyyMMdd}-{(_isDark ? "d" : "l")}-month";
        private string WeekKey => $"{AsOf:yyyyMMdd}-{(_isDark ? "d" : "l")}-week";
        private string PieKey => $"{AsOf:yyyyMMdd}-{(_isDark ? "d" : "l")}-pie";

        protected override async Task OnParametersSetAsync()
        {
            var requested = ParseDate(Date) ?? Calendar.Today;

            if (requested > Calendar.Today)
            {
                requested = Calendar.Today;
            }

            if (requested < MinReportDate)
            {
                requested = MinReportDate;
            }

            if (Stats is not null && requested == AsOf)
            {
                return;
            }

            AsOf = requested;
            await LoadAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }

            try
            {
                _isDark = await JS.InvokeAsync<bool>("theme.current");
                _dotNetRef = DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("chartsTheme.watch", _dotNetRef);
            }
            catch (JSDisconnectedException)
            {
                _isDark = false;
            }

            _ready = true;
            BuildOptions();
            StateHasChanged();
        }

        [JSInvokable]
        public Task OnThemeChanged(bool dark)
        {
            if (_isDark == dark)
            {
                return Task.CompletedTask;
            }

            _isDark = dark;
            BuildOptions();
            return InvokeAsync(StateHasChanged);
        }

        private async Task LoadAsync()
        {
            _loading = true;
            _error = null;
            StateHasChanged();

            try
            {
                var day = AsOf;
                Stats = await Task.Run(() => StatsService.Build(day));
                BuildOptions();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Не удалось построить данные графиков за {Date}", AsOf);
                Stats = null;
                _error = "Не удалось загрузить данные графиков.";
            }
            finally
            {
                _loading = false;
                StateHasChanged();
            }
        }

        private void BuildOptions()
        {
            if (Stats is null)
            {
                return;
            }

            var palette = ChartPalette.For(_isDark);

            _monthOptions = new ApexChartOptions<AdmissionDayPoint>
            {
                Chart = new Chart
                {
                    Id = "month-admissions",
                    Width = "100%",
                    Background = "transparent",
                    ForeColor = palette.Text,
                    FontFamily = "\"Segoe UI Variable\", \"Segoe UI\", system-ui, sans-serif",
                    Toolbar = new Toolbar { Show = false },
                    Animations = new Animations { Enabled = false }
                },
                Colors = new List<string> { palette.Accent },
                Stroke = new Stroke { Curve = Curve.Smooth, Width = 3 },
                Markers = new Markers { Size = 3 },
                DataLabels = new DataLabels { Enabled = false },
                Legend = new Legend { Show = false },
                Grid = new Grid { BorderColor = palette.Grid },
                Xaxis = new XAxis
                {
                    Categories = Stats.Month.Select(p => (object)p.Label).ToList(),
                    TickAmount = 10
                },
                Yaxis = new List<YAxis>
                {
                    new()
                    {
                        Min = 0,
                        ForceNiceScale = true,
                        DecimalsInFloat = 0
                    }
                },
                Tooltip = new Tooltip { Shared = true, Intersect = false },
                Theme = new Theme { Mode = _isDark ? Mode.Dark : Mode.Light }
            };

            _weekOptions = new ApexChartOptions<AdmissionDayPoint>
            {
                Chart = new Chart
                {
                    Id = "week-admissions",
                    Width = "100%",
                    Background = "transparent",
                    ForeColor = palette.Text,
                    FontFamily = "\"Segoe UI Variable\", \"Segoe UI\", system-ui, sans-serif",
                    Toolbar = new Toolbar { Show = false },
                    Animations = new Animations { Enabled = false }
                },
                Colors = new List<string> { palette.Accent },
                PlotOptions = new PlotOptions
                {
                    Bar = new PlotOptionsBar
                    {
                        ColumnWidth = "50%",
                        BorderRadius = 4
                    }
                },
                DataLabels = new DataLabels { Enabled = true },
                Legend = new Legend { Show = false },
                Grid = new Grid { BorderColor = palette.Grid },
                Xaxis = new XAxis
                {
                    Categories = Stats.Week.Select(p => (object)p.Label).ToList(),
                    TickAmount = 7
                },
                Yaxis = new List<YAxis>
                {
                    new()
                    {
                        Min = 0,
                        ForceNiceScale = true,
                        DecimalsInFloat = 0
                    }
                },
                Tooltip = new Tooltip { Shared = true, Intersect = false },
                Theme = new Theme { Mode = _isDark ? Mode.Dark : Mode.Light }
            };

            _nozologyOptions = new ApexChartOptions<NozologySlice>
            {
                Chart = new Chart
                {
                    Id = "nozology-pie",
                    Width = "100%",
                    Background = "transparent",
                    ForeColor = palette.Text,
                    FontFamily = "\"Segoe UI Variable\", \"Segoe UI\", system-ui, sans-serif",
                    Toolbar = new Toolbar { Show = false },
                    Animations = new Animations { Enabled = false }
                },
                Colors = Stats.Nozologies
                    .Select(s => palette.Series[s.ColorIndex % palette.Series.Count])
                    .ToList(),
                DataLabels = new DataLabels { Enabled = false },
                Legend = new Legend
                {
                    Show = true,
                    Position = LegendPosition.Bottom,
                    HorizontalAlign = Align.Center,
                    FontSize = "11px",
                    ItemMargin = new LegendItemMargin { Horizontal = 6, Vertical = 2 },
                    ShowForZeroSeries = false
                },
                Stroke = new Stroke { Width = 1 },
                Theme = new Theme { Mode = _isDark ? Mode.Dark : Mode.Light }
            };
        }

        private static DateOnly? ParseDate(string? raw)
        {
            return DateOnly.TryParseExact(
                raw,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed)
                ? parsed
                : null;
        }

        public async ValueTask DisposeAsync()
        {
            if (_dotNetRef is null)
            {
                return;
            }

            try
            {
                await JS.InvokeVoidAsync("chartsTheme.dispose");
            }
            catch (JSDisconnectedException)
            {
            }

            _dotNetRef?.Dispose();
        }

        private sealed record ChartPalette(
            string Text,
            string Grid,
            string Accent,
            IReadOnlyList<string> Series)
        {
            private static readonly ChartPalette Light = new(
                "#1A1A1A",
                "#E0E0E0",
                "#0F6CBD",
                new[]
                {
                    "#0F6CBD", "#107C10", "#C239B3", "#D13438", "#FF8C00",
                    "#5C2D91", "#00B7C3", "#69797E", "#A4262C", "#8E562E",
                    "#4F6BED", "#498205", "#986F0B", "#7A7574"
                });

            private static readonly ChartPalette Dark = new(
                "#F5F5F5",
                "#484644",
                "#479EF5",
                new[]
                {
                    "#479EF5", "#54B054", "#E36DE0", "#FF6B6B", "#F9A825",
                    "#B69CFF", "#4DD0E1", "#B0B0B0", "#FF8A80", "#D7A86E",
                    "#8FA6FF", "#8BC34A", "#FFD54F", "#C7C6C5"
                });

            public static ChartPalette For(bool dark) => dark ? Dark : Light;
        }
    }
}
