﻿using DownloadeManager;
using System;
using System.IO;

namespace DownloadManagerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var pathToSave = AppDomain.CurrentDomain.BaseDirectory.ToString();
            var downloader = new FileDownload();

            downloader.OnDownloaded += Success;
            downloader.OnFailed += Error;

            int count = 0;
            using (var stream = new StreamReader("FilesToDownload.txt"))
            {
                while (!stream.EndOfStream)
                {
                    downloader.AddFileToDownloadingQueue(count.ToString(), stream.ReadLine(), @$"{pathToSave}{count}.jpg");
                    count++;
                }
            }
            Console.ReadKey();
        }

        public static void Success(string message)
        {
            Console.WriteLine(message);
        }
        public static void Error(string message, Exception e)
        {
            Console.WriteLine(message + e.Message);
        }
    }
}
