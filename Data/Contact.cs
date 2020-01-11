using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelloCouch.Data
{
    public class Contact
    {        
        public string Name { get; set; }
        public string Number { get; set; }
        public string Type => typeof(Contact).Name;
    }
}
