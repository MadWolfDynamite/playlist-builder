using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistBuilder.Client.Api
{
    public enum PlaylistOptions
    {
        Relative,
        Absolute
    }

    public class PlaylistFileProcessor
    {
        public static async Task<List<string>> LoadPlaylistDataAsync(string filePath)
        {
            var output = new List<string>();
            var fileData = await File.ReadAllLinesAsync(filePath);

            foreach (var line in fileData)
                output.Add(line);

            return output;
        }

        public static async Task SavePlaylistDataAsync(string filePath, string content)
        {
            using var stream = new StreamWriter(filePath);
            await stream.WriteAsync(content);
        }
    }
}
