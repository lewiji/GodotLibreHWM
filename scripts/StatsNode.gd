extends Control

# get C# stats provider from autoload script node
onready var stats_facade = get_node("/root/StatsFacade")

# Cpu stats and ui
export(String) var CpuName
export(int) var CpuCoreCount
export(int) var CpuThreadCount
export(float) var CpuFrequency

export(NodePath) var CpuStatNamePath
onready var CpuStatName: Label = get_node(CpuStatNamePath)
export(NodePath) var CpuStatCoresPath
onready var CpuStatCores: Label = get_node(CpuStatCoresPath)
export(NodePath) var CpuStatThreadsPath
onready var CpuStatThreads: Label = get_node(CpuStatThreadsPath)
export(NodePath) var CpuStatFreqPath
onready var CpuStatFreq: Label = get_node(CpuStatFreqPath)

# Memory stats and ui
export(float) var MemUsed
export(float) var MemAvail
export(float) var MemTotal

export(NodePath) var MemStatUsedPath
onready var MemStatUsed: Label = get_node(MemStatUsedPath)
export(NodePath) var MemStatAvailPath
onready var MemStatAvail: Label = get_node(MemStatAvailPath)
export(NodePath) var MemStatTotalPath
onready var MemStatTotal: Label = get_node(MemStatTotalPath)

func _ready():
	stats_facade.connect("Changed", self, "on_changed")
	stats_facade.call("InitialiseStats")
	set_refresh_timer()

# update labels
func on_changed():
	CpuStatName.text = CpuName
	CpuStatCores.text = CpuCoreCount as String
	CpuStatThreads.text = CpuThreadCount as String
	CpuStatFreq.text = CpuFrequency as String
	
	MemStatUsed.text = MemUsed as String
	MemStatAvail.text = MemAvail as String
	MemStatTotal.text = MemTotal as String

# start refresh interval via Timer node	
func set_refresh_timer():
	var timer: Timer = get_node("Timer")
	var err = timer.connect("timeout", self, "on_refresh")
	timer.start()

# call stats refresh
func on_refresh():
	stats_facade.call("RefreshStats")
