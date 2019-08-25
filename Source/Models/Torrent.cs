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
        public static readonly string TORRENT_EXTENSION_FILE = "torrent";

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

        public event EventHandler<int> PieceVerified;

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
                for (var i = 0; i < PieceCount; ++i) 
                {
                    PieceHashes[i] = new byte[20];
                    Buffer.BlockCopy(pieceHashes, i * 20, PieceHashes[i], 0, 20);
                }
            }

            object info = TorrentInfoToBEncodingObject(this);
            byte[] bytes = BEncoding.Encode(info);
            InfoHash = SHA1.Create().ComputeHash(bytes);

            for(var i = 0; i < PieceCount; ++i)
            {
                CheckIntegrityOfPiece(i);
            }
        }

        public byte[] ReadPiece(int piece) 
        {
            return Read(piece * PieceSize, GetPieceSize(piece));
        }

        public byte[] ReadBlock(int piece, int block) 
        {
            var blockSize = GetBlockSize(piece, block);
            return ReadBlock(piece, blockSize, blockSize);
        }

        public byte[] ReadBlock(int piece, int offset, int length) 
        {
            return Read(piece * PieceSize + offset, length);
        }

        /* 
            todo: 
                # need check, can you placed buffer, in this block
                    - if not - then write buffer in other blocks.
                # this method not safe, 
                    because have't check { piece Length} when assign in IsBlockAcquired[][]
         */
        public void WriteBlock(int piece, int block, byte[] buffer) 
        {
            if (buffer.Length >= GetBlockSize(piece, block)) 
            {
                throw new NotImplementedException("Functionality when {buffer.Lenght} more then {block size} not implemented.");
            }

            var start = piece * PieceCount + block * GetBlockSize(piece, block);           
            Write(start, buffer);
            IsBlockAcquired[piece][block] = true;
            CheckIntegrityOfPiece(piece);
        }

        public byte[] Read(long start, int length) 
        {
            long end = start + length;
            byte[] buffer = new byte[length];

            foreach(var file in Files)
            {
                if (IsNotRightFileRange(start, end, file)) continue;

                var filePath = FormatFilePath(file.Path);

                if (!File.Exists(filePath)) return null;

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) 
                {
                    var posInfo = GetDimensionRelativeFile(start, end, file);
                    stream.Seek(posInfo.start, SeekOrigin.Begin);
                    stream.Read(buffer, posInfo.bufStart, posInfo.length);
                }
            }

            return buffer;
        }

        private void Write(long start, byte[] buffer) 
        {
            long end = start + buffer.Length;
            for(var i = 0; i < Files.Count; ++i) 
            {
                var file = Files[i];

                if (IsNotRightFileRange(start, end, file)) continue;

                var filePath = FormatFilePath(file.Path);
                var dir = Path.GetDirectoryName(filePath);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                
                lock(fileWriteLocks[i]) 
                {
                    using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write)) 
                    {
                        var posInfo = GetDimensionRelativeFile(start, end, file);
                        stream.Seek(posInfo.start, SeekOrigin.Begin);
                        stream.Write(buffer, posInfo.bufStart, posInfo.length);
                    }
                }
            }
        }

        private string FormatFilePath(string filePath)
        {
            return DownloadDirectory + Path.DirectorySeparatorChar + FileDirectory + filePath;
        }

        private (long start, int length, int bufStart) GetDimensionRelativeFile(
            long start,
            long end,
            FileItem file)
        {
            var fstart = Math.Max(0, start + file.Offset);
            var fend = Math.Min(end - file.Offset, file.Size);
            var flenght = Convert.ToInt32(fend - fstart);
            var bufStart = Math.Max(0, Convert.ToInt32(file.Offset - start));

            return (fstart, flenght, bufStart);
        }

        private bool IsNotRightFileRange(long start, long end, FileItem file) 
        {
            var endOfFile = file.Offset + file.Size;
            return (start < file.Offset && end < file.Offset) || (start > endOfFile /*|| end > endOfFile */);
        }

        public void CheckIntegrityOfPiece(int piece) 
        {
            if (piece >= PieceHashes.Length) 
            {
                throw new ArgumentOutOfRangeException("Invalid piece index");
            }

            var pieceHash = GetHash(piece);
            var isVerified = (pieceHash != null && pieceHash.SequenceEqual(PieceHashes[piece]));

            if (isVerified) 
            {
                IsPieceVerified[piece] = true;
                for(var j = 0; j < IsBlockAcquired[piece].Length; ++j) 
                {
                    IsBlockAcquired[piece][j] = true;
                }

                PieceVerified?.Invoke(this, piece);

                return;
            }

            IsPieceVerified[piece] = false;

            if(IsBlockAcquired[piece].Any(x => x)) 
            {
                for(var j = 0; j < IsBlockAcquired[piece].Length; ++j)
                {
                    IsBlockAcquired[piece][j] = false;
                }
            }
        }

        public byte[] GetHash(int piece) 
        {
            byte[] data = ReadPiece(piece);

            if (data == null) return null;

            return sha1.ComputeHash(data);
        }

        #region Size Halpers

        public int GetPieceSize(int piece) 
        {
            if (piece == PieceCount - 1)
            {
                var remainder = Convert.ToInt32(TotalSize % PieceSize);
                if(remainder != 0) return remainder;
            }

            return PieceSize;
        }

        public int GetBlockSize(int piece, int block)
        {
            if (block == GetBlockCount(piece) - 1)
            {
                var remainder = Convert.ToInt32(GetPieceSize(piece) % BlockSize);
                if(remainder != 0) return remainder;
            }

            return BlockSize;
        }

        public int GetBlockCount(int piece)
        {
            return Convert.ToInt32(Math.Ceiling(GetPieceSize(piece) / (double)BlockSize));
        }

        #endregion
        
        #region Handlers

        private void HandlePeerListUpdated(object sender, List<IPEndPoint> endPoints) 
        {
            PeersListUpdate?.Invoke(sender, endPoints);
        }

        #endregion

        #region Static

        public static Torrent LoadFromFile(string filePath, string downloadPath) 
        {
            var obj = BEncoding.DecodeFile(filePath);
            var name = Path.GetFileNameWithoutExtension(filePath);       
            return BEncodingObjectToTorrent(obj, name, downloadPath);
        }

        public static void SafeToFile(Torrent torrent) 
        {
            var obj = TorrentToBEncodingObject(torrent);
            BEncoding.EncodeToFile(obj, $"{torrent.Name}.{TORRENT_EXTENSION_FILE}");
        }

        public static long DateTimeToUnixTimeStamp(DateTime dateTime) 
        {
            return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        }

        public static object TorrentToBEncodingObject(Torrent torrent) 
        {
            var dict = new Dictionary<string, object>();
            
            byte[] encode_string(string str)
            {
                return Encoding.UTF8.GetBytes(str);
            }

            if(torrent.Trackers.Count == 1) 
            {
                dict[Defines.ANNOUNCE] = Encoding.UTF8.GetBytes(torrent.Trackers[0].Address);
            }
            else 
            {
                dict[Defines.ANNOUNCE] = torrent.Trackers.Select(x => Encoding.UTF8.GetBytes(x.Address)).ToList();
            }

            dict[Defines.COMMENT] = encode_string(torrent.Comment);
            dict[Defines.CREATED_BY] = encode_string(torrent.CreatedBy);
            dict[Defines.CREATION_DATA] = DateTimeToUnixTimeStamp(torrent.CreationDate);
            dict[Defines.ENCODING] = Encoding.UTF8.GetBytes(Encoding.UTF8.WebName.ToUpper());
            dict[Defines.INFO] = TorrentInfoToBEncodingObject(torrent);
           
            return dict;
        }
        
        public static Torrent BEncodingObjectToTorrent(object obj, string name, string downloadPath) 
        {          
            throw new NotImplementedException();
         
            var torrent = new Torrent(name, downloadPath, null, null, 1);
            return torrent;
        }

        public static object TorrentInfoToBEncodingObject(Torrent torrent) 
        {
            throw new NotImplementedException();
        }

        public static string BytesToString(long size)
        {
            throw new NotImplementedException();
        }

        #endregion
    } 
}