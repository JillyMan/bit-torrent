using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using SHA1 = System.Security.Cryptography.SHA1;

namespace BitTorrent.Models
{
    public class Torrent 
    {
        public string Name { get; }
        public bool? IsPrivate { get; }
        public List<FileItem> Files { get; } = new List<FileItem>();

        public string FileDirectory => Files.Count > 1 ? Name + Path.DirectorySeparatorChar : "";
        public string DownloadDirectory { get; }

        public string Comment { get; }
        public string CreatedBy { get; }
        public Encoding Encoding { get; }
        public DateTime CreationDate { get; } 
        public List<Tracker> Trackers = new List<Tracker>();

        public byte[] InfoHash { get; } = new byte[20];
        public string HexStringInfohash => string.Join("", this.InfoHash.Select(x => x.ToString("x2")));
        public string UrlSafeStirngInfoHash => Encoding.UTF8.GetString(WebUtility.UrlEncodeToBytes(this.InfoHash, 0, this.InfoHash.Length));

        public int BlockSize { get; }
        public int PieceSize { get; }
        public long TotalSize => Files.Sum(x => x.Size);

        public string FormatedPieceSize => BytesToString(PieceSize);
        public string FormatedTotalSize => BytesToString(TotalSize);

        public int PieceCount => PieceHashes.Length;

        public byte[][] PieceHashes { get; }
        public bool[] IsPieceVerified { get; }
        public bool[][] IsBlockAcquired { get; }
       
        public string VerifiedPiecesString => string.Join("", IsPieceVerified.Select(x => x ? 1 : 0));
        public int VerifiedPieceCount => IsPieceVerified.Count(x => x);
        public double VerifiedRatio => VerifiedPieceCount / (double)PieceCount;
        public bool IsCompleted => VerifiedPieceCount == PieceCount;
        public bool IsStarted => VerifiedPieceCount > 0;

        public long Uploaded { get; set; } = 0;
        public long Downloaded => PieceSize * VerifiedPieceCount;
        public long Left => TotalSize - Downloaded;


        public event EventHandler<List<IPEndPoint>> PeersListUpdate;
        private object[] fileWriteLocks;
        private static SHA1 sha1 = SHA1.Create();

        public Torrent(string name, 
            string downloadDir, 
            List<FileItem> files,
            List<string> trackers, 
            int pieceSize,
            byte[] pieceHashes = null, 
            int blockSize = 16384,
            bool? isPrivate = false)
        {
            Name = name;
            DownloadDirectory = downloadDir;
            Files = files;

            PieceSize = pieceSize;
            BlockSize = blockSize;
            IsPrivate = isPrivate;

            fileWriteLocks = new object[Files.Count];
            for(var i = 0; i < Files.Count; ++i)
            {
                fileWriteLocks[i] = new object();
            }

            if(trackers != null) 
            {
                foreach(var url in trackers)
                {
                    var tracker = new Tracker(url);
                    Trackers.Add(tracker);
                    tracker.PeerListUpdated += HandlePeerListUpdated;

                }
            }

            var piecesCount = Convert.ToInt32(Math.Ceiling(TotalSize / Convert.ToDouble(PieceSize)));
            PieceHashes = new byte[piecesCount][];
            IsPieceVerified = new bool[piecesCount];
            IsBlockAcquired = new bool[piecesCount][];
            
            for(var i = 0; i < PieceCount; ++i) 
            {
                IsBlockAcquired[i] = new bool[GetBlockCount(i)];               
            }

            if(pieceHashes != null) 
            {
                for(var i = 0; i < PieceCount; ++i) 
                {
                    PieceHashes[i] = GetHash(i);    
                }
            }
            else 
            {
                for (int i = 0; i < PieceCount; ++i) 
                {
                    PieceHashes[i] = new byte[20];
                    Buffer.BlockCopy(pieceHashes, i * 20, PieceHashes[i], 0, 20);
                }
            }

            object info = TorrentInfoToBEncondingObject(this);
            byte[] bytes = BEncoding.Encode(info);
            InfoHash = SHA1.Create().ComputeHash(bytes);



        }

        public byte[] GetHash(int piece) 
        {
            byte[] data = ReadPiece(piece);
            if (data == null) 
            {
                return null;
            }
            return sha1.ComputeHash(data);
        }

        public int GetPieceSize(int piece) 
        {
            if (piece == PieceCount - 1)
            {
                var remainder = Convert.ToInt32(TotalSize % PieceSize);
                if(remainder != 0) 
                {
                    return remainder;
                }
            }

            return PieceSize;
        }

        public int GetBlockSize(int piece, int block)
        {
            if (block == GetBlockCount(piece) - 1)
            {
                var remainder = Convert.ToInt32(GetPieceSize(piece) % BlockSize);
                if(remainder != 0) 
                {
                    return remainder;
                }
            }   

            return BlockSize; 
        }

        public int GetBlockCount(int piece)
        {
            return Convert.ToInt32(Math.Ceiling(GetPieceSize(piece) / (double)BlockSize));
        }

        public static string BytesToString(long size) 
        {
            throw new NotImplementedException();
        }
    } 
}