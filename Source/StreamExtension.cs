using System.IO;

namespace BitTorrent
{
    public static class StreamExtension 
    {
        public static void Append(this Stream buffer, byte value)
        {
            buffer.WriteByte(value);
        }

        public static void Append(this Stream buffer, byte[] bytes) 
        {
            buffer.Write(bytes, 0, bytes.Length);
        }
    } 
}