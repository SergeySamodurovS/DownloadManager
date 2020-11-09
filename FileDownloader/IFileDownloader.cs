using System;
using System.Collections.Generic;
using System.Text;

namespace DownloadManager
{
    interface IFileDownloader
    {
        void SetDegreeOfParallelism(int degreeOfParallelism);
        void AddFileToDownloadingQueue(string fileId, string url, string pathToSave);

        event Action<string> OnDownloaded;
        event Action<string, Exception> OnFailed;
    }
}
