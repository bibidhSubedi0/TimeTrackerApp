using System;
using System.IO;
using System.Text.Json;
using TimeTrackerApp.Models;

namespace TimeTrackerApp.Utils
{
    public class DataService
    {
        private readonly string _basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TimeTrackerApp");

        public async Task<UserData> LoadUserDataAsync(string userId)
        {
            var filePath = GetUserFilePath(userId);

            if (!File.Exists(filePath))
                return new UserData { UserId = userId };

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<UserData>(json) ?? new UserData { UserId = userId };
        }

        public async Task SaveUserDataAsync(UserData userData)
        {
            var filePath = GetUserFilePath(userData.UserId);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            var json = JsonSerializer.Serialize(
                userData,
                new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(filePath, json);
        }

        private string GetUserFilePath(string userId) =>
            Path.Combine(_basePath, $"user_{userId}.json");
    }
}
