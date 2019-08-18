using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace BitTorrent
{
    public static class BEncoding
    {
        private static readonly byte DictStart = Encoding.UTF8.GetBytes("d")[0];
        private static readonly byte End = Encoding.UTF8.GetBytes("e")[0];
        private static readonly byte ListStart = Encoding.UTF8.GetBytes("l")[0];
        private static readonly byte IntStart = Encoding.UTF8.GetBytes("i")[0];
        private static readonly byte Divider = Encoding.UTF8.GetBytes(":")[0];

        public static object DecodeFile(string fileName)
        {
            if(!File.Exists(fileName))
            {
                throw new FileNotFoundException();   
            }

            var bytes = File.ReadAllBytes(fileName);
            var it = ((IEnumerable<byte>)bytes).GetEnumerator();
            it.MoveNext();

            return DecodeNextObject(it);
        }

        public static object DecodeNextObject(IEnumerator<byte> it)
        {
            if(it.Current == DictStart) 
                return DecodeDictionary(it);
            
            if(it.Current == IntStart) 
                return DecodeInt(it);

            if(it.Current == ListStart)
                return DecodeList(it);
                    
            return DecodeByteArray(it);
        }

    #region Decode Helperes

        private static long DecodeInt(IEnumerator<byte> it) 
        {
            var bytes = new List<byte>();

            while(it.MoveNext() && it.Current != End) 
            {
                bytes.Add(it.Current);
            }

            var str = Encoding.UTF8.GetString(bytes.ToArray());
            return long.Parse(str);
        }

        private static byte[] DecodeByteArray(IEnumerator<byte> it) 
        {
            var lengthBytes = new List<byte>();

            do 
            {
                lengthBytes.Add(it.Current);
            } while(it.MoveNext() && it.Current != Divider);

            var strLength = Encoding.UTF8.GetString(lengthBytes.ToArray());

            if(!Int32.TryParse(strLength, out int length)) 
            {
                throw new Exception("unable to parse lenght.!!");
            }

            var bytes = new byte[length];
            for(int i = 0; i < length; ++i) 
            {
                it.MoveNext();
                bytes[i] = it.Current;
            }
         
            return bytes;
        }

        private static IDictionary<string, object> DecodeDictionary(IEnumerator<byte> it) 
        {
            var result = new Dictionary<string, object>();
            var keys = new List<string>();

            while(it.MoveNext() && it.Current != End) 
            {
                var keyBytes = DecodeByteArray(it);
                var key = Encoding.UTF8.GetString(keyBytes);
                it.MoveNext();
                var value = DecodeNextObject(it); 
                
                keys.Add(key);
                result.Add(key, value);
            }
            
            //todo: invistigate later           
            /* var sortedKeys = keys.OrderBy(x => BitConverter.ToString(Encoding.UTF8.GetBytes(x)));
            if(!keys.SequenceEqual(sortedKeys))
            {
                 throw new Exception("error loading dictionary: keys not sorted");
            }*/

            return result;
        }

        private static IList<object> DecodeList(IEnumerator<byte> it) 
        {
            var result = new List<object>();

            while(it.MoveNext() && it.Current != End)
            {
                var obj = DecodeNextObject(it);
                result.Add(obj);
            }

            return result;
        }
    #endregion

        public static byte[] Encode(object obj)
        {
            var buffer = new MemoryStream();
            EncodeNextObject(buffer, obj);
            return buffer.ToArray();
        }

        public static void EncodeNextObject(Stream buffer, object obj)
        {
            if(obj is byte[]) 
            {
                EncodeByteArray(buffer, (byte[])obj);
            }
            else if (obj is string) 
            {
                EncodeString(buffer, (string)obj);
            }
            else if (obj is long) 
            {
                EncodeInt(buffer, (long)obj);
            }
            else if(obj is IList<object>) 
            {
                EncodeList(buffer, (IList<object>)obj);
            }
            else if(obj is IDictionary<string, object>)
            {
                EncodeDictionary(buffer, (IDictionary<string, object>)obj);
            }
            else 
            {
                throw new InvalidDataException($"uups can't Encode this object: {obj} type: {obj.GetType()}");
            }
        }

        public static void EncodeToFile(object obj, string path) 
        {
            File.WriteAllBytes(path, Encode(obj));
        }

    #region Encode Helpers

        private static void EncodeInt(Stream buffer, long val)
        {
            buffer.Append(IntStart);
            buffer.Append(Encoding.UTF8.GetBytes(Convert.ToString(val)));
            buffer.Append(End);
        }

        private static void EncodeByteArray(Stream buffer, byte[] bytes) 
        {
            buffer.Append(Encoding.UTF8.GetBytes(Convert.ToString(bytes.Length)));
            buffer.Append(Divider);
            buffer.Append(bytes);
        }

        private static void EncodeString(Stream buffer, string str) 
        {
            EncodeByteArray(buffer, Encoding.UTF8.GetBytes(str));
        }

        private static void EncodeList(Stream buffer, IList<object> list) 
        {
            buffer.Append(ListStart);
            foreach(var obj in list)
            {
                EncodeNextObject(buffer, obj);
            }
            buffer.Append(End);
        }

        private static void EncodeDictionary(Stream buffer, IDictionary<string, object> dict)
        {
            buffer.Append(DictStart);
            
            var sortedKeys = dict.Keys.OrderBy(x => BitConverter.ToString(Encoding.UTF8.GetBytes(x)));
            foreach(var key in sortedKeys)
            {
                EncodeString(buffer, key);
                EncodeNextObject(buffer, dict[key]);
            }

            buffer.Append(End);
        }

    #endregion

    }
}