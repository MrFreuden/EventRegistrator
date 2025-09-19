using EventRegistrator.Domain.DTO;
using EventRegistrator.Infrastructure.Utils;

namespace TimeSlotParserTests
{
    public class TimeSlotParserTests
    {
        private Dictionary<int, TimeSpan> _slotMap;

        [SetUp]
        public void Setup()
        {
            _slotMap = new Dictionary<int, TimeSpan>
            {
                { 1, new TimeSpan(10, 0, 0) }, // 10:00
                { 2, new TimeSpan(11, 0, 0) }, // 11:00
                { 3, new TimeSpan(12, 0, 0) }, // 12:00
                { 4, new TimeSpan(13, 30, 0) }, // 13:30
                { 5, new TimeSpan(15, 0, 0) }  // 15:00
            };
        }

        private MessageDTO CreateMessage(string text, long userId, DateTime eventDate, int messageId = 1)
        {
            return new MessageDTO
            {
                Text = text,
                UserId = userId,
                Created = eventDate,
                Id = messageId
            };
        }

        [Test]
        public void ParseRegistrationMessage_MultipleSlotsSingleLine_ReturnsMultipleRegistrations()
        {
            // Arrange
            var message = CreateMessage("Karlenko 1 2 3", 123456789, new DateTime(2025, 8, 8));

            // Act
            var result = TimeSlotParser.ParseRegistrationMessage(message, _slotMap);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));

            Assert.That(result[0].UserId, Is.EqualTo(123456789));
            Assert.That(result[0].Name, Is.EqualTo("Karlenko"));
            Assert.That(result[0].RegistrationOnTime, Is.EqualTo(new TimeSpan(10, 0, 0)));

            Assert.That(result[1].RegistrationOnTime, Is.EqualTo(new TimeSpan(11, 0, 0))); 
            Assert.That(result[2].RegistrationOnTime, Is.EqualTo(new TimeSpan(12, 0, 0))); 
        }

        [Test]
        public void ParseRegistrationMessage_MultipleSlotsSingleLine_ReturnsMultipleRegistrations1()
        {
            // Arrange
            var message = CreateMessage("Karlenko 1, 2, 3", 123456789, new DateTime(2025, 8, 8));

            // Act
            var result = TimeSlotParser.ParseRegistrationMessage(message, _slotMap);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].RegistrationOnTime, Is.EqualTo(new TimeSpan(10, 0, 0)));
            Assert.That(result[1].RegistrationOnTime, Is.EqualTo(new TimeSpan(11, 0, 0)));
            Assert.That(result[2].RegistrationOnTime, Is.EqualTo(new TimeSpan(12, 0, 0)));
        }

        [Test]
        public void ParseRegistrationMessage_MultipleSlotsSingleLine_ReturnsMultipleRegistrations2()
        {
            // Arrange
            var message = CreateMessage("Karlenko L. 1, 2, 3 \n Karlenko N. 1", 123456789, new DateTime(2025, 8, 8));

            // Act
            var result = TimeSlotParser.ParseRegistrationMessage(message, _slotMap);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result[0].Name, Is.EqualTo("Karlenko L."));
            Assert.That(result[0].RegistrationOnTime, Is.EqualTo(new TimeSpan(10, 0, 0)));
            Assert.That(result[1].RegistrationOnTime, Is.EqualTo(new TimeSpan(11, 0, 0)));
            Assert.That(result[2].RegistrationOnTime, Is.EqualTo(new TimeSpan(12, 0, 0)));
            Assert.That(result[3].Name, Is.EqualTo("Karlenko N."));
            Assert.That(result[3].RegistrationOnTime, Is.EqualTo(new TimeSpan(10, 0, 0)));
        }

        [Test]
        public void ParseRegistrationMessage_MultipleSlotsSingleLine_ReturnsMultipleRegistrations3()
        {
            // Arrange
            var message = CreateMessage("Karlenko L. 10:00 11:00", 123456789, new DateTime(2025, 8, 8));

            // Act
            var result = TimeSlotParser.ParseRegistrationMessage(message, _slotMap);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].RegistrationOnTime, Is.EqualTo(new TimeSpan(10, 0, 0)));
            Assert.That(result[1].RegistrationOnTime, Is.EqualTo(new TimeSpan(11, 0, 0)));
        }

        [Test]
        public void ParseRegistrationMessage_MixedNamesAndTimes_ReturnsCorrectRegistrations()
        {
            // Arrange
            var messageText = "Karlenko L. 10:00 11:00\nKarlenko M. 10:00 11:00";
            var message = CreateMessage(messageText, 123456789, new DateTime(2025, 8, 8));

            // Act
            var result = TimeSlotParser.ParseRegistrationMessage(message, _slotMap);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(4));

            Assert.That(result[0].Name, Is.EqualTo("Karlenko L."));
            Assert.That(result[0].RegistrationOnTime, Is.EqualTo(new TimeSpan(10, 0, 0)));
            Assert.That(result[1].Name, Is.EqualTo("Karlenko L."));
            Assert.That(result[1].RegistrationOnTime, Is.EqualTo(new TimeSpan(11, 0, 0)));

            Assert.That(result[2].Name, Is.EqualTo("Karlenko M."));
            Assert.That(result[2].RegistrationOnTime, Is.EqualTo(new TimeSpan(10, 0, 0)));
            Assert.That(result[3].Name, Is.EqualTo("Karlenko M."));
            Assert.That(result[3].RegistrationOnTime, Is.EqualTo(new TimeSpan(11, 0, 0)));
        }

        [Test]
        public void ParseRegistrationMessage_MixedSlotNumbersAndTimes_ReturnsCorrectRegistrations()
        {
            // Arrange
            var messageText = "Karlenko 1 2\nTom 10:00";
            var message = CreateMessage(messageText, 123456789, new DateTime(2025, 8, 8));

            // Act
            var result = TimeSlotParser.ParseRegistrationMessage(message, _slotMap);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));

            Assert.That(result[0].Name, Is.EqualTo("Karlenko"));
            Assert.That(result[0].RegistrationOnTime, Is.EqualTo(new TimeSpan(10, 0, 0))); 
            Assert.That(result[1].Name, Is.EqualTo("Karlenko"));
            Assert.That(result[1].RegistrationOnTime, Is.EqualTo(new TimeSpan(11, 0, 0))); 

            Assert.That(result[2].Name, Is.EqualTo("Tom"));
            Assert.That(result[2].RegistrationOnTime, Is.EqualTo(new TimeSpan(10, 0, 0))); 
        }

        [Test]
        public void ParseRegistrationMessage_SingleSlotWithPlusSymbol_RegistersToSingleSlot()
        {
            // Arrange
            var singleSlotMap = new Dictionary<int, TimeSpan>
            {
                { 1, new TimeSpan(10, 0, 0) }
            };
            var message = CreateMessage("Karlenko +", 123456789, new DateTime(2025, 8, 8));

            // Act
            var result = TimeSlotParser.ParseRegistrationMessage(message, singleSlotMap);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Karlenko"));
            Assert.That(result[0].RegistrationOnTime, Is.EqualTo(new TimeSpan(10, 0, 0)));
        }

        [Test]
        public void ParseRegistrationMessage_SingleSlotWithPlusWithoutSpace_RegistersToSingleSlot()
        {
            // Arrange
            var singleSlotMap = new Dictionary<int, TimeSpan>
            {
                { 1, new TimeSpan(10, 0, 0) }
            };
            var message = CreateMessage("Karlenko+", 123456789, new DateTime(2025, 8, 8));

            // Act
            var result = TimeSlotParser.ParseRegistrationMessage(message, singleSlotMap);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Karlenko"));
            Assert.That(result[0].RegistrationOnTime, Is.EqualTo(new TimeSpan(10, 0, 0)));
        }
    }
}