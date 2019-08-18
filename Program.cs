using System;
using System.Text;
using BitTorrent.Test;

namespace BitTorrent
{       
    class Program
    {
        static void Main(string[] args)
        {
            //TestRun();           
            Validate(args);
            var result = BEncoding.DecodeFile(args[0]);
        }

        static void TestRun() 
        {
            new BEncodeDecodeTest();
            new BEncodeEncodeTest();

            Console.WriteLine("Tests run SUCCESS!!");
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
