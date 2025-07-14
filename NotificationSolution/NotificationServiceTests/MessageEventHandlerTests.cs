using System.Net.Mail;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Microsoft.Extensions.Logging;
using NotificationService.EventHandling;
using NotificationService.Events;
using NotificationService.Events.Base;
using NotificationService.Events.Common;
using NSubstitute;
using Xunit;
using Assert = Xunit.Assert;

namespace NotificationService.Tests
{
    public class MessageEventHandlerTests
    {
        private readonly MessageEventHandler _sut;
        private readonly IFluentEmail _emailSender;
        private readonly ILogger<MessageEventHandler> _logger;

        public MessageEventHandlerTests()
        {
            _emailSender = Substitute.For<IFluentEmail>();
            _logger = Substitute.For<ILogger<MessageEventHandler>>();

            _emailSender.Data.Returns(new FluentEmail.Core.Models.EmailData
            {
                FromAddress = new Address { EmailAddress = "sender@example.com" }
            });

            _emailSender.To(Arg.Any<string>()).Returns(_emailSender);
            _emailSender.Subject(Arg.Any<string>()).Returns(_emailSender);
            _emailSender.Body(Arg.Any<string>()).Returns(_emailSender);
            _emailSender.SetFrom(Arg.Any<string>()).Returns(_emailSender);

            _emailSender.SendAsync().Returns(new SendResponse());

            _sut = new MessageEventHandler(_emailSender, _logger);
        }

        [Fact]
        public async Task HandleEvent_ShouldSendEmail_WhenTaskStatusUpdated()
        {
            var taskEvent = new TaskStatusUpdatedEvent("user@example.com", "Done");
            

            _emailSender.SendAsync().Returns(new SendResponse());

            await _sut.HandleEvent(taskEvent);

            _emailSender.Received(1).To(taskEvent.EmailDestination);
            _emailSender.Received(1).Subject(Arg.Is<string>(s => s.Contains("Task Updated")));
            _emailSender.Received(1).Body(Arg.Is<string>(b => b.Contains(taskEvent.NewStatus)));
            await _emailSender.Received(1).SendAsync();
        }

        [Fact]
        public async Task HandleEvent_ShouldSendEmail_WhenTaskAssignmentUpdated()
        {
            var taskEvent = new TaskAssignementUpdatedEvent("user@example.com", "Test Task");

            _emailSender.SendAsync().Returns(new SendResponse());

            await _sut.HandleEvent(taskEvent);

            _emailSender.Received(1).To(taskEvent.EmailDestination);
            _emailSender.Received(1).Subject(Arg.Is<string>(s => s.Contains("Task has been assigned to you")));
            _emailSender.Received(1).Body(Arg.Is<string>(b => b.Contains(taskEvent.TaskTitle)));
            await _emailSender.Received(1).SendAsync();
        }

        [Fact]
        public async Task HandleEvent_ShouldSendEmail_WhenTaskRemoved()
        {
            var taskEvent = new TaskRemovedEvent("user@example.com", "Obsolete Task");

            _emailSender.SendAsync().Returns(new SendResponse());

            await _sut.HandleEvent(taskEvent);

            _emailSender.Received(1).To(taskEvent.EmailDestination);
            _emailSender.Received(1).Subject(Arg.Is<string>(s => s.Contains("removed")));
            _emailSender.Received(1).Body(Arg.Is<string>(b => b.Contains(taskEvent.TaskTitle)));
            await _emailSender.Received(1).SendAsync();
        }

        [Fact]
        public async Task HandleEvent_ShouldThrow_WhenEventTypeUnknown()
        {
            var unknownEvent = Substitute.For<ITaskEvent>();
            unknownEvent.EventType.Returns((TaskEventType)999);

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.HandleEvent(unknownEvent));
        }

        [Fact]
        public async Task SendEmail_ShouldLogError_WhenInvalidEmail()
        {
            var invalidEmailEvent = new TaskRemovedEvent("invalid-email", "Bad Task");

            await Assert.ThrowsAsync<FormatException>(() => _sut.HandleEvent(invalidEmailEvent));
        }
    }
}
