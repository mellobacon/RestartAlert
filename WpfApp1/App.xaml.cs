using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Hardcodet.Wpf.TaskbarNotification;
using Application = System.Windows.Application;

namespace WpfApp1;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly string _path = "../../../appsettings.json";
    private AppSettings _appsettings;
    public struct AppSettings()
    {
        public bool ShowSettingsOnStartup { get; set; } = false;
        public uint MinUpTime_Days { get; set; } = 1;
        public int AlertFrequency_Hours { get; set; } = 1;
        public bool SpamEnabled { get; set; } = false;
        public int SpamFrequency_Minutes { get; set; } = 5;
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        LoadSettings();
        
        var settings = new Settings();
        var icon = new TaskbarIcon();
        icon.Icon = new Icon("../../../Icons/icon.ico");
        var menu = new ContextMenu
        {
            Placement = PlacementMode.Left
        };
        var settingsmenu = new MenuItem() { Header = "Settings"};
        settingsmenu.Click += async (sender, args) =>
        {
            await Task.Run(() => Thread.Sleep(200));
            settings.Show();
        };
        var exit = new MenuItem() {Header = "Exit"};
        exit.Click += async (sender, args) =>
        {
            await Task.Run(() => Thread.Sleep(200));
            Shutdown();
        };
        
        menu.Items.Add(settingsmenu);
        menu.Items.Add(exit);
        icon.ContextMenu = menu;
        icon.MenuActivation = PopupActivationMode.LeftOrRightClick;

        if ((bool)settings.ShowOnStartUp.IsChecked!)
        {
            settings.Show();
        }
        
        try
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    CheckUptime();
                    Thread.Sleep(TimeSpan.FromHours(_appsettings.AlertFrequency_Hours));
                }
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            Console.WriteLine(ex.Message);
        }
    }

    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    private void LoadSettings()
    {
        if (File.Exists(_path))
        {
            Console.WriteLine("Settings file found...");
            string settingsFile = File.ReadAllText(_path);
            _appsettings = JsonSerializer.Deserialize<AppSettings>(settingsFile);
            return;
        }
        WriteSettings(_path, new AppSettings());
    }

    private async void WriteSettings(string path, AppSettings appSettings)
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
        if (timespan.TotalMicroseconds > TimeSpan.FromDays(_appsettings.MinUpTime_Days).TotalMicroseconds)
        {
            ShowAlert(timespan);
        }
    }
    
    private void ShowAlert(TimeSpan time)
    {
        MessageBox.Show($"Computer hasnt been restarted in {_appsettings.MinUpTime_Days} day(s)" +
                        "\nFix your shit." + 
                        $"\n\nUptime: {time}",
            "Uptime Warning",
            MessageBoxButton.OK,
            MessageBoxImage.Information, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
    }
}
