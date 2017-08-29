using Newtonsoft.Json;
using SmartMirror.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SmartMirror.Utils
{
    public class StorageHelper
    {
        public static async Task ResetUsers()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.CreateFileAsync("data.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(new List<User>()), Windows.Storage.Streams.UnicodeEncoding.Utf8);
        }
        private static async Task<List<User>> getUsersAsync()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file;
            try
            {
                file = await localFolder.GetFileAsync("data.json");
            }
            catch (Exception)
            {
                file = await localFolder.CreateFileAsync("data.json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(new List<User>()), Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }

            var json = await FileIO.ReadTextAsync(file);
            return JsonConvert.DeserializeObject<List<User>>(json);
        }

        public static async Task SaveUserAsync(User user)
        {
            // get all users
            var users = await getUsersAsync();

            // look for existing user
            User dbUser = null;
            for (var i = 0; i < users.Count; i++)
            {
                if (users[i].Id == user.Id)
                {
                    dbUser = users[i];
                    dbUser.AuthResults = user.AuthResults;
                }
            }

            // add the user if they were not found
            if (dbUser == null)
                users.Add(user);

            // update the data.json file in local storage
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            var file = await localFolder.CreateFileAsync("data.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(users), Windows.Storage.Streams.UnicodeEncoding.Utf8);
        }

        public static async Task<List<User>> GetUsersAsync()
        {
            return await getUsersAsync();
        }
    }
}
