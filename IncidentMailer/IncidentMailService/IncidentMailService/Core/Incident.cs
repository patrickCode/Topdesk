using System;
using Newtonsoft.Json;

namespace IncidentMailService
{
    public class Incident
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("number")]
        public string Number { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("creationDate")]
        public DateTime CreationDate { get; set; }
        [JsonProperty("timeSpent")]
        public decimal TimeSpent { get; set; }
        [JsonProperty("caller")]
        public Caller Caller { get; set; }
        [JsonProperty("creator")]
        public Creator Creator { get; set; }
    }

    public class Creator
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Caller
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("dynamicName")]
        public string Name { get; set; }
        [JsonProperty("branch")]
        public CallerBranch Branch { get; set; }
    }

    public class CallerBranch
    {
        [JsonProperty("timeZone")]
        public string Timezone { get; set; }
    }
}