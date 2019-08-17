using System.Text;
using System.Collections.Generic;

namespace BitTorrent
{
    public abstract class Parser<T> 
    {
        private IDictionary<byte, TokenType> _tokens;

        protected Parser(IDictionary<byte, TokenType> tokens)
        {
            _tokens = tokens;
        }

        public T Parse(byte[] bytes, int start, ref int offset) 
        {
            return FormatStringTo(PrivateParse(bytes, start, ref offset));            
        }

        private string PrivateParse(byte[] bytes, int start, ref int offset)
        {
            var builder = new StringBuilder();

            for(int i = start; !MatchToken(bytes[i], TokenType.End); ++i)
            {
                var ch = Encoding.ASCII.GetString(bytes, i, 1);
                builder.Append(ch);
                ++offset;
            }

            return builder.ToString();
        }

        protected abstract T FormatStringTo(string data);

        public bool MatchToken(byte token, TokenType type)
        {
            return _tokens.TryGetValue(token, out TokenType outType) && outType == type; 
        }

        public bool IsToken(byte token) 
        {
            return _tokens.ContainsKey(token);
        }
    }

    

}