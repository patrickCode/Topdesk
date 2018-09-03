using System;
using System.Net;
using System.Text;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IncidentMailService
{
    public class IncidentRepository
    {
        public const string TOPDESK_ENDPOINT = "Topdesk.Endpoint";
        public const string TOPDESK_LOGIN_ENDPOINT = "Topdesk.Login.Endpoint";
        public const string OPERATOR_USERNAME = "Topdesk.Operator.Username";
        public const string OPERATOR_PASSWORD = "Topdesk.Operator.Password";
        public const string TOPDESK_INCIDENTS_ENDPOINT = "Topdesk.Incidents.Endpoint";
        public const string TOPDESK_INCIDENTS_PAGESIZE = "Topdesk.Incidents.PageSize";
        public const string INCIDENTS_SLA = "Incidents.SLA.Hours";

        public IncidentRepository() { }

        public async Task<List<Incident>> GetIncidents(DateTime createdDateTime)
        {
            var createdDateStr = createdDateTime.ToString("yyyy-MM-dd");
            var pageSize = int.Parse(AppSettingsProvider.GetSettingValue(TOPDESK_INCIDENTS_PAGESIZE));
            var offset = 0;
            var incidents = new List<Incident>();


            using (var httpClient = new HttpClient())
            {
                var authorizationHeader = await CreateTokenAuthorizationHeader();
                httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
                var isResultPartial = true;

                do
                {
                    var incidentsUrl = string.Format(AppSettingsProvider.GetSettingValue(TOPDESK_INCIDENTS_ENDPOINT), AppSettingsProvider.GetSettingValue(TOPDESK_ENDPOINT), createdDateStr, offset, pageSize);
                    var response = await httpClient.GetAsync(incidentsUrl);
                    isResultPartial = response.StatusCode == HttpStatusCode.PartialContent;
                    var result = await response.Content.ReadAsStringAsync();
                    var partialIncidents = JsonConvert.DeserializeObject<List<Incident>>(result);
                    incidents.AddRange(partialIncidents);


                    offset = offset + pageSize;
                } while (isResultPartial);
            }

            var incidentsSinceLastTimer = incidents.Where(incident => incident.CreationDate >= createdDateTime).ToList();
            return incidentsSinceLastTimer;
        }

        public List<Incident> GetIncidentsExceedingSla(List<Incident> incidents, decimal sla)
        {
            var exceedingIncidents = incidents.Where(incident => incident.TimeSpent > sla).ToList();
            return exceedingIncidents;
        }

        private async Task<string> CreateTokenAuthorizationHeader()
        {
            var token = await GetAuthorizationToken();
            return $"TOKEN id=\"{token}\"";
        }

        private async Task<string> GetAuthorizationToken()
        {
            var operatorName = AppSettingsProvider.GetSettingValue(OPERATOR_USERNAME);
            var operatorPassword = AppSettingsProvider.GetSettingValue(OPERATOR_PASSWORD);
            var loginUrl = string.Format(AppSettingsProvider.GetSettingValue(TOPDESK_LOGIN_ENDPOINT), AppSettingsProvider.GetSettingValue(TOPDESK_ENDPOINT));

            using (var httpClient = new HttpClient())
            {
                var authorizationHeader = CreateBasicAuthorizationHeader(operatorName, operatorPassword);
                httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

                var response = await httpClient.GetAsync(loginUrl);
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
        }

        private string CreateBasicAuthorizationHeader(string userName, string password)
        {
            var credentialStr = $"{userName}:{password}";
            var asciiCredentialBytes = Encoding.ASCII.GetBytes(credentialStr);
            var base64Credential = Convert.ToBase64String(asciiCredentialBytes);
            var headerValue = $"Basic {base64Credential}";
            return headerValue;
        }
    }
}