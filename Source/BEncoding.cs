using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BitTorrent
{
    public class IntParser : Parser<int>
    {
        public IntParser(IDictionary<byte, TokenType> tokens) : 
            base(tokens)
        {
        }

        protected override int FormatStringTo(string data)
        {
            return Int32.Parse(data);
        }
    }

    public class StringParser : Parser<string>
    {
        public StringParser(IDictionary<byte, TokenType> tokens) : 
            base(tokens)
        {
        }

        protected override string FormatStringTo(string data)
        {
            return data;
        }
    }

    public class BEncoding
    {
        private Parser<int> _intParser;
        private Parser<string> _stringParser;

        public BEncoding(Parser<int> intParser, Parser<string> stringParser) 
        {
            _intParser = intParser;
            _stringParser = stringParser;
        }

        public TorrentFile Decode(string fileName)
        {
            if(!File.Exists(fileName))
            {
                throw new FileNotFoundException();   
            }

            var bytes = File.ReadAllBytes(fileName);

            var enumerator = bytes.GetEnumerator();
          
            Parse(bytes.GetEnumerator());

            for(int i = 0; i < bytes.Length; ++i)
            {
                var op = Encoding.UTF8.GetChars(bytes, i, 1)[0];

                if(op == Defines.DStart) 
                {

                }
            }

            return new TorrentFile();
        }

        void Parse(IEnumerator enumerator) 
        {
            while(enumerator.MoveNext())
            {
                var currentByte = enumerator.Current;
            }
        }
    }
}