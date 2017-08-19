using SmartMirror.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartMirror.Data
{
    public class UserLookupService
    {
        private List<UserProfile> _profiles = new List<UserProfile>();

        private static UserLookupService _instance;
        public static UserLookupService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UserLookupService();
                return _instance;

            }
        }

        private UserLookupService()
        {
            // To add a new user profile, add a UserProfile object to the _profiles list.
            // This in-memory "database" is for demo purposes only. In the real world, you'd 
            // call your own users web service or a database directly to find a user.
            // You'll want to add columns to your users table to store the Cognitive Services 
            // face profile ID for face verification for each person in the table as well as 
            // their voice profile ID if you plan on incorporating voice verification.
            _profiles.Add(new UserProfile() { Id = 100, FaceProfileId = new Guid("9db14edb-d2a1-4753-b273-7822a7cfb2af"), VoiceProfileId = new Guid("ecb1bb57-1b8c-48c5-9437-4bdd1312d899"), FirstName = "Mohammed", LastName = "Adenwala", VoiceSecretPhrase = "Apple juice tastes funny after toothpaste" });
            _profiles.Add(new UserProfile() { Id = 101, FaceProfileId = new Guid("a1c7fd97-beb5-4267-8232-3578a89368c0"), VoiceProfileId = new Guid("f359de6e-448a-4692-b699-4eb5f3bf8ea0"), FirstName = "Andy", LastName = "Wigley", VoiceSecretPhrase = "My voice is stronger then passwords" });
        }

        /// <summary>
        /// Searches for a user with the specified Cognitive Services Face Verfication ID
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UserProfile> GetUserByFaceProfileID(CancellationToken ct, Guid id)
        {
            // Simulate delay that would be caused by network roundtrip to database or web service you'd have in the real world
            await Task.Delay(2000, ct);

            // Search the local in-memory list of profiles for a matching profile
            return _profiles.FirstOrDefault(f => f.FaceProfileId == id);
        }
    }
}
