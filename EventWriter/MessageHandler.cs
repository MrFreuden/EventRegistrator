using System;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace EventWriter
{
    public class MessageHandler
    {
        UserRepository _userRepository;
        MessageSender _messageSender;
        RepositoryLoader _repositoryLoader;
        public MessageHandler(UserRepository userRepository, MessageSender messageSender)
        {
            _userRepository = userRepository;
            _messageSender = messageSender;
            _repositoryLoader = new RepositoryLoader(EnvLoader.GetDataPath());
        }

        public async Task ProcessMessage(Message message)
        {
            if (IsPrivateMessage(message))
            {
                if (IsCommand(message))
                {
                    await ProcessPrivateMessageCommand(message);
                }
                else if (IsUserAsked(message))
                {
                    await ProcessEditTemplateText(message);
                }
            }
            else if (IsMessageFromTargetChat(message))
            {
                if (IsFromChannel(message) && IsHasHashtag(message))
                {
                    await ProcessNewEvent(message);
                }
                else if (IsReplyToPostMessage(message))
                {
                    await ProssecOnEventRegistration(message);
                }
            }
        }

        private bool IsHasHashtag(Message message)
        {
            var parts = message.Text.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.None
            );
            return parts[parts.Length - 1].Contains(_userRepository.GetUserByTargetChat(message.Chat.Id).HashtagName);
        }

        private bool IsFromChannel(Message message)
        {
            return message.ForwardFromChat != null;
        }

        private bool IsReplyToPostMessage(Message message)
        {
            return message.ReplyToMessage != null;
        }

        private bool IsPrivateMessage(Message message)
        {
            return message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private;
        }

        private bool IsMessageFromTargetChat(Message message)
        {
            var user = _userRepository.GetUserByTargetChat(message.Chat.Id);
            return user.TargetChatId == message.Chat.Id;
        }

        private bool IsUserAsked(Message message)
        {
            var user = _userRepository.GetUser(message.Chat.Id);
            return user.IsAsked;
        }

        private bool IsCommand(Message message)
        {
            return message.Text.StartsWith('/');
        }

        private async Task ProcessEditTemplateText(Message message)
        {
            var user = _userRepository.GetUser(message.Chat.Id);
            user.IsAsked = false;
            user.TempleText = message.Text;
            await _messageSender.SendTextTemplate(message.Chat.Id, user.TempleText);
            await SaveRepositoryAsync();
        }

        private async Task ProssecOnEventRegistration(Message message)
        {
            var user = _userRepository.GetUserByTargetChat(message.Chat.Id);
            var lastEvent = user.Events.Last();
            var reg = TimeSlotParser.ParseRegistrationMessage(message.Text, message.From.Id, message.Date);
            var slot = TimeSlotParser.FindMatchingTimeSlot(lastEvent._slots, reg);
            var result = slot.AddRegistration(reg);
            if (result == true)
            {
                if (lastEvent.PrivateMessageId == default)
                {
                    var text = TimeSlotParser.UpdateTemplateText(user.TempleText, lastEvent._slots);
                    lastEvent.TemplateText = text;
                    var m = await _messageSender.SendEventData(user.PrivateChatId, lastEvent);
                    lastEvent.PrivateMessageId = m.MessageId;
                }
                else
                {
                    var text = TimeSlotParser.UpdateTemplateText(user.TempleText, lastEvent._slots);
                    lastEvent.TemplateText = text;
                    await _messageSender.EditEventData(user.PrivateChatId, lastEvent.PrivateMessageId, lastEvent);
                }
                await _messageSender.EditFirstComment(user.TargetChatId, lastEvent.MessageId, lastEvent.TemplateText);
                await _messageSender.LikeMessage(user.TargetChatId, message.Id);

                await SaveRepositoryAsync();
            }
            else
            {
                Console.WriteLine("Ошибка добавления во временной слот");
            }
        }

        private async Task ProcessNewEvent(Message message)
        {
            var user = _userRepository.GetUserByTargetChat(message.Chat.Id);
            var m = await _messageSender.SendFirstCommentAsAntwort(message.Chat.Id, message.Id, user.TempleText);

            var newEvent = new Event(new Guid(), "SWS", message.ForwardFromChat.Id, m.MessageId, user.HashtagName, user.TempleText);
            var slots = TimeSlotParser.ExtractTimeSlotsFromTemplate(user.TempleText, message.Date);
            newEvent.AddSlots(slots);
            user.Events.Add(newEvent);

            await SaveRepositoryAsync();
        }

        private async Task ProcessPrivateMessageCommand(Message message)
        {
            switch (message.Text)
            {
                case "/start":
                    _userRepository.AddUser(message.Chat.Id);
                    await _messageSender.SendGreetings(message.Chat.Id);
                    break;
                case "/settings":
                    var text = _userRepository.GetUser(message.Chat.Id).TempleText;
                    await _messageSender.SendTextTemplate(message.Chat.Id, text);
                    break;
                default:
                    break;
            }
        }

        private async Task SaveRepositoryAsync()
        {
            await _repositoryLoader.SaveDataAsync(_userRepository);
        }
    }
}