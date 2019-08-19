using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace BitTorrent.Models
{
    public class Torrent 
    {
        public string Name { get; }
        public bool? IsPrivate { get; }
        public List<FileItem> Files { get; } = new List<FileItem>();

        public string FileDirectory 
        {
            get 
            {
                return Files.Count > 1 ? Name + Path.DirectorySeparatorChar : "";
            }
        }

        public string DownloadDirectory { get; }

        public string Comment { get; }
        public string CreatedBy { get; }
        public Encoding Encoding { get; }
        public DateTime CreationDate { get; } 
        public List<Tracker> Trackers = new List<Tracker>();

        public int BlockSize { get; }
        public int PieceSize { get; }

        public byte[,] PieceHashes {get;}

        public byte[] InfoHash { get; } = new byte[20];
        public string HexStringInfohash 
        {
            get  
            {
                return string.Join("", this.InfoHash.Select(x => x.ToString("x2")));
            }
        }

        public string UrlSafeStirngInfoHash 
        {
            get 
            {
                return Encoding.UTF8.GetString(WebUtility.UrlEncodeToBytes(this.InfoHash, 0, this.InfoHash.Length));
            }
        }

        public static string BytesToString(long size) 
        {
            throw new NotImplementedException();
        }
    } 
}