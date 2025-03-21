using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace TahoTibor
{
    public class ChatMessage : INotifyPropertyChanged
    {
        private string _content;
        private string _senderName;
        private DateTime _timestamp;
        private bool _isFromMe;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}