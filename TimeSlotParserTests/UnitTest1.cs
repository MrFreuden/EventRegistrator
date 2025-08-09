using EventRegistrator;

namespace TimeSlotParserTests
{
    public class Tests
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

        [Test]
        public void ParseRegistrationMessage_MultipleSlotsSingleLine_ReturnsMultipleRegistrations()
        {
            // Arrange
            string message = "Karlenko 1 2 3";
            long userId = 123456789;
            DateTime eventDate = new DateTime(2025, 8, 8);

            // Act
            var result = TimeSlotParser.ParseRegistrationMessage(message, userId, eventDate, _slotMap);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));

            Assert.That(result[0].UserId, Is.EqualTo(userId));
            Assert.That(result[0].Name, Is.EqualTo("Karlenko"));
            Assert.That(result[0].RegistrationTime, Is.EqualTo(new DateTime(2025, 8, 8, 10, 0, 0))); // 1-й слот

            Assert.That(result[1].UserId, Is.EqualTo(userId));
            Assert.That(result[1].Name, Is.EqualTo("Karlenko"));
            Assert.That(result[1].RegistrationTime, Is.EqualTo(new DateTime(2025, 8, 8, 11, 0, 0))); // 2-й слот

            Assert.That(result[2].UserId, Is.EqualTo(userId));
            Assert.That(result[2].Name, Is.EqualTo("Karlenko"));
            Assert.That(result[2].RegistrationTime, Is.EqualTo(new DateTime(2025, 8, 8, 12, 0, 0))); // 3-й слот
        }

        [Test]
        public void ParseRegistrationMessage_MultipleSlotsSingleLine_ReturnsMultipleRegistrations1()
        {
            // Arrange
            string message = "Karlenko 1, 2, 3";
            long userId = 123456789;
            DateTime eventDate = new DateTime(2025, 8, 8);

            // Act
            var result = TimeSlotParser.ParseRegistrationMessage(message, userId, eventDate, _slotMap);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));

            Assert.That(result[0].UserId, Is.EqualTo(userId));
            Assert.That(result[0].Name, Is.EqualTo("Karlenko"));
            Assert.That(result[0].RegistrationTime, Is.EqualTo(new DateTime(2025, 8, 8, 10, 0, 0))); // 1-й слот

            Assert.That(result[1].UserId, Is.EqualTo(userId));
            Assert.That(result[1].Name, Is.EqualTo("Karlenko"));
            Assert.That(result[1].RegistrationTime, Is.EqualTo(new DateTime(2025, 8, 8, 11, 0, 0))); // 2-й слот

            Assert.That(result[2].UserId, Is.EqualTo(userId));
            Assert.That(result[2].Name, Is.EqualTo("Karlenko"));
            Assert.That(result[2].RegistrationTime, Is.EqualTo(new DateTime(2025, 8, 8, 12, 0, 0))); // 3-й слот
        }
        [Test]
        public void ParseRegistrationMessage_MultipleSlotsSingleLine_ReturnsMultipleRegistrations2()
        {
            // Arrange
            string message = "Karlenko L. 1, 2, 3 \n Karlenko N. 1";
            long userId = 123456789;
            DateTime eventDate = new DateTime(2025, 8, 8);

            // Act
            var result = TimeSlotParser.ParseRegistrationMessage(message, userId, eventDate, _slotMap);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(4));

            Assert.That(result[0].UserId, Is.EqualTo(userId));
            Assert.That(result[0].Name, Is.EqualTo("Karlenko L."));
            Assert.That(result[0].RegistrationTime, Is.EqualTo(new DateTime(2025, 8, 8, 10, 0, 0))); // 1-й слот

            Assert.That(result[1].UserId, Is.EqualTo(userId));
            Assert.That(result[1].Name, Is.EqualTo("Karlenko L."));
            Assert.That(result[1].RegistrationTime, Is.EqualTo(new DateTime(2025, 8, 8, 11, 0, 0))); // 2-й слот

            Assert.That(result[2].UserId, Is.EqualTo(userId));
            Assert.That(result[2].Name, Is.EqualTo("Karlenko L."));
            Assert.That(result[2].RegistrationTime, Is.EqualTo(new DateTime(2025, 8, 8, 12, 0, 0))); // 3-й слот

            Assert.That(result[3].UserId, Is.EqualTo(userId));
            Assert.That(result[3].Name, Is.EqualTo("Karlenko N."));
            Assert.That(result[3].RegistrationTime, Is.EqualTo(new DateTime(2025, 8, 8, 10, 0, 0))); // 3-й слот
        }
        [Test]
        public void ParseRegistrationMessage_MultipleSlotsSingleLine_ReturnsMultipleRegistrations3()
        {
            // Arrange
            string message = "Karlenko L. 10:00 11:00";
            long userId = 123456789;
            DateTime eventDate = new DateTime(2025, 8, 8);

            // Act
            var result = TimeSlotParser.ParseRegistrationMessage(message, userId, eventDate, _slotMap);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0].UserId, Is.EqualTo(userId));
            Assert.That(result[0].Name, Is.EqualTo("Karlenko L."));
            Assert.That(result[0].RegistrationTime, Is.EqualTo(new DateTime(2025, 8, 8, 10, 0, 0))); // 1-й слот

            Assert.That(result[1].UserId, Is.EqualTo(userId));
            Assert.That(result[1].Name, Is.EqualTo("Karlenko L."));
            Assert.That(result[1].RegistrationTime, Is.EqualTo(new DateTime(2025, 8, 8, 11, 0, 0))); // 2-й слот

        }
        [Test]
        public void ParseRegistrationMessage_MultipleSlotsSingleLine_ReturnsMultipleRegistrations4()
        {
            // Arrange
            string message = "Karlenko L. 10.00 11.00";
            long userId = 123456789;
            DateTime eventDate = new DateTime(2025, 8, 8);

            // Act
            var result = TimeSlotParser.ParseRegistrationMessage(message, userId, eventDate, _slotMap);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0].UserId, Is.EqualTo(userId));
            Assert.That(result[0].Name, Is.EqualTo("Karlenko L."));
            Assert.That(result[0].RegistrationTime, Is.EqualTo(new DateTime(2025, 8, 8, 10, 0, 0))); // 1-й слот

            Assert.That(result[1].UserId, Is.EqualTo(userId));
            Assert.That(result[1].Name, Is.EqualTo("Karlenko L."));
            Assert.That(result[1].RegistrationTime, Is.EqualTo(new DateTime(2025, 8, 8, 11, 0, 0))); // 2-й слот
        }
    }
}