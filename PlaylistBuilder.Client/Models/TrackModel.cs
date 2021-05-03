using System;
using System.Collections.Generic;
using System.Text;

namespace PlaylistBuilder.Client.Models
{
    public class TrackModel
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public IList<string> Genres { get; set; }

        public double Length { get; set; }
        public int Bitrate { get; set; }
        public string Codec { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ResolvedPath { get; set; }

        public TrackModel()
        {
            Genres = new List<string>();
        }
    }
}
