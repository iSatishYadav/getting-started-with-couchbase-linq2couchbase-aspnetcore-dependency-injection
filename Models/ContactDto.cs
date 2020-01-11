using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HelloCouch.Models
{
    public class ContactDto
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }        
    }
}
