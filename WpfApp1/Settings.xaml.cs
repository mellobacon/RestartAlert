using System.ComponentModel;
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

    private readonly string _path = "../../../appsettings.json";
    private readonly App.AppSettings _defaultSettings;

    private readonly KeyValuePair<uint,string>[] _uptimeValues =
    [
        new KeyValuePair<uint, string>(1, "1 Day"),
        new KeyValuePair<uint, string>(5, "5 Days"),
        new KeyValuePair<uint, string>(30, "30 Days")
    ];

    private readonly KeyValuePair<int, string>[] _alertFreqs =
    [
        new KeyValuePair<int, string>(1, "1 hour"),
        new KeyValuePair<int, string>(3, "3 hour"),
        new KeyValuePair<int, string>(6, "6 hour"),
        new KeyValuePair<int, string>(8, "8 hour"),
        new KeyValuePair<int, string>(-1, "Never")
    ];

    private readonly KeyValuePair<int, string>[] _spamFreqs =
    [
        new KeyValuePair<int, string>(5, "5 minutes"),
        new KeyValuePair<int, string>(10, "10 minutes"),
        new KeyValuePair<int, string>(20, "20 minutes")
    ];

    public Settings()
    {
        InitializeComponent();

        string settingsFile = File.ReadAllText(_path);
        var settings = JsonSerializer.Deserialize<App.AppSettings>(settingsFile);
        _defaultSettings = settings;
        
        UptimeValue.ItemsSource = _uptimeValues;
        AlertFreq.ItemsSource = _alertFreqs;
        SpamFreq.ItemsSource = _spamFreqs;
        
        SetSettings(settings);
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
        SetSettings(_defaultSettings);
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
    
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    private async void WriteSettings(string path, App.AppSettings appSettings)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, appSettings, _options);
    }

    private void OpenJson_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        Process.Start("explorer.exe", _path);
    }

    private void SetSettings(App.AppSettings settings)
    {
        UptimeValue.SelectedItem = _uptimeValues.Single(x => x.Key == settings.MinUpTime_Days);
        
        AlertFreq.SelectedItem = _alertFreqs.Single(x => x.Key == settings.AlertFrequency_Hours);
        
        SpamFreq.SelectedItem = _spamFreqs.Single(x => x.Key == settings.SpamFrequency_Minutes);
        
        SpamMode.IsChecked = settings.SpamEnabled;
        ShowOnStartUp.IsChecked = settings.ShowSettingsOnStartup;
        
        _minUptime = (uint)UptimeValue.SelectedValue;
        _alertFreq = (int)AlertFreq.SelectedValue;
        _showOnStartup = (bool)SpamMode.IsChecked;
        _spamEnabled = (bool)ShowOnStartUp.IsChecked;
        _spamFreq = (int)SpamFreq.SelectedValue;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        e.Cancel = true;
        Hide();
    }
}