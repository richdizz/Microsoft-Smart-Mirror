using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMirror.Models
{
    public class User
    {
        public User() { }
        public User(AuthResult authResults)
        {
            this.AuthResults = authResults;

            var parts = authResults.id_token.Split('.');
            byte[] data = Convert.FromBase64String(parts[1]);
            string decodedString = Encoding.UTF8.GetString(data);
            var id = JsonConvert.DeserializeObject<IdToken>(decodedString);
            this.Id = id.oid;
            this.TenantId = id.tid;
            this.Upn = id.upn;
            this.DisplayName = id.name;
        }

        public Guid Id { get; set; }
        public string Upn { get; set; }
        public string DisplayName { get; set; }
        public Guid TenantId { get; set; }
        public AuthResult AuthResults { get; set; }
        public byte[] Photo { get; set; }
    }
}
