using System;
using System.IO;
using System.Collections.Generic;

namespace QuickTimeParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = ParseArguments(args);

            if (arguments.Count == 0 || arguments.Contains("--help"))
            {
                PrintHelp();
                return;
            }

            var filePath = arguments[0];

            if (!File.Exists(filePath))
            {
                PrintHelp();
                return;
            }

            bool deepScan = arguments.Contains("--deep-scan");
            bool showData = arguments.Contains("--show-data");

            var parser = new Parser(deepScan, showData);
            parser.ParseFile(filePath);
        }

        static List<string> ParseArguments(string[] args)
        {
            var arguments = new List<string>();

            foreach (var arg in args)
            {
                arguments.Add(arg);
            }

            return arguments;
        }

        static void PrintHelp()
        {
            Console.WriteLine("QuickTimeParser");
            Console.WriteLine("https://github.com/xmatekaj/QuickTimeParser");
            Console.WriteLine("");
            Console.WriteLine("Help:");
            Console.WriteLine("------");
            Console.WriteLine("Usage: QuickTimeParser FILE [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --help             Display this help message");
            Console.WriteLine("  --deep-scan        Scans file for hidden video/audio");
            Console.WriteLine("  --show-data        Show some additional data about atoms");
            Console.ReadKey();
        }
    }
}
