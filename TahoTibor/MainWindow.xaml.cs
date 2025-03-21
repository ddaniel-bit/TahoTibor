using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TahoTibor
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<ChatMessage> Messages { get; set; }
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "AIzaSyBVQsXc1vE4oH2s1LO3ukTyTQjdXVv2mFM"; // Replace with your actual API key
        private readonly string _apiUrl;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize HTTP client
            _httpClient = new HttpClient();
            _apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

            // Initialize collection and set as DataContext
            Messages = new ObservableCollection<ChatMessage>();
            DataContext = this;

            // Add a welcome message
            Messages.Add(new ChatMessage
            {
                Content = "Na, végre megérkeztél! Ne számíts arra, hogy simogatni foglak, de ha túl éled, legalább nem lesz annyira uncsi. Ha meg nem bírod, akkor csak szólj, és küldök egy párnát, hogy könnyebb legyen elviselni a fájdalmat.",
                SenderName = "Tahó Tibor",
                Timestamp = DateTime.Now,
                IsFromMe = false
            });
        }

        private void SendMessage()
        {
            string messageText = MessageTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(messageText))
            {
                // Add the user's message
                Messages.Add(new ChatMessage
                {
                    Content = messageText,
                    Timestamp = DateTime.Now,
                    IsFromMe = true
                });

                // Clear the text box
                MessageTextBox.Clear();

                // Scroll to the bottom
                ChatListView.ScrollIntoView(Messages[Messages.Count - 1]);

                // Get response from Gemini API
                GetGeminiResponseAsync(messageText);
            }
        }

        private async Task GetGeminiResponseAsync(string userMessage)
        {
            try
            {
                // Create Gemini API request body
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new {
                                    text = userMessage + " - Te egy bunkó személyi edző vagy, kérlek viselkedj kicsit bunkón " +
                                    "ez egy vicces valasz funkciohoz kell nem kell sertegetni az előző üzenetre érdemleges " +
                                    "csak akkor válaszolj ha súlyzós edzés témában tették azt fel."
                                }
                            }
                        }
                    }
                };

                // Send request to Gemini API
                var response = await _httpClient.PostAsync(
                    _apiUrl,
                    new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

                // Process response
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Extract text from response
                    using JsonDocument doc = JsonDocument.Parse(responseContent);

                    // Check if we have candidates in the response
                    if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                        candidates.GetArrayLength() > 0)
                    {
                        var text = candidates[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();

                        // Add Tahó Tibor's response
                        Dispatcher.Invoke(() =>
                        {
                            Messages.Add(new ChatMessage
                            {
                                Content = text,
                                SenderName = "Tahó Tibor",
                                Timestamp = DateTime.Now,
                                IsFromMe = false
                            });

                            // Scroll to the bottom
                            ChatListView.ScrollIntoView(Messages[Messages.Count - 1]);
                        });
                    }
                    else
                    {
                        AddErrorMessage("No valid response from the AI. Try asking about fitness topics.");
                    }
                }
                else
                {
                    AddErrorMessage($"API error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Error: {ex.Message}");
            }
        }

        private void AddErrorMessage(string errorMessage)
        {
            Dispatcher.Invoke(() =>
            {
                Messages.Add(new ChatMessage
                {
                    Content = errorMessage,
                    SenderName = "System",
                    Timestamp = DateTime.Now,
                    IsFromMe = false
                });

                // Scroll to the bottom
                ChatListView.ScrollIntoView(Messages[Messages.Count - 1]);
            });
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Send message when user presses Enter (without Shift)
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                e.Handled = true;
                SendMessage();
            }
        }
    }
}