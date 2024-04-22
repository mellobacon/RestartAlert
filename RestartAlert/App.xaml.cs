using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Serilog;
using Serilog.Core;
using Application = System.Windows.Application;

namespace RestartAlert;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly string _path = "appsettings.json";
    private readonly string _logpath = "Logs/log.txt";
    private static AppSettings _appSettings;
    private static DispatcherTimer _timer = new();
    private static Logger _log = new LoggerConfiguration().CreateLogger();

    public struct AppSettings()
    {
        public bool ShowSettingsOnStartup { get; set; } = false;

        public Dictionary<string, string> MinUpTime { get; set; } = new()
        {
            ["Value"] = "1",
            ["Unit"] = "Hour(s)"
        };

        public Dictionary<string, string> AlertFrequency { get; set; } = new()
        {
            ["Value"] = "1",
            ["Unit"] = "Hour(s)"
        };

        public bool SpamEnabled { get; set; } = false;

        public Dictionary<string, string> SpamFrequency { get; set; } = new()
        {
            ["Value"] = "1",
            ["Unit"] = "Minute(s)"
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _log = new LoggerConfiguration()
            .WriteTo.File(_logpath,
                rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        LoadSettings();
        
        _log.Information("Starting app...");

        var settings = new Settings(_appSettings);
        var icon = new TaskbarIcon();
        icon.Icon = new Icon("Icons/icon.ico");
        var menu = new ContextMenu
        {
            Placement = PlacementMode.Left
        };
        var settingsmenu = new MenuItem() { Header = "Settings" };
        settingsmenu.Click += (sender, args) => { settings.ShowDialog(); };
        var exit = new MenuItem() { Header = "Exit" };
        exit.Click += (sender, args) =>
        {
            _log.Information("Exiting app...");
            Shutdown();
            Log.CloseAndFlush();
        };

        menu.Items.Add(settingsmenu);
        menu.Items.Add(exit);
        icon.ContextMenu = menu;
        icon.MenuActivation = PopupActivationMode.LeftOrRightClick;

        if (settings.ShowOnStartUp.IsChecked is true)
        {
            settings.ShowDialog();
        }

        try
        {
            _timer = new DispatcherTimer
            {
                Interval = GetAlertFreq()
            };
            _timer.Tick += (sender, args) =>
            {
                CheckUptime();
            };
            _timer.Start();
            CheckUptime();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            Console.WriteLine(ex.Message);
        }
    }

    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    private void LoadSettings()
    {
        try
        {
            using var stream = File.OpenRead(_path);
            _appSettings = JsonSerializer.Deserialize<AppSettings>(stream);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            _log.Error(e.Message);
            _appSettings = new AppSettings();
        }
    }

    public static async Task WriteSettings(string path, AppSettings appSettings)
    {
        _log.Information("Saving settings...");
        try
        {
            await using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, appSettings, _options);
            _appSettings = appSettings;
            _log.Information("Settings saved");
            _timer.Interval = GetAlertFreq();
        
            var upcomingtime = (DateTime.Now + _timer.Interval).ToString("hh:mm:ss tt");
            _log.Information($"New alert time set...Next alert in {_timer.Interval} ({upcomingtime})...");
        }
        catch (Exception e)
        {
            _log.Fatal(e, "Cannot save settings");
        }
    }

    private void CheckUptime()
    {
        long ticks = Environment.TickCount64;
        var timespan = TimeSpan.FromMilliseconds(ticks);
        _log.Information($"Checking Uptime...{timespan}");
        if (timespan.TotalMilliseconds > GetMinUptime().TotalMilliseconds)
        {
            _log.Information("Min Uptime past threshold. Alerting...");
            ShowAlert(timespan);
        }
        var upcomingtime = (DateTime.Now + _timer.Interval).ToString("hh:mm:ss tt");
        _log.Information($"Check Done...Next alert in {_timer.Interval} ({upcomingtime})...");
    }

    private void ShowAlert(TimeSpan time)
    {
        _timer.Stop();
        MessageBox.Show(
            $"Computer hasnt been restarted in {_appSettings.MinUpTime["Value"]} {_appSettings.MinUpTime["Unit"]}" +
            "\nFix your shit." +
            $"\n\nUptime: {time}",
            "Uptime Warning",
            MessageBoxButton.OK,
            MessageBoxImage.Information, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
        _timer.Start();
    }
    
    private TimeSpan GetMinUptime()
    {
        return _appSettings.MinUpTime["Unit"] switch
        {
            "Hour(s)" => TimeSpan.FromHours(double.Parse(_appSettings.MinUpTime["Value"])),
            "Day(s)" => TimeSpan.FromDays(double.Parse(_appSettings.MinUpTime["Value"])),
            "Week(s)" => TimeSpan.FromDays(double.Parse(_appSettings.MinUpTime["Value"]) / 7),
            _ => TimeSpan.MaxValue
        };
    }

    private static TimeSpan GetAlertFreq()
    {
        return _appSettings.AlertFrequency["Unit"] switch
        {
            "Minute(s)" => TimeSpan.FromMinutes(double.Parse(_appSettings.AlertFrequency["Value"])),
            "Hour(s)" => TimeSpan.FromHours(double.Parse(_appSettings.AlertFrequency["Value"])),
            "Day(s)" => TimeSpan.FromDays(double.Parse(_appSettings.AlertFrequency["Value"])),
            "Week(s)" => TimeSpan.FromDays(double.Parse(_appSettings.AlertFrequency["Value"]) / 7),
            _ => TimeSpan.MaxValue
        };
    }
}
