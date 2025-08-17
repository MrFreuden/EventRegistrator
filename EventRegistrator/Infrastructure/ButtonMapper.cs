using EventRegistrator.Application.Objects;
using Telegram.Bot.Types.ReplyMarkups;

namespace EventRegistrator.Infrastructure
{
    public static class ButtonMapper
    {
        public static InlineKeyboardButton Map(Button button)
        {
            return new InlineKeyboardButton(button.Label, button.Callback);
        }

        public static List<InlineKeyboardButton> Map(List<Button> buttons)
        {
            var buttonsList = new List<InlineKeyboardButton>();
            foreach (var item in buttons)
            {
                buttonsList.Add(Map(item));
            }
            return buttonsList;
        }

        public static List<List<InlineKeyboardButton>> Map(List<List<Button>> buttons)
        {
            var buttonsList = new List<List<InlineKeyboardButton>>();
            foreach (var item in buttons)
            {
                buttonsList.Add(Map(item));
            }
            return buttonsList;
        }

        public static InlineKeyboardMarkup Map(ButtonData buttonData)
        {
            var markup = new InlineKeyboardMarkup();
            if (buttonData.SingleButton is not null)
            {
                markup.AddButton(Map(buttonData.SingleButton));
            }
            else if (buttonData.ButtonList is not null)
            {
                markup.AddButtons(Map(buttonData.ButtonList).ToArray());
            }
            else if (buttonData.ButtonMatrix is not null)
            {
                foreach (var row in buttonData.ButtonMatrix)
                {
                    markup.AddNewRow(Map(row).ToArray());
                }
            }
            return markup;
        }
    }
}
