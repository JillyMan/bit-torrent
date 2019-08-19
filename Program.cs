using System;
using System.Linq;
using System.Net;
using System.Text;
using BitTorrent.Test;

namespace BitTorrent
{   
    class Program
    {
        static void Main(string[] args)
        {
#if TODO    
    "BUG" occures when i`am 'try' read my 'lorem.txt.torrent', 
        scipped comment section
        
        Validate(args);
        var result = BEncoding.DecodeFile(args[0]);
#endif
            var bytes = new byte[] {54,23,15,13}.Select(x => x.ToString("x2")).ToArray();
            Array.ForEach(bytes, x => System.Console.Write($"{x} "));
        }

        static void TestRun() 
        {
            new BEncodeDecodeTest();
            new BEncodeEncodeTest();

            Console.WriteLine("Tests run SUCCESS!!");
            System.Console.WriteLine();
        }

        static void Validate(string[] args) 
        {
            if(args.Length != 1 && 
                !string.IsNullOrEmpty(args[0])) 
            {
                Console.Error.Write("Please input `file path`");
                throw new ArgumentException();
            }
        }
    }
}
