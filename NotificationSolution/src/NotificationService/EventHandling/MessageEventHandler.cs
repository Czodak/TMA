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

        public MessageEventHandler(IFluentEmail emailSender)
        {
            _emailSender = emailSender;
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

                Console.WriteLine($"To: {toAddress.Address}, From: {fromAddress.Address}");

                await _emailSender
                    .To(toAddress.Address)
                    .SetFrom(fromAddress.Address)
                    .Subject(emailMessage.Subject)
                    .Body(emailMessage.Body)
                    .SendAsync();
            }
            catch(SmtpException smtpEx)
            {
                Console.WriteLine($"SMTP error : {smtpEx.StatusCode}, message : {smtpEx.Message}");
                if(smtpEx.InnerException != null)
                {
                    Console.WriteLine($"Inner excpt : {smtpEx.InnerException.Message}");
                }
            }
            catch (FormatException e)
            {
                Console.WriteLine($"Invalid email format: {e.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send failed, message {ex.Message}, stacktrace {ex.StackTrace}");
            }
        }
    }
}
