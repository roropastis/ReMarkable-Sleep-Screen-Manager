using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RemarkableSleepScreenManager.Localization
{
    public class LocalizedString : INotifyPropertyChanged
    {
        private readonly string _key;
        private string _value;

        public LocalizedString(string key)
        {
            _key = key;
            _value = ResourceManager.GetString(key);
            ResourceManager.CultureChanged += OnCultureChanged;
        }

        public string Value
        {
            get => _value;
            private set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnCultureChanged(object? sender, EventArgs e)
        {
            Value = ResourceManager.GetString(_key);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static implicit operator string(LocalizedString localizedString)
        {
            return localizedString.Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
