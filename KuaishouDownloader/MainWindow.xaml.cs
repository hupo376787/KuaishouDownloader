using KuaishouDownloader.Models;
using Newtonsoft.Json;
using RestSharp;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace KuaishouDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        string downloadFolder = AppContext.BaseDirectory;
        SnackbarService? snackbarService = null;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            snackbarService = new SnackbarService();
            snackbarService.SetSnackbarPresenter(snackbarPresenter);

            if (File.Exists("AppConfig.json"))
            {
                var model = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText("AppConfig.json"));
                if (model != null)
                {
                    tbUid.Text = model.Uid;
                    tbCookie.Text = model.Cookie;
                }
            }
        }

        private void Theme_Click(object sender, RoutedEventArgs e)
        {
            if (Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Light)
            {
                themeButton.Icon = new SymbolIcon(SymbolRegular.WeatherSunny48);
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Dark);
            }
            else
            {
                themeButton.Icon = new SymbolIcon(SymbolRegular.WeatherMoon48);
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Light);
            }
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnDownload.IsEnabled = false;
                btnParseJson.IsEnabled = false;

                if (string.IsNullOrEmpty(tbUid.Text) || string.IsNullOrEmpty(tbCookie.Text))
                {
                    snackbarService?.Show("提示", $"请输入uid以及cookie", ControlAppearance.Caution, null, TimeSpan.FromSeconds(3));
                    return;
                }

                var json = JsonConvert.SerializeObject(new AppConfig() { Uid = tbUid.Text, Cookie = tbCookie.Text }, Formatting.Indented);
                File.WriteAllText("AppConfig.json", json);

                var options = new RestClientOptions("https://live.kuaishou.com")
                {
                    Timeout = TimeSpan.FromSeconds(15),
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36",
                };
                var client = new RestClient(options);
                var request = new RestRequest($"/live_api/profile/public?count=9999&pcursor=&principalId={tbUid.Text}&hasMore=true", Method.Get);
                request.AddHeader("host", "live.kuaishou.com");
                request.AddHeader("connection", "keep-alive");
                request.AddHeader("cache-control", "max-age=0");
                request.AddHeader("sec-ch-ua", "\"Not)A;Brand\";v=\"99\", \"Google Chrome\";v=\"127\", \"Chromium\";v=\"127\"");
                request.AddHeader("sec-ch-ua-mobile", "?0");
                request.AddHeader("sec-ch-ua-platform", "\"Windows\"");
                request.AddHeader("upgrade-insecure-requests", "1");
                request.AddHeader("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                request.AddHeader("sec-fetch-site", "none");
                request.AddHeader("sec-fetch-mode", "navigate");
                request.AddHeader("sec-fetch-user", "?1");
                request.AddHeader("sec-fetch-dest", "document");
                request.AddHeader("accept-encoding", "gzip, deflate, br, zstd");
                request.AddHeader("accept-language", "zh,en;q=0.9,zh-CN;q=0.8");
                request.AddHeader("cookie", tbCookie.Text);
                request.AddHeader("x-postman-captr", "9467712");
                RestResponse response = await client.ExecuteAsync(request);
                Debug.WriteLine(response.Content);

                var model = JsonConvert.DeserializeObject<KuaishouModel>(response.Content!);
                if (model == null || model?.Data?.List == null || model?.Data?.List?.Count == 0)
                {
                    snackbarService?.Show("提示", $"获取失败，可能触发了快手的风控机制，请等一段时间再试。", ControlAppearance.Danger, null, TimeSpan.FromSeconds(3));
                    return;
                }

                await Download(model!);
            }
            finally
            {
                btnDownload.IsEnabled = true;
                btnParseJson.IsEnabled = true;
            }
        }

        private async void ParseJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnDownload.IsEnabled = false;
                btnParseJson.IsEnabled = false;

                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.Filter = "Json文件(.Json)|*.json";
                bool? result = dialog.ShowDialog();
                if (result == false)
                {
                    return;
                }
                var model = JsonConvert.DeserializeObject<KuaishouModel>(File.ReadAllText(dialog.FileName)!);
                if (model == null || model?.Data?.List == null || model?.Data?.List?.Count == 0)
                {
                    snackbarService?.Show("提示", $"不是正确的json", ControlAppearance.Caution, null, TimeSpan.FromSeconds(3));
                    return;
                }

                await Download(model!);
            }
            finally
            {
                btnDownload.IsEnabled = true;
                btnParseJson.IsEnabled = true;
            }
        }

        private async Task Download(KuaishouModel model)
        {
            progress.Value = 0;
            progress.Minimum = 0;
            progress.Maximum = (double)model?.Data?.List?.Count!;
            snackbarService?.Show("提示", $"解析到{model?.Data?.List?.Count!}个作品，开始下载", ControlAppearance.Success, null, TimeSpan.FromSeconds(5));

            imgHeader.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(model?.Data?.List?[0]?.Author?.Avatar!));
            tbNickName.Text = model?.Data?.List?[0]?.Author?.Name;

            string pattern = @"\d{4}/\d{2}/\d{2}/\d{2}";

            for (int i = 0; i < model?.Data?.List!.Count; i++)
            {
                DateTime dateTime = DateTime.Now;
                string fileNamePrefix = "";
                var item = model?.Data?.List[i]!;
                Match match = Regex.Match(item.Poster!, pattern);
                if (match.Success)
                {
                    dateTime = new DateTime(int.Parse(match.Value.Split("/")[0]), int.Parse(match.Value.Split("/")[1]),
                        int.Parse(match.Value.Split("/")[2]), int.Parse(match.Value.Split("/")[3]), 0, 0);
                    if (cbAddDate.IsChecked == true)
                        fileNamePrefix = match.Value.Split("/")[0] + "-" + match.Value.Split("/")[1] + "-" + match.Value.Split("/")[2]
                            + " " + match.Value.Split("/")[3] + "-00-00 ";
                }
                downloadFolder = Path.Combine(AppContext.BaseDirectory, "Download", item?.Author?.Name! + "(" + item?.Author?.Id! + ")");
                Directory.CreateDirectory(downloadFolder);

                switch (item?.WorkType)
                {
                    case "single":
                    case "vertical":
                    case "multiple":
                        {
                            await DownLoadHelper.Download(item?.ImgUrls!, dateTime, downloadFolder, fileNamePrefix);
                        }
                        break;
                    case "video":
                        {
                            await DownLoadHelper.Download(new List<string>() { item?.PlayUrl! }, dateTime, downloadFolder, fileNamePrefix);
                        }
                        break;
                }

                progress.Value = i + 1;
                tbProgress.Text = $"{i + 1} / {model?.Data?.List!.Count}";
                Random random = new Random();
                if (cbLongInterval.IsChecked == true)
                    await Task.Delay(random.Next(5000, 10000));
                else
                    await Task.Delay(random.Next(1000, 5000));
            }

            snackbarService?.Show("提示", $"下载完成，共下载{model?.Data?.List!.Count}个作品", ControlAppearance.Success, null, TimeSpan.FromDays(1));
        }

        private void CopyUrl(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(tbUid.Text))
            {
                snackbarService?.Show("提示", "请输入uid以及cookie", ControlAppearance.Caution, null, TimeSpan.FromSeconds(3));
                return;
            }
            Clipboard.SetText($"https://live.kuaishou.com/live_api/profile/public?count=9999&pcursor=&principalId={tbUid.Text}&hasMore=true");

            snackbarService?.Show("提示", "复制完成，请粘贴到浏览器打开", ControlAppearance.Success, null, TimeSpan.FromSeconds(3));
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            flyout.IsOpen = true;
        }
    }
}