using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1;

public partial class Settings : Window
{
    private uint _minUptime;
    private int _alertFreq;
    private bool _showOnStartup;
    private bool _spamEnabled;
    private int _spamFreq;

    private string _path = "../../../appsettings.json";

    private App.AppSettings _defaultSettings;

    public Settings()
    {
        InitializeComponent();

        string settingsFile = File.ReadAllText(_path);
        var settings = JsonSerializer.Deserialize<App.AppSettings>(settingsFile);
        _defaultSettings = settings;
        
        var uptimeValues = new[]
        {
            new KeyValuePair<uint, string>(1, "1 Day"),
            new KeyValuePair<uint, string>(5, "5 Days"),
            new KeyValuePair<uint, string>(30, "30 Days")
        };
        var alertFreqs = new[]
        {
            new KeyValuePair<int, string>(1, "1 hour"),
            new KeyValuePair<int, string>(3, "3 hour"),
            new KeyValuePair<int, string>(6, "6 hour"),
            new KeyValuePair<int, string>(8, "8 hour"),
            new KeyValuePair<int, string>(-1, "Never")
        };
        var spamFreqs = new[]
        {
            new KeyValuePair<int, string>(5, "5 minutes"),
            new KeyValuePair<int, string>(10, "10 minutes"),
            new KeyValuePair<int, string>(20, "20 minutes"),
        };
        UptimeValue.ItemsSource = uptimeValues;
        UptimeValue.SelectedItem = uptimeValues.Single(x => x.Key == settings.MinUpTime_Days);
        
        AlertFreq.ItemsSource = alertFreqs;
        AlertFreq.SelectedItem = alertFreqs.Single(x => x.Key == settings.AlertFrequency_Hours);
        
        SpamFreq.ItemsSource = spamFreqs;
        SpamFreq.SelectedItem = spamFreqs.Single(x => x.Key == settings.SpamFrequency_Minutes);

        SpamMode.IsChecked = settings.SpamEnabled;
        ShowOnStartUp.IsChecked = settings.ShowSettingsOnStartup;
        
        _minUptime = (uint)UptimeValue.SelectedValue;
        _alertFreq = (int)AlertFreq.SelectedValue;
        _showOnStartup = (bool)SpamMode.IsChecked;
        _spamEnabled = (bool)ShowOnStartUp.IsChecked;
        _spamFreq = (int)SpamFreq.SelectedValue;
    }

    private void ShowOnStartUp_OnClick(object sender, RoutedEventArgs e)
    {
        _showOnStartup = (bool)ShowOnStartUp.IsChecked!;
    }

    private void UptimeValue_OnDropDownClosed(object? sender, EventArgs e)
    {
        // set uptime minimum
        _minUptime = (uint)UptimeValue.SelectedValue;
    }

    private void AlertFreq_OnDropDownClosed(object? sender, EventArgs e)
    {
        // set alert message frequency
        _alertFreq = (int)AlertFreq.SelectedValue;
    }

    private void SpamMode_OnClick(object sender, RoutedEventArgs e)
    {
        _spamEnabled = (bool)SpamMode.IsChecked!;
    }

    private void SpamFreq_OnDropDownClosed(object? sender, EventArgs e)
    {
        _spamFreq = (int)SpamFreq.SelectedValue;
    }
    
    private void Reset_OnClick(object sender, RoutedEventArgs e)
    {
        
    }
    
    private void Save_OnClick(object sender, RoutedEventArgs e)
    {
        var settings = new App.AppSettings
        {
            ShowSettingsOnStartup = _showOnStartup,
            MinUpTime_Days = _minUptime,
            AlertFrequency_Hours = _alertFreq,
            SpamEnabled = _spamEnabled,
            SpamFrequency_Minutes = _spamFreq
        };
        WriteSettings(_path, settings);
        Hide();
    }
    
    private void ShowAlert(TimeSpan time)
    {
        MessageBox.Show($"Computer hasnt been restarted in {UptimeValue.Text}" +
                        "\nFix your shit." + 
                        $"\n\nUptime: {time}",
            "Uptime Warning",
            MessageBoxButton.OK,
            MessageBoxImage.Information, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
    }
    
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    private async void WriteSettings(string path, App.AppSettings appSettings)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, appSettings, _options);
    }

    public void CheckUptime()
    {
        long ticks = Environment.TickCount64;
        var timespan = TimeSpan.FromMilliseconds(ticks);
        if (timespan.TotalMicroseconds > TimeSpan.FromDays(_minUptime).TotalMicroseconds)
        {
            ShowAlert(timespan);
        }
    }

    private void OpenJson_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        Process.Start("explorer.exe", _path);
    }
}