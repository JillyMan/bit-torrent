using System;
using System.Collections.Generic;
using System.Text;

namespace BitTorrent
{
    public abstract class BEncodeBaseTest
    {
        protected IEnumerator<byte> GetIterator(byte[] bytes) 
        {
            var it = ((IEnumerable<byte>)bytes).GetEnumerator();
            it.MoveNext();
            return it;
        }

        protected byte[] GetBytes(string val) 
        {
            return Encoding.UTF8.GetBytes(val);
        }

        protected byte[] GetBytes(int val) 
        {
            return Encoding.UTF8.GetBytes(Convert.ToString(val));
        }

        protected string GetString(byte[] bytes) 
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}