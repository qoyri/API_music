using System.Diagnostics;
using API_music.ViewModels;
using MugenMvvmToolkit.Models;

namespace API_music.Commands
{
    public class Strings : ObservableObject
    {
        private string _statusMessage;
        private string _albumImageUrl;
        private string _spotifyLink;
        private string _songTitle;
        private string _artist;

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public string AlbumImageUrl
        {
            get => _albumImageUrl;
            set
            {
                _albumImageUrl = value;
                OnPropertyChanged(nameof(AlbumImageUrl));
                Debug.WriteLine($"AlbumImageUrl changed: {_albumImageUrl}");
            }
        }

        public string SpotifyLink
        {
            get => _spotifyLink;
            set
            {
                _spotifyLink = value;
                OnPropertyChanged(nameof(SpotifyLink));

                // Adjust this to use the correct generic type
                if (MainViewModel.OpenSpotifyLinkCommand is RelayCommand<string> command)
                {
                    command.RaiseCanExecuteChanged();
                }
            }
        }

        public string SongTitle
        {
            get => _songTitle;
            set
            {
                _songTitle = value;
                OnPropertyChanged(nameof(SongTitle));
            }
        }

        public string Artist
        {
            get => _artist;
            set
            {
                _artist = value;
                OnPropertyChanged(nameof(Artist));
            }
        }
    }
}