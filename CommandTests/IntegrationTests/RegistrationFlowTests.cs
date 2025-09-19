using EventRegistrator.Domain.DTO;

namespace CommandTests.IntegrationTests
{
    [TestFixture]
    public class RegistrationFlowTests : TestBase
    {
        [Test]
        public async Task CompleteRegistrationFlow_Test()
        {
            await CreateEvent();
            await RegisterFirstUser();
            await RegisterSecondUser();
            await AttemptToRegisterInFullSlot();
            await EditRegistration();
            await DeleteRegistrationByName();
            await DeleteRegistrationByReply();

            // ��������� �������� ��������� - ��� ����� ������ ���� �����
            Assert.That(Event.Slots.Sum(s => s.CurrentRegistrationCount), Is.EqualTo(0), "� ����� ����� �������� �����������");
        }

        private async Task CreateEvent()
        {
            // 1. �������� �������
            var createEventMessage = new MessageDTO
            {
                ChatId = 123456,
                UserId = 101112,
                Id = CREATE_EVENT_MESSAGE_ID,
                Text = "�������� ������� \n#test",
                Created = DateTime.Now
            };

            var createEventResponse = await CreateEventCommand.Execute(createEventMessage, UserAdmin);
            Assert.That(createEventResponse, Is.Not.Empty, "������ ��� �������� �������");

            // �������� ��������� �������
            Event = UserAdmin.GetLastEvent();
            Assert.That(Event, Is.Not.Null, "������� �� ���� �������");
            Assert.That(Event.HashtagName, Is.EqualTo("test"), "�������� ������ �������");

            // ���������, ��� ����� ������� ���������
            VerifyEventSlots();
        }

        private void VerifyEventSlots()
        {
            Assert.That(Event.Slots.Count, Is.EqualTo(3), "�������� ���������� ��������� ������");
            Assert.That(Event.Slots.ElementAt(0).Time, Is.EqualTo(new TimeSpan(10, 0, 0)), "�������� ����� ������� �����");
            Assert.That(Event.Slots.ElementAt(0).MaxCapacity, Is.EqualTo(2), "�������� ����������� ������� �����");
        }

        private async Task RegisterFirstUser()
        {
            // 2. ����������� ������������
            var registerMessage = new MessageDTO
            {
                ChatId = 123456,
                UserId = 201112,
                Id = REGISTER_IVAN_MESSAGE_ID,
                Text = "���� 1 2",
                ReplyToMessageId = Event.PostId,
                IsReply = true
            };

            var registerResponse = await RegisterCommand.Execute(registerMessage, UserAdmin);
            Assert.That(registerResponse, Is.Not.Empty, "������ ��� �����������");

            // ��������� ������� ��������� �����
            var likeMessage = registerResponse.FirstOrDefault(r => r.Like);
            Assert.That(likeMessage, Is.Not.Null, "����������� ��������� ����� ��� �����������");
            Assert.That(likeMessage.MessageToEditId, Is.EqualTo(REGISTER_IVAN_MESSAGE_ID), "�������� MessageId ��� �����");

            // ���������, ��� ����������� ���������
            VerifyFirstUserRegistration();
        }

        private void VerifyFirstUserRegistration()
        {
            var slot1 = Event.Slots.ElementAt(0);
            var slot2 = Event.Slots.ElementAt(1);

            Assert.That(slot1.CurrentRegistrationCount, Is.EqualTo(1), "����������� �� ��������� � ������ ����");
            Assert.That(slot2.CurrentRegistrationCount, Is.EqualTo(1), "����������� �� ��������� �� ������ ����");
            Assert.That(slot1.Contains("����"), Is.EqualTo(true), "�������� ��� � �����������");
        }

        private async Task RegisterSecondUser()
        {
            // 3. ����������� ������� ������������
            var register2Message = new MessageDTO
            {
                ChatId = 123456,
                UserId = 301112,
                Id = REGISTER_PETR_MESSAGE_ID,
                Text = "���� 1, 3",
                ReplyToMessageId = Event.PostId,
                IsReply = true
            };

            var register2Response = await RegisterCommand.Execute(register2Message, UserAdmin);
            Assert.That(register2Response, Is.Not.Empty, "������ ��� ����������� ������� ������������");

            // ��������� ������� ��������� ����� ��� ������� ������������
            var likeMessage = register2Response.FirstOrDefault(r => r.Like);
            Assert.That(likeMessage, Is.Not.Null, "����������� ��������� ����� ��� �����������");
            Assert.That(likeMessage.MessageToEditId, Is.EqualTo(REGISTER_PETR_MESSAGE_ID), "�������� MessageId ��� �����");

            // ��������� ��������� ������ ����� ������ �����������
            VerifySecondUserRegistration();
        }

        private void VerifySecondUserRegistration()
        {
            var slot1 = Event.Slots.ElementAt(0);
            Assert.That(slot1.CurrentRegistrationCount, Is.EqualTo(2), "����������� ������� ������������ �� ��������� � ������ ����");
            Assert.That(slot1.Contains("����"), Is.True, "��� ����������� ����� � ������ �����");
        }

        private async Task AttemptToRegisterInFullSlot()
        {
            // 4. ������� ����������� � ����������� ���� (������ �����������)
            var register3Message = new MessageDTO
            {
                ChatId = 123456,
                UserId = 401112,
                Id = REGISTER_ALEXEY_MESSAGE_ID,
                Text = "������� 1",
                ReplyToMessageId = Event.PostId,
                IsReply = true
            };

            var register3Response = await RegisterCommand.Execute(register3Message, UserAdmin);

            // ������ ������� ������ ������, ��� ��� ���� ��������
            Assert.That(register3Response, Is.Empty, "����������� ������ �������, ���� ���� ��� ��������");

            var slot1 = Event.Slots.ElementAt(0);
            Assert.That(slot1.CurrentRegistrationCount, Is.EqualTo(2), "����������� ���������� ����������� ����� ������� ������������");
        }

        private async Task EditRegistration()
        {
            // 5. �������������� �����������
            var editMessage = new MessageDTO
            {
                ChatId = 123456,
                UserId = 201112,
                Id = REGISTER_IVAN_MESSAGE_ID, // ��� �� ID, ��� � ��� ������ �����������
                Text = "���� 2 3", // �������� �����
                ReplyToMessageId = Event.PostId,
                IsReply = true,
                IsEdit = true
            };

            var editResponse = await EditRegistrationCommand.Execute(editMessage, UserAdmin);
            Assert.That(editResponse, Is.Not.Empty, "������ ��� �������������� �����������");

            // ��������� ������� ��������� �������� ��� �������������� (������ ������ �����������)
            var unlikeMessage = editResponse.FirstOrDefault(r => r.UnLike);
            Assert.That(unlikeMessage, Is.Not.Null, "����������� ��������� ������ ����� ��� ��������������");

            // ��������� ������� ��������� ����� ��� ����� �����������
            var likeMessage = editResponse.FirstOrDefault(r => r.Like);
            Assert.That(likeMessage, Is.Not.Null, "����������� ��������� ����� ��� ��������������");
            Assert.That(likeMessage.MessageToEditId, Is.EqualTo(REGISTER_IVAN_MESSAGE_ID), "�������� MessageId ��� �����");

            // ���������, ��� ����� ����������
            VerifyEditedRegistration();
        }

        private void VerifyEditedRegistration()
        {
            var slot1 = Event.Slots.ElementAt(0);
            var slot2 = Event.Slots.ElementAt(1);

            Assert.That(slot1.Contains(201112), Is.EqualTo(false), "����������� �� ������� �� ������� �����");
            Assert.That(slot2.Contains(201112), Is.EqualTo(true), "����������� �� ��������� �� ������ ����");
        }

        private async Task DeleteRegistrationByName()
        {
            // 6. �������� ����������� �� �����
            var deleteByNameMessage = new MessageDTO
            {
                ChatId = 123456,
                ThreadId = Event.ThreadId,
                UserId = 101112,
                Id = DELETE_BY_NAME_MESSAGE_ID,
                Text = "����-" // ������� ����������� �����
            };

            var deleteByNameResponse = await DeleteReigstrationsByNameCommand.Execute(deleteByNameMessage, UserAdmin);
            Assert.That(deleteByNameResponse, Is.Not.Empty, "������ ��� �������� ����������� �� �����");

            // ��������� ������� ��������� ��������� ��� �������� �� �����
            var unlikeMessages = deleteByNameResponse.Where(r => r.UnLike).ToList();
            Assert.That(unlikeMessages, Is.Not.Empty, "����������� ��������� ������ ������ ��� �������� �� �����");

            // ��������� ������� ��������� ����� ��� ��������� ��������
            var likeMessage = deleteByNameResponse.FirstOrDefault(r => r.Like);
            Assert.That(likeMessage, Is.Not.Null, "����������� ��������� ����� ��� ��������� ��������");
            Assert.That(likeMessage.MessageToEditId, Is.EqualTo(DELETE_BY_NAME_MESSAGE_ID), "�������� MessageId ��� ����� ��� �������� �� �����");

            // ���������, ��� ����������� ����� �������
            Assert.That(Event.Slots.All(s => !s.Contains("����")), Is.True, "����������� ����� �� �������");
        }

        private async Task DeleteRegistrationByReply()
        {
            // 7. �������� ����������� �� ID ���������
            var deleteMessage = new MessageDTO
            {
                ChatId = 123456,
                ThreadId = Event.ThreadId,
                UserId = 201112,
                Id = DELETE_BY_REPLY_MESSAGE_ID,
                ReplyToMessageId = REGISTER_IVAN_MESSAGE_ID,
                ReplyToMessage = new MessageDTO { UserId = 201112, Id = REGISTER_IVAN_MESSAGE_ID },
                IsReply = true
            };

            var deleteResponse = await DeleteRegistrationsCommand.Execute(deleteMessage, UserAdmin);
            Assert.That(deleteResponse, Is.Not.Empty, "������ ��� �������� ����������� �� ID");

            // ��������� ������� ��������� �������� ��� ��������
            var unlikeMessage = deleteResponse.FirstOrDefault(r => r.UnLike);
            Assert.That(unlikeMessage, Is.Not.Null, "����������� ��������� ������ ����� ��� �������� �� ID");

            // ��������� ������� ��������� ����� ��� ��������� ��������
            var likeMessage = deleteResponse.FirstOrDefault(r => r.Like);
            Assert.That(likeMessage, Is.Not.Null, "����������� ��������� ����� ��� ��������� ��������");
            Assert.That(likeMessage.MessageToEditId, Is.EqualTo(REGISTER_IVAN_MESSAGE_ID), "�������� MessageId ��� ����� ��� �������� �� ID");

            // ���������, ��� ��� ����������� ����� �������
            Assert.That(Event.Slots.All(s => !s.Contains("����")), Is.True, "����������� ����� �� �������");
        }
    }
}