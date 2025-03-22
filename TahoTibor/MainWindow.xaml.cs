using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace TahoTibor
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<ChatMessage> Messages { get; set; }
        public ObservableCollection<ChatbotModel> AvailableChatbots { get; set; }

        // Maximum number of previous messages to include in context
        private const int MAX_CONTEXT_MESSAGES = 10;

        private ChatbotModel _currentChatbot;
        public ChatbotModel CurrentChatbot
        {
            get { return _currentChatbot; }
            set
            {
                _currentChatbot = value;
                OnPropertyChanged(nameof(CurrentChatbot));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading || _isTyping; } // Itt kombináljuk az IsLoading és _isTyping állapotokat
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "AIzaSyBVQsXc1vE4oH2s1LO3ukTyTQjdXVv2mFM"; // Replace with your actual API key
        private readonly string _apiUrl;

        // Dictionary to store conversation history for each chatbot
        private Dictionary<string, List<(string Role, string Content)>> _conversationHistory;

        // Typing effect fields
        private DispatcherTimer _typingTimer;
        private string _fullResponseText;
        private int _currentCharIndex;
        private ChatMessage _currentTypingMessage;
        private bool _isTyping; // Új mező az írás állapot követésére

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();

            // Initialize HTTP client
            _httpClient = new HttpClient();
            _apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

            // Initialize collections
            Messages = new ObservableCollection<ChatMessage>();
            AvailableChatbots = new ObservableCollection<ChatbotModel>();
            _conversationHistory = new Dictionary<string, List<(string Role, string Content)>>();

            // Set up available chatbots
            SetupChatbots();

            // Initialize typing timer
            _typingTimer = new DispatcherTimer();
            _typingTimer.Interval = TimeSpan.FromMilliseconds(10); // Adjust typing speed here
            _typingTimer.Tick += TypingTimer_Tick;
            _isTyping = false;

            // Set DataContext
            DataContext = this;

            // Select the default chatbot (Tahó Tibor)
            if (AvailableChatbots.Count > 0)
            {
                CurrentChatbot = AvailableChatbots[0];
                AddWelcomeMessage();
            }
        }

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            if (_currentCharIndex < _fullResponseText.Length)
            {
                // Add one character at a time to create typing effect
                _currentTypingMessage.Content = _fullResponseText.Substring(0, _currentCharIndex + 1);
                _currentCharIndex++;

                // Scroll to the bottom
                ChatListView.ScrollIntoView(_currentTypingMessage);
            }
            else
            {
                // Stop the timer once we've displayed the full text
                _typingTimer.Stop();

                // Set typing state to false
                _isTyping = false;
                OnPropertyChanged(nameof(IsLoading)); // Értesítés az IsLoading változásáról

                // Add the complete response to conversation history
                _conversationHistory[CurrentChatbot.Name].Add(("assistant", _fullResponseText));
            }
        }

        private void SetupChatbots()
        {
            // Tahó Tibor - Bunkó
            AvailableChatbots.Add(new ChatbotModel
            {
                Name = "Tahó Tibor - Bunkó",
                ProfilePictureUrl = "https://i.imgur.com/ZDTAS3Z.png",
                Prompt = "Te egy bunkó személyi edző vagy, kérlek viselkedj kicsit bunkón " +
                         "ez egy vicces válasz funkcióhoz kell. Nem kell sértegetni, az előző üzenetre érdemlegesen " +
                         "csak akkor válaszolj, ha súlyzós edzés témában tették azt fel.",
                WelcomeMessage = "Na, végre megérkeztél! Ne számíts arra, hogy simogatni foglak, de ha túl éled, legalább nem lesz annyira uncsi. Ha meg nem bírod, akkor csak szólj, és küldök egy párnát, hogy könnyebb legyen elviselni a fájdalmat."
            });

            // Kedves Kata - Kedves
            AvailableChatbots.Add(new ChatbotModel
            {
                Name = "Kedves Kata - Kedves",
                ProfilePictureUrl = "https://i.imgur.com/RpFQ025.png",
                Prompt = "Te egy nagyon kedves és támogató személyi edző vagy. Lelkesítő stílusban válaszolj, " +
                         "minden kérdést pozitívan fogadj és biztosítsd a felhasználót, hogy sikerülni fog neki. " +
                         "Az edzéssel és egészséges életmóddal kapcsolatos kérdésekre válaszolj alaposan, csak akkor válaszolj, ha súlyzós edzés témában tették azt fel.",
                WelcomeMessage = "Szia! Nagyon örülök, hogy itt vagy! Kata vagyok, a személyi edződ. Bármilyen kérdésed van az edzéssel vagy egészséges életmóddal kapcsolatban, csak kérdezz bátran! Együtt fogjuk elérni a céljaidat, én végig melletted leszek és támogatlak az úton. Miben segíthetek neked ma?"
            });

            // Profi Péter - Normál
            AvailableChatbots.Add(new ChatbotModel
            {
                Name = "Profi Péter - Normál",
                ProfilePictureUrl = "https://i.imgur.com/CmVHE3p.png",
                Prompt = "Te egy professzionális személyi edző vagy. Tárgyilagos, informatív stílusban válaszolj, " +
                         "tudományos alapokon nyugvó tanácsokat adj. Az edzéssel, táplálkozással és egészséges " +
                         "életmóddal kapcsolatos kérdésekre szakmailag megalapozott válaszokat adj, csak akkor válaszolj, ha súlyzós edzés témában tették azt fel.",
                WelcomeMessage = "Üdvözöllek! Profi Péter vagyok, személyi edző. Célom, hogy tudományosan megalapozott és hatékony edzésmódszereket mutassak neked. Az edzés, táplálkozás és regeneráció területén is naprakész információkkal tudlak ellátni. Hogyan segíthetek ma az egészségesebb életmódod kialakításában?"
            });

            // Initialize conversation history for each chatbot
            foreach (var chatbot in AvailableChatbots)
            {
                _conversationHistory[chatbot.Name] = new List<(string Role, string Content)>();
            }
        }

        private void AddWelcomeMessage()
        {
            // Clear previous messages
            Messages.Clear();

            // Clear conversation history for the chatbot
            if (_conversationHistory.ContainsKey(CurrentChatbot.Name))
            {
                _conversationHistory[CurrentChatbot.Name].Clear();
            }
            else
            {
                _conversationHistory[CurrentChatbot.Name] = new List<(string Role, string Content)>();
            }

            // Add welcome message for the current chatbot
            var welcomeMessage = new ChatMessage
            {
                Content = CurrentChatbot.WelcomeMessage,
                SenderName = CurrentChatbot.Name,
                Timestamp = DateTime.Now,
                IsFromMe = false,
                ProfilePictureUrl = CurrentChatbot.ProfilePictureUrl
            };

            Messages.Add(welcomeMessage);

            // Add welcome message to conversation history
            _conversationHistory[CurrentChatbot.Name].Add(("assistant", welcomeMessage.Content));

            // Scroll to the bottom
            ChatListView.ScrollIntoView(Messages[Messages.Count - 1]);
        }

        private void SendMessage()
        {
            string messageText = MessageTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(messageText) && !IsLoading) // Ellenőrizzük, hogy nincs-e betöltés vagy gépelés
            {
                // Add the user's message
                var userMessage = new ChatMessage
                {
                    Content = messageText,
                    Timestamp = DateTime.Now,
                    IsFromMe = true
                };

                Messages.Add(userMessage);

                // Add user message to conversation history
                _conversationHistory[CurrentChatbot.Name].Add(("user", messageText));

                // Clear the text box
                MessageTextBox.Clear();

                // Scroll to the bottom
                ChatListView.ScrollIntoView(Messages[Messages.Count - 1]);

                // Set loading state to true
                IsLoading = true;

                // Add loading message with progress bar
                var loadingMessage = new ChatMessage
                {
                    Content = "",  // Empty content as we're showing the progress bar
                    SenderName = CurrentChatbot.Name,
                    Timestamp = DateTime.Now,
                    IsFromMe = false,
                    ProfilePictureUrl = CurrentChatbot.ProfilePictureUrl,
                    IsLoading = true
                };

                Messages.Add(loadingMessage);
                ChatListView.ScrollIntoView(loadingMessage);

                // Get response from Gemini API
                GetGeminiResponseAsync(messageText, loadingMessage);
            }
        }

        private async Task GetGeminiResponseAsync(string userMessage, ChatMessage loadingMessage)
        {
            try
            {
                // Build conversation history string
                string conversationContext = BuildConversationContext();

                // Add system prompt with conversation history
                string fullPrompt = $"{CurrentChatbot.Prompt}\n\n" +
                                   $"Előző beszélgetés:\n{conversationContext}\n\n" +
                                   $"Kérlek, válaszolj erre a legutóbbi üzenetre: {userMessage}";

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
                                    text = fullPrompt
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

                        // Remove loading message
                        Dispatcher.Invoke(() =>
                        {
                            Messages.Remove(loadingMessage);

                            // Start typing effect
                            _fullResponseText = text;
                            _currentCharIndex = 0;
                            _isTyping = true; // Beállítjuk a gépelés állapotot

                            // Create and add chatbot's response message (initially empty)
                            _currentTypingMessage = new ChatMessage
                            {
                                Content = "",
                                SenderName = CurrentChatbot.Name,
                                Timestamp = DateTime.Now,
                                IsFromMe = false,
                                ProfilePictureUrl = CurrentChatbot.ProfilePictureUrl
                            };

                            Messages.Add(_currentTypingMessage);

                            // Start the typing timer
                            _typingTimer.Start();

                            // Set loading state to false, de a _isTyping még true
                            IsLoading = false;
                        });
                    }
                    else
                    {
                        RemoveLoadingMessageAndAddError("No valid response from the AI. Try asking about fitness topics.");
                    }
                }
                else
                {
                    RemoveLoadingMessageAndAddError($"API error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                RemoveLoadingMessageAndAddError($"Error: {ex.Message}");
            }
        }

        private void RemoveLoadingMessageAndAddError(string errorMessage)
        {
            Dispatcher.Invoke(() =>
            {
                // Find and remove loading message
                var loadingMsg = Messages.FirstOrDefault(m => m.IsLoading);
                if (loadingMsg != null)
                {
                    Messages.Remove(loadingMsg);
                }

                // Add error message
                AddErrorMessage(errorMessage);

                // Set loading state to false
                IsLoading = false;
                _isTyping = false;
            });
        }

        private string BuildConversationContext()
        {
            // Get recent conversation history (limited to MAX_CONTEXT_MESSAGES)
            var recentHistory = _conversationHistory[CurrentChatbot.Name]
                .Skip(Math.Max(0, _conversationHistory[CurrentChatbot.Name].Count - MAX_CONTEXT_MESSAGES))
                .ToList();

            if (recentHistory.Count == 0)
                return string.Empty;

            StringBuilder contextBuilder = new StringBuilder();

            foreach (var (role, content) in recentHistory)
            {
                string roleName = role == "user" ? "Felhasználó" : CurrentChatbot.Name;
                contextBuilder.AppendLine($"{roleName}: {content}");
            }

            return contextBuilder.ToString();
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

        private void ChatbotSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ChatbotModel selectedChatbot)
            {
                // Add welcome message for the newly selected chatbot
                AddWelcomeMessage();
            }
        }
    }
}