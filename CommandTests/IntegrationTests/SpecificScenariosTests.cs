using EventRegistrator.Application.Commands;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Interfaces;
using EventRegistrator.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CommandTests.IntegrationTests
{
    [TestFixture]
    public class SpecificScenariosTests : TestBase
    {
        [SetUp]
        public void Setup()
        {
            base.Setup();

            CreateTestEvent().Wait();
        }
        
        private async Task CreateTestEvent()
        {
            var createEventMessage = new MessageDTO
            {
                ChatId = 123456,
                UserId = 101112,
                Id = 1000,
                Text = "�������� ������� \n#test",
                Created = DateTime.Now
            };

            await CreateEventCommand.Execute(createEventMessage, UserAdmin);
            Event = UserAdmin.GetLastEvent();
        }

        [Test]
        public async Task EventEdit_PreventsDuplicateCreation()
        {
            // Arrange - �������� ��������� ���������� �������
            int initialEventsCount = UserAdmin.GetEvents(123456).Count;
            
            // ����������� ��������� � ��������� ��������
            var editEventMessage = new MessageDTO
            {
                ChatId = 123456,
                UserId = 101112,
                Id = 1000,
                Text = "����������� �������� ������� \n#test",
                Created = DateTime.Now,
                IsEdit = true
            };

            // Act
            var response = await CreateEventCommand.Execute(editEventMessage, UserAdmin);
            
            // Assert - ���������, ��� ����� ������� �� ���������
            int currentEventsCount = UserAdmin.GetEvents(123456).Count;
            Assert.That(currentEventsCount, Is.EqualTo(initialEventsCount), "��� �������������� ��������� �������� �������� �������");
        }

        [Test]
        public async Task InvalidRegistrationFormat_ReturnsEmptyResponse()
        {
            // Arrange - ������� ��������� � ������������ �������� �����������
            var invalidFormatMessage = new MessageDTO
            {
                ChatId = 123456,
                UserId = 201112,
                Id = 2001,
                Text = "���� ��� �������� ������", // ��� �������� ������� ������
                ReplyToMessageId = Event.PostId,
                IsReply = true
            };

            // Act
            var response = await RegisterCommand.Execute(invalidFormatMessage, UserAdmin);
            
            // Assert
            Assert.That(response, Is.Empty, "������������ ������ ����������� ������ ��������� � ������� ������");
            
            // ���������, ��� ����������� �� ����������
            foreach (var slot in Event.Slots)
            {
                Assert.That(slot.Contains("����"), Is.False, "����������� � ������������ �������� �� ������ �����������");
            }
        }

        [Test]
        public async Task PlainText_NotRegistered()
        {
            // Arrange - ������� ��������� ��� ������ �� �������
            var plainTextMessage = new MessageDTO
            {
                ChatId = 123456,
                UserId = 301112,
                Id = 3001,
                Text = "������� ����� ��� �����������",
                IsReply = false
            };

            // Act
            var response = await RegisterCommand.Execute(plainTextMessage, UserAdmin);
            
            // Assert
            Assert.That(response, Is.Empty, "������� ����� �� ������ �������������� ��� �����������");
            
            // ���������, ��� ����������� �� ����������
            foreach (var slot in Event.Slots)
            {
                Assert.That(slot.CurrentRegistrationCount, Is.EqualTo(0), "����� �������� ������ �� ������ ���� �����������");
            }
        }

        [Test]
        public async Task RegistrationWithQuestionMark_IsProcessedCorrectly()
        {
            // Arrange - ��������� � ������������ � ������ �������
            var questionMarkMessage = new MessageDTO
            {
                ChatId = 123456,
                UserId = 401112,
                Id = 4001,
                Text = "���� 1 2 ?", // ����������� �� ������ �������
                ReplyToMessageId = Event.PostId,
                IsReply = true
            };

            // Act
            var response = await RegisterCommand.Execute(questionMarkMessage, UserAdmin);
            
            // Assert
            Assert.That(response, Is.Empty, "����������� �� ������ ������� �� ������ ��������������");
            
            // ���������, ��� ����������� ����������
            var slot1 = Event.Slots.ElementAt(0); // ���� 10:00
            var slot2 = Event.Slots.ElementAt(1); // ���� 11:00
            
            Assert.That(slot1.Contains("����"), Is.False, "����������� �� ������ ������� �� ������ ����������� � ���� 1");
            Assert.That(slot2.Contains("����"), Is.False, "����������� �� ������ ������� �� ������ ����������� � ���� 2");
        }
    }
}