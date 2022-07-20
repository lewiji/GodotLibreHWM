using Godot;

namespace SystemStats; 

public class StatsNodeGdAdapter : Node {
   // GDScript node to update values on
   public static Node? StatsNode;
   // The stats properties we want to report back to Godot
   public float? MemUsed {
      get => StatsNode?.Get("MemUsed") as float?;
      set => StatsNode?.Set("MemUsed", value);
   }
   public float? MemTotal {
      get => StatsNode?.Get("MemTotal") as float?;
      set => StatsNode?.Set("MemTotal", value);
   }
   public string? CpuName {
      get => StatsNode?.Get("CpuName") as string;
      set => StatsNode?.Set("CpuName", value);
   }
   public float CpuFrequency {
      get => (float) StatsNode?.Get("CpuFrequency")!;
      set => StatsNode?.Set("CpuFrequency", value);
   }
   public int CpuCoreCount {
      get => (int)StatsNode?.Get("CpuCoreCount")!;
      set => StatsNode?.Set("CpuCoreCount", value);
   }
   public int CpuThreadCount {
      get => (int)StatsNode?.Get("CpuThreadCount")!;
      set => StatsNode?.Set("CpuThreadCount", value);
   }
   // Get the GDScript StatsNode on ready
   public override void _Ready() {
      StatsNode = GetNode("/root/Main");
   }
}