using EventRegistrator.Domain.Models;
using Sprache;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EventRegistrator
{
    public static class TimeSlotParser
    {
        private static readonly Regex TokenSplit = new Regex(@"[\s,;]+");
        private static readonly Regex SlotOrTimePattern = new Regex(@"^\d{1,2}(:\d{2})?[\.\)]?$"); // 1  1.  1)  10:00 10:00.
        private static readonly Regex TemplateRegex = new Regex(@"(\d{1,2}[:\.]\d{2})\s*[-–]\s*(\d+)\s+вільних місць", RegexOptions.Compiled);

        public static List<TimeSlot> ExtractTimeSlotsFromTemplate(string templateText, DateTime eventDate)
        {
            var timeSlots = new List<TimeSlot>();

            if (string.IsNullOrWhiteSpace(templateText))
                return timeSlots;

            var lines = templateText.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.RemoveEmptyEntries
            );

            foreach (var line in lines)
            {
                var match = TemplateRegex.Match(line);
                if (match.Success)
                {
                    string timeStr = match.Groups[1].Value.Replace('.', ':');
                    string capacityStr = match.Groups[2].Value;

                    if (TimeSpan.TryParse(timeStr, out TimeSpan time) &&
                        int.TryParse(capacityStr, out int capacity))
                    {
                        DateTime slotTime = eventDate.Date.Add(time);
                        timeSlots.Add(new TimeSlot(slotTime, capacity));
                    }
                }
            }

            return timeSlots;
        }

        public static Dictionary<int, TimeSpan> GetMaper(string templateText)
        {
            var dic = new Dictionary<int, TimeSpan>();

            if (string.IsNullOrWhiteSpace(templateText))
                return dic;

            var lines = templateText.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.RemoveEmptyEntries
            );

            int i = 1;
            foreach (var line in lines)
            {
                var match = TemplateRegex.Match(line);
                if (match.Success)
                {
                    string timeStr = match.Groups[1].Value.Replace('.', ':');    

                    if (TimeSpan.TryParse(timeStr, out TimeSpan time))
                    {
                        dic.Add(i, time);
                        i++;
                    }
                }
            }

            return dic;
        }


        public static List<Registration> ParseRegistrationMessage(string input, long userId, DateTime eventDate, Dictionary<int, TimeSpan> slotMap, int messageId)
        {
            var result = new List<Registration>();
            if (string.IsNullOrWhiteSpace(input)) return result;

            var tokens = TokenSplit.Split(input.Trim());
            int i = 0;

            while (i < tokens.Length)
            {
                if (string.IsNullOrWhiteSpace(tokens[i])) { i++; continue; }

                var nameParts = new List<string>();
                while (i < tokens.Length && !IsSlotToken(tokens[i]))
                {
                    nameParts.Add(tokens[i]);
                    i++;
                }

                if (nameParts.Count == 0)
                {
                    i++;
                    continue;
                }

                var name = string.Join(" ", nameParts);

                bool anySlot = false;
                while (i < tokens.Length && IsSlotToken(tokens[i]))
                {
                    var slotToken = tokens[i++];
                    if (TryResolveSlotToken(slotToken, eventDate, slotMap, out DateTime registrationTime))
                    {
                        result.Add(new Registration(userId, name, registrationTime, messageId));
                        anySlot = true;
                    }
                }

                if (!anySlot)
                {
                }
            }

            return result;
        }

        private static bool IsSlotToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            return SlotOrTimePattern.IsMatch(token.Trim());
        }

        private static bool TryResolveSlotToken(string token, DateTime eventDate, Dictionary<int, TimeSpan> slotMap, out DateTime time)
        {
            time = default;
            if (string.IsNullOrWhiteSpace(token)) return false;

            var t = token.Trim().TrimEnd('.', ')');

            if (DateTime.TryParseExact(t, new[] { "H:mm", "HH:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
            {
                time = eventDate.Date.Add(dt.TimeOfDay);
                return true;
            }

            if (int.TryParse(t, out int slot) && slotMap != null && slotMap.TryGetValue(slot, out TimeSpan slotTime))
            {
                time = eventDate.Date.Add(slotTime);
                return true;
            }

            return false;
        }

        public static TimeSlot FindMatchingTimeSlot(List<TimeSlot> timeSlots, Registration registration)
        {
            if (registration.RegistrationTime.Date == DateTime.MinValue.Date)
            {
                int slotIndex = (int)registration.RegistrationTime.Hour - 1;
                if (slotIndex >= 0 && slotIndex < timeSlots.Count)
                    return timeSlots[slotIndex];
                return null;
            }

            return timeSlots.FirstOrDefault(slot =>
                slot.Time.Hour == registration.RegistrationTime.Hour &&
                slot.Time.Minute == registration.RegistrationTime.Minute);
        }

        public static string UpdateTemplateText(string templateText, List<TimeSlot> timeSlots)
        {
            if (string.IsNullOrWhiteSpace(templateText))
                return templateText;

            var lines = templateText.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.None
            );

            var result = new List<string>();

            foreach (var line in lines)
            {
                string updatedLine = line;
                var match = TemplateRegex.Match(line);

                if (match.Success)
                {
                    string timeStr = match.Groups[1].Value.Replace('.', ':');

                    if (TimeSpan.TryParse(timeStr, out TimeSpan time))
                    {
                        var matchingSlot = timeSlots.FirstOrDefault(slot =>
                            slot.Time.Hour == time.Hours &&
                            slot.Time.Minute == time.Minutes);

                        if (matchingSlot != null)
                        {
                            int availableSpots = matchingSlot.MaxCapacity - matchingSlot.CurrentRegistrationCount;
                            updatedLine = TemplateRegex.Replace(line, m =>
                                $"{timeStr} - {availableSpots} вільних місць");
                        }
                    }
                }

                result.Add(updatedLine);
            }

            return string.Join(Environment.NewLine, result);
        }
    }
}
