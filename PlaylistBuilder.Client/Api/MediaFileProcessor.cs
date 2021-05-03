using PlaylistBuilder.Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlaylistBuilder.Client.Api
{
    public class MediaFileProcessor
    {
        private static readonly IList<string> compatibleFormats = new List<string> { ".mp3", ".ogg", ".wav" };

        public static TrackModel LoadTrackMetadata(string filePath)
        {
            FileInfo trackDetails = new FileInfo(filePath);

            if (!compatibleFormats.Contains(trackDetails.Extension))
                return null;

            if (!trackDetails.Exists)
            {
                string[] splitSeperators = { "\\", "/", "." };
                var trackName = filePath.Split(splitSeperators, StringSplitOptions.RemoveEmptyEntries);

                string cleanedTrackName = Tools.StringManipulator.StringParser.RemoveSpecialCharacters(trackName[^2]);
                var folderPath = "";

                for (int index = 0; index < trackName.Length - 2; index++)
                    folderPath += $"{trackName[index]}\\";

                folderPath = string.IsNullOrWhiteSpace(folderPath) ? Environment.CurrentDirectory : folderPath;
                var currentIndex = cleanedTrackName.Length;

                while (currentIndex > 3 && !trackDetails.Exists)
                {
                    var searchStr = cleanedTrackName.Substring(0, currentIndex--);
                    trackDetails = GetTrackDetails(folderPath, searchStr);
                }

                if (trackDetails == null)
                    return null;
            }

            var fileStream = trackDetails.OpenRead();
            var trackMetadata = TagLib.File.Create(new TagLib.StreamFileAbstraction(trackDetails.Name, fileStream, fileStream));

            var tags = trackMetadata.GetTag(TagLib.TagTypes.Id3v2);

            double trackLength = 0;
            string trackCodec = "";
            foreach (var codec in trackMetadata.Properties.Codecs)
            {
                TagLib.IAudioCodec acodec = codec as TagLib.IAudioCodec;
                trackLength = acodec != null ? acodec.Duration.TotalMilliseconds : 0;
                trackCodec = acodec.Description;
            }

            var trackData = new TrackModel()
            {
                Title = string.IsNullOrWhiteSpace(tags.Title) ? trackDetails.Name : tags.Title,
                Artist = tags.FirstPerformer,
                Album = tags.Album,

                Length = trackLength,
                Bitrate = trackMetadata.Properties.AudioBitrate,
                Codec = trackCodec,

                FileName = trackDetails.Name,
                FilePath = trackDetails.Directory.FullName,
                ResolvedPath = trackDetails.FullName
            };

            foreach (var genre in tags.Genres)
                trackData.Genres.Add(genre);

            return trackData;
        }

        private static FileInfo GetTrackDetails(string directory, string searchStr)
        {
            foreach (var file in Directory.GetFiles(directory, $"{searchStr}*"))
                return new FileInfo(file);

            return new FileInfo(Path.Combine(directory, searchStr));
        }
    }
}
