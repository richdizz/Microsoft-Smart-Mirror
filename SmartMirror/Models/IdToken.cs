using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMirror.Models
{
    public class IdToken
    {
        public string family_name { get; set; }
        public string given_name { get; set; }
        public string name { get; set; }
        public Guid oid { get; set; }
        public Guid tid { get; set; }
        public string upn { get; set; }
    }
}
