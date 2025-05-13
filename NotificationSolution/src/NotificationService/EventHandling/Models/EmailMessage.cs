namespace NotificationService.EventHandling.Models
{
    internal record EmailMessage(string To, string Subject, string Body);    
}
