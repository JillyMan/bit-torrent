using System;
using System.Collections.Generic;
using System.Text;

namespace BitTorrent
{       
    class Program
    {
        // static Dictionary<byte, TokenType> Tokens = new Dictionary<byte, TokenType>()
        // {
        //     { Encoding.UTF8.GetBytes(Defines.Int)[0], TokenType.Integer },
        //     { Encoding.UTF8.GetBytes(Defines.End)[0], TokenType.End },
        //     { Encoding.UTF8.GetBytes(Defines.DStart)[0], TokenType.DStart },
        // };
//        char dsa = ''

        static void Main(string[] args)
        {          
            Validate(args);

            // new BEncoding(
            //     new IntParser(Tokens), 
            //     new StringParser(Tokens)
            // ).Decode(args[0]);
            
            Console.WriteLine("Hello World!");
        }

        static void Validate(string[] args) 
        {
            if(args.Length != 1 && 
                !string.IsNullOrEmpty(args[0])) 
            {
                throw new ArgumentException("Please input `file path`");
            }
        }
    }
}
