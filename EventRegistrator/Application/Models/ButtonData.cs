namespace EventRegistrator.Application.Objects
{
    public class ButtonData
    {
        public Button? SingleButton { get; set; }
        public List<Button>? ButtonList { get; set; }
        public List<List<Button>>? ButtonMatrix { get; set; }

        public ButtonData(string titel, string callback) => SingleButton = new Button(titel, callback);
        public ButtonData(Button button) => SingleButton = button;
        public ButtonData(List<Button> buttons) => ButtonList = buttons;
        public ButtonData(List<List<Button>> matrix) => ButtonMatrix = matrix;
    }

    public record Button(string Label, string Callback);
}
