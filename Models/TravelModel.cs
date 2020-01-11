using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelloCouch.Models
{
    public class TravelModel

    {
        [JsonProperty("callsign")]
        public string Callsign { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("iata")]
        public string Iata { get; set; }

        [JsonProperty("icao")]
        public string Icao { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
