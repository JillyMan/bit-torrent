using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BitTorrent.Test;

namespace BitTorrent
{   
    class Program
    {
        static void Main(string[] args)
        {
       //     Validate(args);
            TestRun();

            var time = new DateTime(2019, 08, 25, 16, 28, 0);
            System.Console.Write("Time: ");
            System.Console.WriteLine(new DateTimeOffset(time).ToUnixTimeSeconds());
            System.Console.WriteLine(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
   
//            var result = BEncoding.DecodeFile(args[0]);
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
