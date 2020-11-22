using DownloadManager;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DownloadeManager
{
    public class FileDownload : IFileDownloader
    {
        public event Action<string> OnDownloaded;
        public event Action<string, Exception> OnFailed;
        private int DegreeOfParallelism { set; get; } = 4;
        private bool IsSetDegreeOfParallelism { set; get; } = false;

        private ConcurrentQueue<FileModel> queueFilesToDownload;
        private HttpClient client;
        private int countOfDownloadTasks;

        public FileDownload()
        {
            queueFilesToDownload = new ConcurrentQueue<FileModel>();
            client = new HttpClient();
        }

        public void AddFileToDownloadingQueue(string fileId, string url, string pathToSave)
        {
            queueFilesToDownload.Enqueue(new FileModel
            { 
                FileId = fileId,
                Url = url,
                PathToSave = pathToSave
            });
            AddTask();
        }

        public void SetDegreeOfParallelism(int degreeOfParallelism)
        {
            if (IsSetDegreeOfParallelism || queueFilesToDownload.Any())
            {
                throw new Exception("The degree of parallelism is already set");
            }

            DegreeOfParallelism = degreeOfParallelism;
            IsSetDegreeOfParallelism = true;
        }

        private async Task AddTask()
        {
            while (!queueFilesToDownload.IsEmpty)
            {
                if (countOfDownloadTasks < DegreeOfParallelism)
                {
                    countOfDownloadTasks++;
                    queueFilesToDownload.TryDequeue(out FileModel file);
                    await DownloadFileAsync(file);
                }
            }
        }

        private async Task DownloadFileAsync(FileModel file)
        {
            await Task.Run(async () => {
                try
                {
                    using (var response = client.GetAsync(file.Url))
                    {
                        using (var stream = new FileStream(file.PathToSave, FileMode.Create))
                        {
                            await response.Result.Content.CopyToAsync(stream);
                        }
                    }
                    OnDownloaded?.Invoke($"File {file.FileId} downloaded");
                }
                catch (Exception e)
                {
                    OnFailed?.Invoke($"Downloading of file {file.FileId} canceled", e);
                }
            });
            countOfDownloadTasks--;
        }
    }
}
