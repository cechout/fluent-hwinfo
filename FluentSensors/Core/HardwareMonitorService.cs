using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace FluentSensors.Core
{
    // record container for all the relevant data about one sensor
    public record SensorData(
        string Id, // e.g. "/intelcpu/0/load/1" 
        string Name, // e.g. "CPU Package"
        string HardwareName, // e.g. "Intel Core i9-12900H"
        string HardwareType, // e.g. "Cpu", "GpuNvidia", "Memory"
        string SensorType, // e.g. "Power", "Temperature", "Load"
        double Value // the actual value of the sensor
    );


    public class HardwareMonitorService
    {
        // === fields ===

        private readonly Computer _computer;

        // the dynamic list:
        // it contains all sensors we want to monitor
        // the manual way would be:
        // "private IHardware? _cpuHardware;" 
        // "private ISensor? _cpuPackagePowerSensor;" and so on
        private readonly List<ISensor> _activeSensors = new();

        private readonly object _sensorLock = new object();
        private CancellationTokenSource? _cts;
        private Task? _loopTask;
        private readonly HashSet<string> _excludedSensorIds = new();


        // === singleton instance ===

        // this class is a singleton, because we want to have only one instance of this service that runs in the
        // background and updates the sensor values
        private static readonly HardwareMonitorService _instance = new HardwareMonitorService();
        public static HardwareMonitorService Instance => _instance;


        // === constructor ===

        private HardwareMonitorService()
        {
            _computer = new Computer
            {
                // all hardware components are explicitly disabled here to prevent the UI thread from freezing 
                // the actual initialization is deferred and chunked into the asynchronous pipeline methods (Init...Async)
                // below
                IsCpuEnabled = false,
                IsGpuEnabled = false,
                IsMemoryEnabled = false,
                IsStorageEnabled = false,
                IsMotherboardEnabled = false,
                IsControllerEnabled = false,
                IsNetworkEnabled = false,

            };

            _computer.Open();
        }


        // === public api ===

        public int UpdateIntervalMs { get; set; } = 500;

        // asynchronous initialization pipeline:
        // lhm heavily blocks the calling thread when enabling all the hardware components
        // to prevent application freezes, these methods allow any consuming class or caller to trigger the 
        // hardware discovery step-by-step on isolated background threads (via Task.Run)
        public Task InitMotherboardAsync()
        {
            return Task.Run(() => { _computer.IsMotherboardEnabled = true; });
        }

        public Task InitCpuAsync()
        {
            return Task.Run(() => { _computer.IsCpuEnabled = true; });
        }

        public Task InitGpuAsync()
        {
            return Task.Run(() => { _computer.IsGpuEnabled = true; });
        }

        public Task InitMemoryAndStorageAsync()
        {
            return Task.Run(() =>
            {
                _computer.IsMemoryEnabled = true;
                _computer.IsStorageEnabled = true;
            });
        }

        public Task InitControllerAsync()
        {
            return Task.Run(() => { _computer.IsControllerEnabled = true; });
        }

        public Task InitNetworkAsync()
        {
            return Task.Run(() => { _computer.IsNetworkEnabled = true; });
        }

        // monitoring control:
        // starts the background polling loop to read sensor values
        // this method gets called from the outside (e.g. MainWindow); only after the asynchronous initialization pipeline has
        // fully completed of course
        public void StartMonitoring()
        {
            // prevent double execution
            if (_cts != null) return;

            InitAllSensors();
            _cts = new CancellationTokenSource();

            // task.run() creates a new thread in the background, and puts explicitly the method
            // LoopAsync on this new thread
            // we keep the reference so StopMonitoring can actually wait for the loop to finish, not just ask it to stop
            _loopTask = Task.Run(() => LoopAsync(_cts.Token));
        }

        public void StopMonitoring()
        {
            if (_cts == null) return;

            _cts.Cancel();

            // block until the loop has fully exited (including a possibly already in-flight update), so once this method
            // returns, callers can be 100% sure HardwareDataUpdated will never fire again
            _loopTask?.Wait(2000);

            _cts = null;
            _loopTask = null;
        }

        public void Cleanup()
        {
            StopMonitoring();
            _computer.Close();
        }

        // exclusion API:
        // the service stays blind about the meaning of "excluded" (hidden, disabled, whatever); it just skips these ids
        public void AddExcludedSensor(string sensorId)
        {
            lock (_sensorLock)
            {
                _excludedSensorIds.Add(sensorId);
            }
        }

        public void RemoveExcludedSensor(string sensorId)
        {
            lock (_sensorLock)
            {
                _excludedSensorIds.Remove(sensorId);
            }
        }

        // bulk sync for startup, replaces the current exclusion set in one shot
        public void SetExcludedSensors(IEnumerable<string> sensorIds)
        {
            lock (_sensorLock)
            {
                _excludedSensorIds.Clear();
                foreach (var id in sensorIds)
                {
                    _excludedSensorIds.Add(id);
                }
            }
        }


        // === events ===

        // the master event:
        // instead of having multiple events for each sensor, we can have one event that
        // sends a list of all the sensor data at once
        // the manual way would be:
        // "public event Action<double>? CpuPackagePowerUpdated;"
        // "public event Action<double>? CpuIaPowerUpdated;" and so on
        public event Action<List<SensorData>>? HardwareDataUpdated;


        // === private helpers ===

        // polling loop
        private async Task LoopAsync(CancellationToken token)
        {
            // TEMP
            System.Diagnostics.Debug.WriteLine("=== Active (Up) .NET NetworkInterfaces + IP check ===");
            foreach (var nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;

                int addressCount = nic.GetIPProperties().UnicastAddresses.Count;
                System.Diagnostics.Debug.WriteLine(
                    $"[.NET] Name='{nic.Name}' | UnicastAddresses={addressCount} | Type={nic.NetworkInterfaceType}");
            }


            while (!token.IsCancellationRequested)
            {
                // update hardware (lhm fetches new values from the sensor)
                foreach (var hardware in _computer.Hardware)
                {
                    hardware.Update();
                }

                // snapshot of currently "up" network adapters, keyed by NetworkInterface.Name
                // (matches LHM's Hardware.Name 1:1)
                // rebuilt every tick since Wi-Fi/Ethernet can connect or disconnect while the app is running
                // filter layers (QoS Packet Scheduler, WFP, Native/Virtual WiFi Filter Driver) and WAN Miniport stubs as "Up"
                // even though they carry no real traffic; requiring at least one assigned IP address filters those out, since
                // only the actual physical/virtual adapter above them gets an address
                var activeNetworkAdapters = new HashSet<string>(
                    NetworkInterface.GetAllNetworkInterfaces()
                        .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                            nic.GetIPProperties().UnicastAddresses.Count > 0)
                        .Select(nic => nic.Name));

                // this is the exact list for the big event HardwareDataUpdated, we create a new list
                // and every iteration fill it with the current values of all the sensors we want to monitor
                var payload = new List<SensorData>();

                lock (_sensorLock)
                {
                    foreach (var sensor in _activeSensors)
                    {
                        string id = sensor.Identifier.ToString();

                        // skip sensors that were excluded by the user (e.g. hidden in the UI); no payload entry means no
                        // UI update and no widget graph update for this tick
                        if (_excludedSensorIds.Contains(id)) continue;

                        // skip sensors belonging to network adapters that are not currently active; Windows creates a huge
                        // amount of virtual/filter pseudo-adapters alongside every real one (QoS, WFP, Wi-Fi Direct, etc.),
                        // and those never carry meaningful data
                        if (sensor.Hardware.HardwareType == HardwareType.Network &&
                            !activeNetworkAdapters.Contains(sensor.Hardware.Name))
                        {
                            continue;
                        }

                        // some sensors might not have a value at the moment (maybe a hdd is still sleeping or smth)
                        if (sensor.Value.HasValue)
                        {
                            double value = sensor.Value.Value;

                            // some sensors (e.g. "Network Utilization" on adapters with no known link speed) report NaN or
                            // Infinity instead of leaving Value unset
                            if (double.IsNaN(value) || double.IsInfinity(value))
                            {
                                continue;
                            }

                            if (sensor.SensorType == SensorType.Throughput)
                            {
                                value /= 1_048_576.0; // bytes/s -> MB/s
                            }

                            payload.Add(new SensorData(
                                Id: id,
                                Name: sensor.Name,
                                HardwareName: sensor.Hardware.Name,
                                HardwareType: sensor.Hardware.HardwareType.ToString(),
                                SensorType: sensor.SensorType.ToString(),
                                Value: value
                            ));
                        }
                    }
                }

                // extra guard: skip the broadcast entirely if a shutdown was requested while we were building the payload above
                if (token.IsCancellationRequested) break;

                // we fire the event with the new list of sensor data
                HardwareDataUpdated?.Invoke(payload);

                try
                {
                    await Task.Delay(UpdateIntervalMs, token);
                }
                catch (OperationCanceledException)
                {
                    // StopMonitoring cancelled the token while we were waiting; exit the loop cleanly here so the
                    // task completes normally instead of ending up in the Canceled state
                    break;
                }
            }
        }

        // sensor discovery:
        // goes through the discovered hardware tree and registers relevant sensors into the flat list
        // this process is protected by _sensorLock to ensure thread-safety, preventing collection modification crashes if the
        // background polling loop is preparing to run simultaneously
        private void InitAllSensors()
        {
            lock (_sensorLock)
            {
                _activeSensors.Clear();

                // we go through every sensor that lhm detects
                foreach (var hardware in _computer.Hardware)
                {
                    DiscoverSensors(hardware);
                }
            }
        }

        private void DiscoverSensors(IHardware hardware)
        {
            // pump all relevant sensors of this found hardware into our flat list
            foreach (var sensor in hardware.Sensors)
            {
                // we accepts only this explicit sensor types, all the other are not relevant for now
                if (sensor.SensorType == SensorType.Load ||
                    sensor.SensorType == SensorType.Power ||
                    sensor.SensorType == SensorType.Temperature ||
                    sensor.SensorType == SensorType.Clock ||
                    sensor.SensorType == SensorType.Data ||
                    sensor.SensorType == SensorType.SmallData ||
                    sensor.SensorType == SensorType.Fan ||
                    sensor.SensorType == SensorType.Voltage ||
                    sensor.SensorType == SensorType.Throughput) 
                {
                    _activeSensors.Add(sensor);
                }
            }

            // some hardware (like motherboards or big GPUs) have sub-hardware
            // we traverse them recursively here
            foreach (var subHardware in hardware.SubHardware)
            {
                DiscoverSensors(subHardware);
            }
        }
    }
}