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
            //HACK
            var users = await getUsersAsync();
            users.Add(user);
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
