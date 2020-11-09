using DownloadManager;
using System;
using System.Collections.Generic;
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
        public int DegreeOfParallelism { private set; get; } = 4;
        public bool IsSetDegreeOfParallelism { private set; get; } = false;

        public Queue<FileModel> queueFilesToDownload;
        private List<Task> downloadTasks;
        HttpClient client;

        public FileDownload()
        {
            queueFilesToDownload = new Queue<FileModel>();
            downloadTasks = new List<Task>();
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

        public void AddTask()
        {
            if (downloadTasks.Count < DegreeOfParallelism)
            {
                var file = queueFilesToDownload.Dequeue();
                downloadTasks.Add(DownloadFileAsync(file));
            }
        }

        public async Task DownloadFileAsync(FileModel file)
        {
            await Task.Factory.StartNew(async () => {
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
        }
    }
}
