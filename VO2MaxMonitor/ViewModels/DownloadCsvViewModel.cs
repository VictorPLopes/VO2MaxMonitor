using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using ReactiveUI;
using VO2MaxMonitor.Models;
using VO2MaxMonitor.Services;

namespace VO2MaxMonitor.ViewModels;

/// <summary>
///     ViewModel for downloading readings from an MQTT broker and saving them as a CSV file.
/// </summary>
public class DownloadCsvViewModel : ViewModelBase
{
    private readonly IFilesService _filesService;
    private readonly List<Reading> _readings = [];
    private          string        _broker   = string.Empty;
    private          string        _filePath = string.Empty;
    private          bool          _isDownloading;
    private          IMqttClient?  _mqttClient;
    private          string        _password = string.Empty;
    private          int           _port     = 1883;
    private          string        _topic    = string.Empty;
    private          string        _username = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DownloadCsvViewModel" /> class.
    /// </summary>
    public DownloadCsvViewModel(string defaultUsername = "")
    {
        Username = defaultUsername;

        var canDownload = this.WhenAnyValue(
                                            x => x.Broker,
                                            x => x.Port,
                                            x => x.Topic,
                                            x => x.Username,
                                            x => x.FilePath,
                                            (broker, port, topic, username, filePath) =>
                                                !string.IsNullOrWhiteSpace(broker) &&
                                                !string.IsNullOrWhiteSpace(topic) &&
                                                !string.IsNullOrWhiteSpace(username) &&
                                                !string.IsNullOrWhiteSpace(filePath) &&
                                                port > 0);

        SaveCsvCommand = ReactiveCommand.CreateFromTask(SaveCsvFileAsync);

        StartStopCommand = ReactiveCommand.CreateFromTask(StartStopDownload, canDownload);
    }

    /// <summary>
    ///     Gets or sets the MQTT broker address.
    /// </summary>
    public string Broker
    {
        get => _broker;
        set => this.RaiseAndSetIfChanged(ref _broker, value);
    }

    /// <summary>
    ///     Gets or sets the MQTT port.
    /// </summary>
    public int Port
    {
        get => _port;
        set => this.RaiseAndSetIfChanged(ref _port, value);
    }

    /// <summary>
    ///     Gets or sets the MQTT topic to subscribe to.
    /// </summary>
    public string Topic
    {
        get => _topic;
        set => this.RaiseAndSetIfChanged(ref _topic, value);
    }

    /// <summary>
    ///     Gets or sets the MQTT username.
    /// </summary>
    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    /// <summary>
    ///     Gets or sets the MQTT password.
    /// </summary>
    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    /// <summary>
    ///     Gets or sets the path to the CSV file where readings will be saved.
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set => this.RaiseAndSetIfChanged(ref _filePath, value);
    }

    /// <summary>
    ///     Gets the text for the button: “Start Download” when not downloading and “Stop Download” when downloading.
    /// </summary>
    public string StartStopButtonText => _isDownloading ? "Stop Download" : "Start Download";

    /// <summary>
    ///     Gets the command for saving readings to a CSV file.
    /// </summary>
    public ReactiveCommand<Unit, Unit> SaveCsvCommand { get; }

    /// <summary>
    ///     Gets the command for starting or stopping the download of readings from the MQTT broker.
    /// </summary>
    public ReactiveCommand<Unit, Unit> StartStopCommand { get; }

    private async Task SaveCsvFileAsync()
    {
        try
        {
            var filesService = App.Current?.Services?.GetRequiredService<IFilesService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var file = await filesService.SaveFileAsync();
            if (file is null) return;

            // Get the file path
            FilePath = file.Path.AbsolutePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error selecting file: {ex.Message}");
        }
    }

    private async Task StartStopDownload()
    {
        _isDownloading = !_isDownloading;
        this.RaisePropertyChanged(nameof(StartStopButtonText));

        // If the download is starting, we should initiate the MQTT connection and start receiving messages.
        if (_isDownloading)
        {
            // Create a MQTT client factory
            var factory = new MqttClientFactory();

            // Create a MQTT client instance
            _mqttClient = factory.CreateMqttClient();

            // Create MQTT client options
            var options = new MqttClientOptionsBuilder()
                          .WithTcpServer(Broker, Port)
                          .WithCredentials(Username, Password)
                          .WithClientId(Guid.NewGuid().ToString())
                          .WithCleanSession()
                          .Build();

            // Connect to the MQTT broker
            var connectResult = await _mqttClient.ConnectAsync(options);

            if (connectResult.ResultCode != MqttClientConnectResultCode.Success)
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
                _isDownloading = false;
                this.RaisePropertyChanged(nameof(StartStopButtonText));
                return;
            }

            // Subscribe to the specified topic
            await _mqttClient.SubscribeAsync(Topic);

            // Callback function when a message is received
            _mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                try
                {
                    var payloadSequence = e.ApplicationMessage.Payload;
                    if (payloadSequence.IsEmpty || payloadSequence.Length == 0)
                    {
                        Console.WriteLine("Received empty message payload.");
                        return Task.CompletedTask;
                    }

                    var payloadBytes = payloadSequence.ToArray();
                    // Deserialize the payload
                    var reading = JsonSerializer.Deserialize<Reading>(payloadBytes);
                    // Check if the values are valid
                    if (reading.O2 < 0 || reading.O2 > 100 ||
                        reading.VenturiAreaRegular <= 0.0001 || reading.VenturiAreaRegular > 0.001 ||
                        reading.VenturiAreaConstricted <= 0.0001 || reading.VenturiAreaConstricted > 0.001 ||
                        Math.Abs(reading.DifferentialPressure) >= 1000.0)
                    {
                        Console.WriteLine("Received invalid reading.");
                        return Task.CompletedTask;
                    }

                    // If valid, check if the readings were received in order
                    if (_readings.Count == 0 || _readings.Last().TimeStamp < reading.TimeStamp)
                    {
                        _readings.Add(reading);
                    }
                    else if (_readings.Last().TimeStamp != reading.TimeStamp)
                    {
                        // If not in order, insert the reading at the correct position.
                        var index = _readings.FindIndex(r => r.TimeStamp > reading.TimeStamp);
                        _readings.Insert(index, reading);
                        // If the last reading has the same timestamp, ignore it.
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                }

                return Task.CompletedTask;
            };
        }

        // If the download is stopping, we should disconnect from the MQTT broker and save the readings to a CSV file.
        else
        {
            if (_mqttClient is not null)
            {
                await _mqttClient.UnsubscribeAsync(Topic);
                await _mqttClient.DisconnectAsync();
                _mqttClient.Dispose();
                _mqttClient = null!;
            }

            // Save the readings to a CSV file
            await using (var writer = new StreamWriter(FilePath))
                await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    await csv.WriteRecordsAsync(_readings);

            _readings.Clear();
        }
    }
}