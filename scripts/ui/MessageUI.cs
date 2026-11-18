using Godot;

public partial class MessageUI : Label
{
    private Timer _timer;

    public override void _Ready()
    {
        _timer = new Timer();
        _timer.WaitTime = 2.0; // сообщение висит 2 секунды
        _timer.OneShot = true;
        _timer.Timeout += HideMessage;
        AddChild(_timer);
    }

    public void ShowMessage(string message, Color? color = null)
    {
        Text = message;
        Modulate = new Color(1, 1, 1, 1); // непрозрачный
        if (color.HasValue)
            Modulate = color.Value;
        _timer.Start();
    }

    private void HideMessage()
    {
        Modulate = new Color(1, 1, 1, 0); // прозрачный
    }
}
