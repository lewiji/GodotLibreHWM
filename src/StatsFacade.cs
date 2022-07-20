using System;
using System.Text.RegularExpressions;
using Godot;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.CPU;
using LibreHardwareMonitor.Hardware.Motherboard;

namespace SystemStats {
    public class StatsFacade : Node {
        // Sensor interface
        private Computer? _computer;
        private StatsNodeGdAdapter _statsNodeGdAdapter = null!;
        // Signal emitted when values update
        [Signal] public delegate void Changed();

        public override void _Ready() {
            _statsNodeGdAdapter = new StatsNodeGdAdapter();
            AddChild(_statsNodeGdAdapter);
        }
        
        // InitialiseStats() is invoked via `.call` in the "StatsNode" gdscript's _ready
        // so that the UI is set up first
        public void InitialiseStats() {
            if (_computer != null) return;
            _computer = new Computer {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsPsuEnabled = true,
                IsStorageEnabled = true
            };
            _computer.Open();
            
            RefreshStats();
        }
        
        public override void _ExitTree() {
            _computer?.Close();
        }

        private void RefreshStats() {
            if (_computer == null) return;
            _computer.Accept(new UpdateVisitor());

            foreach (IHardware hardware in _computer.Hardware)
            {
                switch (hardware.HardwareType) {
                    case HardwareType.Cpu:
                        HandleCpus(hardware); break;
                    case HardwareType.GpuAmd or HardwareType.GpuIntel or HardwareType.GpuNvidia:
                        HandleGpus(hardware); break;
                    case HardwareType.Memory:
                        HandleMemory(hardware); break;
                    case HardwareType.Motherboard:
                        HandleMotherboard(hardware); break;
                    default:
                        GD.PushWarning($"HardwareType not handled: {hardware.HardwareType}"); break;
                }
                HandleSubHardware(hardware);
            }
            EmitSignal(nameof(Changed));
        }

        private void HandleMotherboard(IHardware hardware) {
            if (hardware is not Motherboard motherboard) return;
            GD.Print(motherboard.Name);
        }

        private void HandleMemory(IHardware hardware) {
            foreach (ISensor sensor in hardware.Sensors) {
                GD.Print($"\tSensor: {sensor.Name}, value: {sensor.Value}");
                switch (sensor.Name) {
                    case "Memory Used":
                        _statsNodeGdAdapter.MemUsed = sensor.Value; break;
                    case "Memory Available":
                        _statsNodeGdAdapter.MemTotal = sensor.Value; break;
                    default:
                        GD.Print($"Unhandled sensor: {sensor.Name}"); break;
                }
            }
        }

        private void HandleSubHardware(IHardware hardware) {
            foreach (IHardware subhardware in hardware.SubHardware) {
                GD.Print($"\tSubhardware: {subhardware.Name}");

                foreach (ISensor sensor in subhardware.Sensors) {
                    GD.Print($"\t\tSensor: {sensor.Name}, value: {sensor.Value}");
                }
            }
        }

        private void HandleCpus(IHardware hardware) {
            if (hardware is not GenericCpu cpu) return;
            // the library's most useful fields/types are private or internal, so let's do some good ol' regex
            var report = cpu.GetReport();
            
            _statsNodeGdAdapter.CpuName = cpu.Name;
            _statsNodeGdAdapter.CpuFrequency = (float) cpu.TimeStampCounterFrequency;
            try {
                var cpuCoreCount = Regex.Match(report, @"Number of Cores: (\d)").Groups[1].Value.ToInt();
                var threadsPerCore = Regex.Match(report, @"Threads per Core: (\d)").Groups[1].Value.ToInt();
                _statsNodeGdAdapter.CpuCoreCount = cpuCoreCount;
                _statsNodeGdAdapter.CpuThreadCount = cpuCoreCount * threadsPerCore;
            } catch (Exception e) {
                GD.Print("Failed to fetch CPU cores/threads. Full report:");
                GD.Print(e.ToString());
                GD.Print(report);
            }
        }

        private void HandleGpus(IHardware hardware) {
            switch (hardware.HardwareType) {
                case HardwareType.GpuAmd:
                    GD.Print("GPU == GpuAmd"); break;
                case HardwareType.GpuIntel:
                    GD.Print("GPU == GpuIntel"); break;
                case HardwareType.GpuNvidia:
                    GD.Print("GPU == GpuNvidia"); break;
                default:
                    GD.PushWarning($"{hardware.Name} was not a known GPU type"); break;
            }
        }

        private class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }

            public void VisitSensor(ISensor sensor) { }

            public void VisitParameter(IParameter parameter) { }
        }
    }
}
