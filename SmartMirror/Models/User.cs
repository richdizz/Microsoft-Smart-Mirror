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
        public User() {
            this.Preferences = new Dictionary<int, string>();
        }
        public User(AuthResult authResults)
        {
            this.AuthResults = authResults;

            var parts = authResults.id_token.Split('.');
            if (parts[1].Length % 4 > 0)
                parts[1] = parts[1].PadRight(parts[1].Length + 4 - parts[1].Length % 4, '=');
            byte[] data = Convert.FromBase64String(parts[1]);
            string decodedString = Encoding.UTF8.GetString(data);
            var id = JsonConvert.DeserializeObject<IdToken>(decodedString);
            this.Id = id.oid;
            this.TenantId = id.tid;
            this.Upn = id.upn;
            this.DisplayName = id.name;
            this.GivenName = id.given_name;
            this.FamilyName = id.family_name;
            this.Preferences = new Dictionary<int, string>();

            // initialize preferences to empty
            this.Preferences.Add(1, "SmartMirror.Controls.ProfilePicPart");
            for (var i = 2; i <= 15; i++)
                this.Preferences.Add(i, "SmartMirror.Controls.EmptyPart");

            // initialize Face Reco PersonId
            this.PersonId = Guid.Empty;
        }

        public Guid Id { get; set; }
        public string Upn { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string DisplayName { get; set; }
        public Guid TenantId { get; set; }
        public AuthResult AuthResults { get; set; }
        public byte[] Photo { get; set; }
        public Dictionary<int, string> Preferences { get; set; }
        // PersonId is the Guid assigned by the Face Reco group when adding a person
        public Guid PersonId { get; set; }
    }
}
