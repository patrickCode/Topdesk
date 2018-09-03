using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace IncidentMailService
{
    public static class MailerHttp
    {
        
        public const string INCIDENTS_SLA = "Incidents.SLA.Hours";
        public const string NOTIFICATION_SUBJECT = "Topdesk.Notification.Email.Subject";

        [FunctionName("MailerHttp")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                log.Info("Mailer Service triggered");
                var incidentRepository = new IncidentRepository();
                var mailManager = new MailManager();

                var incidents = await incidentRepository.GetIncidents(new DateTime(1971, 1, 1));
                log.Info($"Incidents found: {incidents.Count}");

                var sla = Decimal.Parse(AppSettingsProvider.GetSettingValue(INCIDENTS_SLA));
                var incidentsExceedingSla = incidentRepository.GetIncidentsExceedingSla(incidents, sla);
                log.Info($"Incidents exceeding SLA: {incidentsExceedingSla.Count}");


                if (incidentsExceedingSla != null && incidentsExceedingSla.Count > 0)
                {
                    log.Info("Attempting to send email");
                    mailManager.SendEmailForExceedingIncidents(incidentsExceedingSla, AppSettingsProvider.GetSettingValue(NOTIFICATION_SUBJECT));
                    log.Info("Emails sent");
                }

                return req.CreateResponse(HttpStatusCode.OK, "Function completed");
            }
            catch (Exception exception)
            {
                log.Error(exception.ToString());
                throw;
            }
        }
    }
}