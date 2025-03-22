using System;
using System.ComponentModel;

namespace TahoTibor
{
    public class ChatMessage : INotifyPropertyChanged
    {
        private string _content;
        private string _senderName;
        private DateTime _timestamp;
        private bool _isFromMe;
        private string _profilePictureUrl;

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        public string SenderName
        {
            get { return _senderName; }
            set
            {
                _senderName = value;
                OnPropertyChanged(nameof(SenderName));
            }
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
            set
            {
                _timestamp = value;
                OnPropertyChanged(nameof(Timestamp));
            }
        }

        public string TimestampFormatted => Timestamp.ToString("HH:mm");

        public bool IsFromMe
        {
            get { return _isFromMe; }
            set
            {
                _isFromMe = value;
                OnPropertyChanged(nameof(IsFromMe));
            }
        }

        public string ProfilePictureUrl
        {
            get { return _profilePictureUrl; }
            set
            {
                _profilePictureUrl = value;
                OnPropertyChanged(nameof(ProfilePictureUrl));
                OnPropertyChanged(nameof(HasProfilePicture));
            }
        }

        public bool HasProfilePicture => !string.IsNullOrEmpty(ProfilePictureUrl);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}