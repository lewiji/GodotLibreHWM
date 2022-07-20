extends Label

export(NodePath) var timerPath

onready var _timer: Timer = get_node(timerPath)
onready var _intervalSlider: HSlider = get_parent()
onready var _intervalInputBox: LineEdit = get_node("LineEdit")
onready var _inputToggleButton: Button = get_node("Button")

var _sliderValue: float

func _ready():
	_intervalInputBox.visible = false;
	_intervalSlider.connect("value_changed", self, "on_value_changed")
	_intervalSlider.connect("drag_ended", self, "confirm_value")
	_inputToggleButton.connect("pressed", self, "on_toggle_input")
	_inputToggleButton.visible = true
	on_value_changed(_intervalSlider.value)
	confirm_value(true)
	
func on_value_changed(var value: float):
	_sliderValue = min(_intervalSlider.max_value, max(_intervalSlider.min_value, value))
	text = ("%.1f" % _sliderValue) as String + "s"
	if _intervalInputBox.visible:
		_intervalInputBox.visible = false
		_inputToggleButton.visible = true
	
func confirm_value(var did_change: bool):
	if did_change:
		_timer.wait_time = _sliderValue
		_intervalSlider.value = _sliderValue

func on_toggle_input():
	_inputToggleButton.visible = false
	_intervalInputBox.visible = true
	_intervalInputBox.placeholder_text = ("%.1f" % _sliderValue) as String + "s"
	_intervalInputBox.text = ""
	_intervalInputBox.grab_focus()
	_intervalInputBox.caret_position = 0
	
func _input(var event: InputEvent):
	if (event is InputEventKey and _intervalInputBox.visible):
		if (event.scancode == KEY_ENTER):
			_intervalInputBox.visible = false
			_inputToggleButton.visible = true
			on_value_changed(_intervalInputBox.text as float)
			confirm_value(true)
