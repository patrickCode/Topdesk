using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace IncidentMailService
{
    public static class MailerTimer
    {   
        public const string INCIDENTS_SLA = "Incidents.SLA.Hours";
        public const string NOTIFICATION_SUBJECT = "Topdesk.Notification.Email.Subject";

        [FunctionName("func-incident-notification")]
        public static void Run([TimerTrigger("0 0 */2 * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"Incident Notification Job triggered at: {DateTime.Now}");

            try
            {
                log.Info("Mailer Service triggered");
                var incidentRepository = new IncidentRepository();
                var mailManager = new MailManager();

                var lastTrigger = myTimer.ScheduleStatus.Last;

                var incidents = incidentRepository.GetIncidents(lastTrigger).Result;
                log.Info($"Total Incidents found: {incidents.Count}");

                var sla = Decimal.Parse(AppSettingsProvider.GetSettingValue(INCIDENTS_SLA));
                var incidentsExceedingSla = incidentRepository.GetIncidentsExceedingSla(incidents, sla);
                log.Info($"Incidents exceeding SLA: {incidentsExceedingSla.Count}");


                if (incidentsExceedingSla != null && incidentsExceedingSla.Count > 0)
                {
                    log.Info("Attempting to send email");
                    mailManager.SendEmailForExceedingIncidents(incidentsExceedingSla, AppSettingsProvider.GetSettingValue(NOTIFICATION_SUBJECT));
                    log.Info("Emails sent");
                }
            }
            catch (Exception exception)
            {
                log.Error(exception.ToString());
                throw;
            }
        }
    }
}
