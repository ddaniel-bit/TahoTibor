using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace TahoTibor
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<ChatMessage> Messages { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Initialize collection and set as DataContext
            Messages = new ObservableCollection<ChatMessage>();
            DataContext = this;

            // Add some sample messages
            AddSampleMessages();
        }

        private void AddSampleMessages()
        {
            Messages.Add(new ChatMessage
            {
                Content = "Hey there! How are you?",
                SenderName = "Friend",
                Timestamp = DateTime.Now.AddMinutes(-10),
                IsFromMe = false
            });

            Messages.Add(new ChatMessage
            {
                Content = "I'm good, thanks for asking! What about you?",
                Timestamp = DateTime.Now.AddMinutes(-8),
                IsFromMe = true
            });

            Messages.Add(new ChatMessage
            {
                Content = "I'm doing great. Did you finish the project we were talking about?",
                SenderName = "Friend",
                Timestamp = DateTime.Now.AddMinutes(-5),
                IsFromMe = false
            });
        }

        private void SendMessage()
        {
            string messageText = MessageTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(messageText))
            {
                // Add the new message
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

                // Simulate a response (for demo purposes)
                SimulateResponse();
            }
        }

        private void SimulateResponse()
        {
            // This is just for demonstration
            string[] responses = new[]
            {
                "That's interesting!",
                "Tell me more about it.",
                "I see what you mean.",
                "OK, I'll keep that in mind.",
                "Thanks for sharing that with me!"
            };

            // Pick a random response
            Random random = new Random();
            string response = responses[random.Next(responses.Length)];

            // Add the response after a delay (in a real app, you'd connect to a service)
            System.Threading.Tasks.Task.Delay(1000).ContinueWith(t =>
            {
                Dispatcher.Invoke(() =>
                {
                    Messages.Add(new ChatMessage
                    {
                        Content = response,
                        SenderName = "Friend",
                        Timestamp = DateTime.Now,
                        IsFromMe = false
                    });

                    // Scroll to the bottom
                    ChatListView.ScrollIntoView(Messages[Messages.Count - 1]);
                });
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