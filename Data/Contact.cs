using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HelloCouch.Data
{
    public class Contact
    {
        [Key]
        [JsonIgnore]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Type => typeof(Contact).Name;
    }
}
