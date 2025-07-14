using System.Net.Mail;
using FluentEmail.Core;
using NotificationService.EventHandling.Models;
using NotificationService.Events;
using NotificationService.Events.Base;
using NotificationService.Events.Common;

namespace NotificationService.EventHandling
{
    public class MessageEventHandler : IMessageEventHandler
    {
        private readonly IFluentEmail _emailSender;
        private ILogger<MessageEventHandler> _logger;

        public MessageEventHandler(IFluentEmail emailSender, ILogger<MessageEventHandler> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task HandleEvent(ITaskEvent taskEvent)
        {             
            switch(taskEvent.EventType)
            {
                case TaskEventType.TaskStatusUpdated:
                    await HandleUpdate(taskEvent as TaskStatusUpdatedEvent);
                    break;
                case TaskEventType.TaskAssignementUpdated:
                    await HadleAssignementUpdate(taskEvent as TaskAssignementUpdatedEvent);
                    break;
                case TaskEventType.TaskRemoved:
                    await HandleTaskRemoved(taskEvent as TaskRemovedEvent);
                    break;
                default: 
                    throw new ArgumentException("Unkown event type");
            }
        }

        private async Task HandleUpdate(TaskStatusUpdatedEvent taskEvent)
        {
            var emailMessage = new EmailMessage(taskEvent.EmailDestination, "Task Updated", $"New task status {taskEvent.NewStatus}");
            await SendEmail(emailMessage);
        }
        private async Task HadleAssignementUpdate(TaskAssignementUpdatedEvent taskEvent)
        {
            var emailMessage = new EmailMessage(taskEvent.EmailDestination, "Task has been assigned to you", $"task {taskEvent.TaskTitle} is yours now");
            await SendEmail(emailMessage);
        }

        private async Task HandleTaskRemoved(TaskRemovedEvent taskEvent)
        {
            var emailMessage = new EmailMessage(taskEvent.EmailDestination, "Task has been removed", $"Task : {taskEvent.TaskTitle}");
            await SendEmail(emailMessage);
        }

        private async Task SendEmail(EmailMessage emailMessage)
        {
            try
            {
                var toAddress = new MailAddress(emailMessage.To);
                var fromAddress = new MailAddress(_emailSender.Data.FromAddress.EmailAddress);

                await _emailSender
                    .To(toAddress.Address)
                    .SetFrom(fromAddress.Address)
                    .Subject(emailMessage.Subject)
                    .Body(emailMessage.Body)
                    .SendAsync();
            }
            catch(SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, $"SMTP error");
                throw;
            }
            catch (FormatException e)
            {
                _logger.LogError(e, $"Invalid email format");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Send failed");
                throw;
            }
        }
    }
}
