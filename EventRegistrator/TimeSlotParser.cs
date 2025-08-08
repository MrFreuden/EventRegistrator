using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EventRegistrator
{
    public static class TimeSlotParser
    {
        private static readonly Regex TemplateRegex = new Regex(@"(\d{1,2}[:\.]\d{2})\s*[-–]\s*(\d+)\s+вільних місць", RegexOptions.Compiled);

        private static readonly Regex RegistrationRegex = new Regex(@"^([^\d]+)\s+(\d{1,2}(?:[:\.]\d{2})?)$", RegexOptions.Compiled);

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

        public static Registration ParseRegistrationMessage(string message, long userId, DateTime eventDate)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = RegistrationRegex.Match(message);
            if (!match.Success)
                return null;

            string name = match.Groups[1].Value.Trim();
            string timeStr = match.Groups[2].Value;

            if (timeStr.Contains(':') || timeStr.Contains('.'))
            {
                timeStr = timeStr.Replace('.', ':');

                if (!timeStr.Contains(':'))
                    timeStr += ":00";

                if (TimeSpan.TryParse(timeStr, out TimeSpan time))
                {
                    DateTime registrationTime = eventDate.Date.Add(time);
                    return new Registration(userId, name, registrationTime);
                }
            }
            else if (int.TryParse(timeStr, out int slotNumber))
            {
                return new Registration(userId, name, DateTime.MinValue.AddHours(slotNumber));
            }

            return null;
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
