using System.Text;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IncidentMailService
{
    public class MailManager
    {
        public const string EMAIL_SENDER_ID = "Email.Sender.Id";
        public const string EMAIL_SENDER_PASSWORD = "Email.Sender.Password";
        public const string EMAIL_SMTP_CLIENT = "Email.Sender.Smtp.Client";
        public const string EMAIL_SMTP_PORT = "Email.Sender.Smtp.Port";
        public const string NOTIFICATION_EMAIL_ID = "Topdesk.Notification.Email.Id";
        public const string NOTIFICATION_SUBJECT = "Topdesk.Notification.Email.Subject";

        private string _senderEmailAddress;
        private string _senderEmailPassword;
        private string _receiverEmailAddress;
        private string _smtpHost;
        private int _smtpPort;

        public MailManager()
        {   
            _senderEmailAddress = AppSettingsProvider.GetSettingValue(EMAIL_SENDER_ID);
            _senderEmailPassword = AppSettingsProvider.GetSettingValue(EMAIL_SENDER_PASSWORD);
            _smtpHost = AppSettingsProvider.GetSettingValue(EMAIL_SMTP_CLIENT);
            _smtpPort = int.Parse(AppSettingsProvider.GetSettingValue(EMAIL_SMTP_PORT));
            _receiverEmailAddress = AppSettingsProvider.GetSettingValue(NOTIFICATION_EMAIL_ID);
        }

        public void SendEmailForExceedingIncidents(List<Incident> exceededSlaIncidents, string subjectFormat)
        {
            Parallel.ForEach(exceededSlaIncidents, (incident) =>
            {
                SendEmailForExceedingIncident(incident, subjectFormat);
            });
        }

        private void SendEmailForExceedingIncident(Incident incident, string subjectFormat)
        {
            var notificationSubject = string.Format(subjectFormat, incident.Number);
            var messgaeBody = CreateMessageFromIncident(incident);

            var client = new SmtpClient
            {
                Port = _smtpPort,
                Host = _smtpHost,
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_senderEmailAddress, _senderEmailPassword)
            };

            var notificationMessage = new MailMessage(_senderEmailAddress, _receiverEmailAddress, notificationSubject, messgaeBody)
            {
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
            };

            client.Send(notificationMessage);
        }

        private string CreateMessageFromIncident(Incident incident)
        {
            return $"<p>" +
                $"Incident ID - {incident.Id} <br/>" +
                $"Incident Number - {incident.Number} <br/>" +
                $"Incident Status - {incident.Status} <br/>" +
                $"Caller - {incident.Caller.Name} (ID - {incident.Caller.Id} Timezone - {incident.Caller.Branch.Timezone}) <br/>" +
                $"Incident Creator - {incident.Creator.Name} (ID - {incident.Creator.Id}) <br/>" +
                $"Incident Created On - {incident.CreationDate.ToString()} <br/>" +
                $"Time Spent - {incident.TimeSpent} <br/>" +
                $"</p>";
        }
    }
}