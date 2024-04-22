using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Application = System.Windows.Application;

namespace WpfApp1;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly string _path = "../../../appsettings.json";
    public static AppSettings _appsettings;

    public struct AppSettings()
    {
        public bool ShowSettingsOnStartup { get; set; } = false;

        public Dictionary<string, string> MinUpTime { get; set; } = new()
        {
            ["Value"] = "1",
            ["Unit"] = "Hours"
        };

        public Dictionary<string, string> AlertFrequency { get; set; } = new()
        {
            ["Value"] = "1",
            ["Unit"] = "Hours"
        };

        public bool SpamEnabled { get; set; } = false;

        public Dictionary<string, string> SpamFrequency { get; set; } = new()
        {
            ["Value"] = "1",
            ["Unit"] = "Minutes"
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _ = LoadSettings();

        var settings = new Settings();
        var icon = new TaskbarIcon();
        icon.Icon = new Icon("../../../Icons/icon.ico");
        var menu = new ContextMenu
        {
            Placement = PlacementMode.Left
        };
        var settingsmenu = new MenuItem() { Header = "Settings" };
        settingsmenu.Click += (sender, args) => { settings.ShowDialog(); };
        var exit = new MenuItem() { Header = "Exit" };
        exit.Click += (sender, args) => { Shutdown(); };

        menu.Items.Add(settingsmenu);
        menu.Items.Add(exit);
        icon.ContextMenu = menu;
        icon.MenuActivation = PopupActivationMode.LeftOrRightClick;

        if (settings.ShowOnStartUp.IsChecked is true)
        {
            settings.ShowDialog();
        }
        
        CheckUptime();

        try
        {
            var timer = new DispatcherTimer
            {
                Interval = GetAlertFreq()
            };
            timer.Tick += (sender, args) =>
            {
                CheckUptime();
            };
            timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            Console.WriteLine(ex.Message);
        }
    }

    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    private async Task LoadSettings()
    {
        try
        {
            await using var stream = File.OpenRead(_path);
            _appsettings = JsonSerializer.Deserialize<AppSettings>(stream);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _appsettings = new AppSettings();
        }
    }

    public static async Task WriteSettings(string path, AppSettings appSettings)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, appSettings, _options);
        _appsettings = appSettings;
    }

    private void CheckUptime()
    {
        Console.WriteLine("Checking uptime...");
        long ticks = Environment.TickCount64;
        var timespan = TimeSpan.FromMilliseconds(ticks);
        if (timespan.TotalMilliseconds > GetMinUptime().TotalMilliseconds)
        {
            ShowAlert(timespan);
        }
    }

    private void ShowAlert(TimeSpan time)
    {
        MessageBox.Show(
            $"Computer hasnt been restarted in {_appsettings.MinUpTime["Value"]} {_appsettings.MinUpTime["Unit"]}" +
            "\nFix your shit." +
            $"\n\nUptime: {time}",
            "Uptime Warning",
            MessageBoxButton.OK,
            MessageBoxImage.Information, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
    }

    private TimeSpan GetMinUptime()
    {
        return _appsettings.MinUpTime["Unit"] switch
        {
            "Hours" => TimeSpan.FromHours(double.Parse(_appsettings.MinUpTime["Value"])),
            "Days" => TimeSpan.FromDays(double.Parse(_appsettings.MinUpTime["Value"])),
            "Weeks" => TimeSpan.FromDays(double.Parse(_appsettings.MinUpTime["Value"]) / 7),
            _ => TimeSpan.MaxValue
        };
    }

    private TimeSpan GetAlertFreq()
    {
        return _appsettings.AlertFrequency["Unit"] switch
        {
            "Hours" => TimeSpan.FromHours(double.Parse(_appsettings.AlertFrequency["Value"])),
            "Days" => TimeSpan.FromDays(double.Parse(_appsettings.AlertFrequency["Value"])),
            "Weeks" => TimeSpan.FromDays(double.Parse(_appsettings.AlertFrequency["Value"]) / 7),
            _ => TimeSpan.Zero
        };
    }
}
