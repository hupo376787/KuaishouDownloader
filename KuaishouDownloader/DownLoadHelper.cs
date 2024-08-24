using Downloader;
using System.Diagnostics;
using System.IO;

namespace KuaishouDownloader
{
    public class DownLoadHelper
    {
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="dateTime"></param>
        /// <param name="downloadFolder"></param>
        /// <param name="fileNamePrefix"></param>
        /// <returns></returns>
        public static async Task Download(List<string> urls, DateTime dateTime, string downloadFolder, string fileNamePrefix)
        {
            string file = string.Empty;
            try
            {
                var downloader = new DownloadService();
                foreach (var url in urls)
                {
                    Uri uri = new Uri(url);
                    file = downloadFolder + "\\" + fileNamePrefix + Path.GetFileName(uri.LocalPath);
                    if (!File.Exists(file))
                        await downloader.DownloadFileTaskAsync(url, file);

                    //修改文件日期时间为发博的时间
                    File.SetCreationTime(file, dateTime);
                    File.SetLastWriteTime(file, dateTime);
                    File.SetLastAccessTime(file, dateTime);
                }
            }
            catch
            {
                Debug.WriteLine(file);
                Trace.Listeners.Add(new TextWriterTraceListener(downloadFolder + "\\_FailedFiles.txt", "myListener"));
                Trace.TraceInformation(file);
                Trace.Flush();
            }
        }
    }
}
