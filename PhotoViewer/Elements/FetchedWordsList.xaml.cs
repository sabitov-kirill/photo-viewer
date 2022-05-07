using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.ObjectModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using http = Windows.Web.Http;

namespace PhotoViewer.Elements
{
    public class FetchedWord : Common.BindableBase
    {
        private string content;
        public string Content
        {
            get { return content; }
            set { SetProperty(ref content, value); }
        }

        public static async Task<List<FetchedWord>> FetchNew(int ItemsToFetch)
        {
            List<FetchedWord> words = new List<FetchedWord>();

            http.HttpClient httpClient = new http.HttpClient();

            http.Headers.HttpRequestHeaderCollection headers = httpClient.DefaultRequestHeaders;
            Uri requestUri = new Uri($"https://random-word-api.herokuapp.com/word?number={ItemsToFetch}");
            http.HttpResponseMessage httpResponse = new http.HttpResponseMessage();

            try
            {
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                string httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

                words.Clear();
                foreach (string word in JsonDocument.Parse(httpResponseBody).Deserialize<List<string>>())
                    words.Add(new FetchedWord(word));

                return words;
            }
            catch
            {
                return words;
            }
        }

        public FetchedWord(string Word)
        {
            Content = Word;
        }
    }

    public sealed partial class FetchedWordsList : UserControl
    {
        public ObservableCollection<FetchedWord> Words { get; set; }

        public static readonly DependencyProperty WordsCountProperty =
            DependencyProperty.Register(
                "WordsCount",
                typeof(int),
                typeof(FetchedWordsList),
                new PropertyMetadata(10)
            );
        public int WordsCount
        {
            get { return (int)GetValue(WordsCountProperty); }
            set { SetValue(WordsCountProperty, value); FetchWords(); }
        }

        public FetchedWordsList()
        {
            this.InitializeComponent();
            Words = new ObservableCollection<FetchedWord>();
        }

        private async void FetchWords()
        {
            fetchingProgress.Visibility = Visibility.Visible;

            // Fetch new words
            List<FetchedWord> words = await FetchedWord.FetchNew(WordsCount);

            // Clear old worsd, set new
            if (Words == null) Words = new ObservableCollection<FetchedWord>();
            else Words.Clear();
            foreach (FetchedWord word in words) Words.Add(word);

            fetchingProgress.Visibility = Visibility.Collapsed;
        }

        private void wordsList_Click(object sender, RoutedEventArgs e)
        {
            FetchWords();
        }
    }
}

