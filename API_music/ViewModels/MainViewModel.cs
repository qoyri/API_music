using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using API_music.Commands;
using API_music.Service;
using MugenMvvmToolkit.Models;
using RelayCommand = API_music.Commands.RelayCommand;

namespace API_music.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private const string HistoryFilePath = "search_history.json";
        private bool _isLoading;
        private bool _hasAlbumImage;
        private bool _isRecording; // Nouveau booléen pour vérifier l'état d'enregistrement.


        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLottieVisible));
                OnPropertyChanged(nameof(IsAlbumImageVisible));
            }
        }

        public bool HasAlbumImage
        {
            get => _hasAlbumImage;
            set
            {
                _hasAlbumImage = value;
                Debug.WriteLine($"HasAlbumImage updated to: {_hasAlbumImage}"); // Journal
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAlbumImageVisible));
            }
        }

        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                _isRecording = value;
                OnPropertyChanged();
            }
        }

        private void ResetDisplay()
        {
            Strings.AlbumImageUrl = null; // Réinitialiser l'album
            Strings.SpotifyLink = null; // Supprimer le lien
            Strings.SongTitle = null; // Réinitialiser le titre
            Strings.Artist = null; // Réinitialiser l'artiste
            HasAlbumImage = false; // Cacher les informations
            Debug.WriteLine("Display reset successfully."); // Journalisation
        }

        // Control visibility for UI
        public bool IsLottieVisible => IsLoading;
        public bool IsAlbumImageVisible => !IsLoading && HasAlbumImage;

        public ObservableCollection<MusicEntry> SearchHistory { get; private set; }
        public Strings Strings { get; private set; }

        // Commands
        public ICommand CaptureCommand { get; private set; }
        public static ICommand OpenSpotifyLinkCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand MinimizeCommand { get; private set; }

        public MainViewModel()
        {
            Strings = new Strings
            {
                AlbumImageUrl = null
            };

            HasAlbumImage = false;
            SearchHistory = LoadHistory();

            CaptureCommand = new RelayCommand(async () => await CaptureAndRecognizeMusicAsync());
            OpenSpotifyLinkCommand = new Wpf.Ui.Input.RelayCommand<string>(OpenSpotifyLink);
            CloseCommand = new RelayCommand(() => Application.Current.Shutdown());
            MinimizeCommand = new RelayCommand(MinimizeWindow);
        }

        /// <summary>
        /// Captures and recognizes music, updates UI with results.
        /// </summary>
        private async Task CaptureAndRecognizeMusicAsync()
        {
            ResetDisplay();
            
            if (IsRecording)
            {
                // Montre une notification si l'utilisateur tente de relancer l'enregistrement.
                MessageBox.Show("An audio recording is already in progress. Please wait until it completes.",
                    "Recording in Progress", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            IsRecording = true; // Marque le début de l'enregistrement.

            var outputFile = "recording.wav";

            try
            {
                Strings.StatusMessage = "Capturing audio...";
                var soundCapture = new SoundCapture();
                await soundCapture.CaptureAudioAsync(5, outputFile);

                var wavFile = new FileInfo(outputFile);
                if (!wavFile.Exists || wavFile.Length == 0)
                {
                    throw new Exception("Recording failed or file is empty.");
                }

                Strings.StatusMessage = "Recognizing music...";
                var musicInfo = await ShazamClient.RecognizeMusicAsync(wavFile);

                // Partie inchangée : Extraction des titres, artistes et recherche Spotify.
                var infoParts = musicInfo.Split(", ");
                string title = infoParts.Length > 0 ? infoParts[0].Replace("Title: ", "") : "Unknown Title";
                string artist = infoParts.Length > 1 ? infoParts[1].Replace("Artist: ", "") : "Unknown Artist";

                Strings.StatusMessage = "Searching on Spotify...";
                var (spotifyLink, albumImageUrl) = await SpotifyClient.SearchSongOnSpotifyAsync(title, artist);

                if (string.IsNullOrWhiteSpace(albumImageUrl))
                {
                    albumImageUrl =
                        "https://static.vecteezy.com/system/resources/previews/023/986/494/non_2x/spotify-logo-spotify-logo-transparent-spotify-icon-transparent-free-free-png.png";
                    HasAlbumImage = false; // Pas d'image disponible
                }
                else
                {
                    HasAlbumImage = true; // Une image a été trouvée
                }

                Strings.AlbumImageUrl = albumImageUrl;
                Strings.SpotifyLink = spotifyLink;

                Strings.SongTitle = title;
                Strings.Artist = artist;

                Strings.StatusMessage = $"Process completed. {musicInfo}. Spotify Link: {spotifyLink}";
                

                AddToSearchHistory(new MusicEntry
                {
                    Name = $"{title} - {artist}",
                    Link = spotifyLink
                });
            }
            catch (Exception ex)
            {
                Strings.StatusMessage = $"Error: {ex.Message}";
                Console.WriteLine(ex);
            }
            finally
            {
                IsRecording = false; // Libère l'état à la fin de l'enregistrement.
            }
        }

        /// <summary>
        /// Opens the provided Spotify link in the browser.
        /// </summary>
        private void OpenSpotifyLink(string link)
        {
            if (!string.IsNullOrEmpty(link))
            {
                Debug.WriteLine($"Opening Spotify link: {link}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = link,
                    UseShellExecute = true
                });
            }
            else
            {
                Debug.WriteLine("Spotify link is empty or null, cannot open.");
            }
        }

        /// <summary>
        /// Minimizes the main application window.
        /// </summary>
        private void MinimizeWindow()
        {
            Debug.WriteLine("Minimizing application window.");
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }
        }

        /// <summary>
        /// Loads previously saved search history from a JSON file.
        /// </summary>
        private ObservableCollection<MusicEntry> LoadHistory()
        {
            if (!File.Exists(HistoryFilePath))
            {
                Debug.WriteLine("No history file found. Starting with an empty history.");
                return new ObservableCollection<MusicEntry>();
            }

            Debug.WriteLine("Loading search history from file...");
            var json = File.ReadAllText(HistoryFilePath);

            var history = JsonConvert.DeserializeObject<ObservableCollection<MusicEntry>>(json);
            Debug.WriteLine($"Loaded {history?.Count ?? 0} entries from history.");
            return history ?? new ObservableCollection<MusicEntry>();
        }

        /// <summary>
        /// Saves the current search history to a JSON file.
        /// </summary>
        private void SaveHistory()
        {
            Debug.WriteLine($"Saving {SearchHistory.Count} history entries to file...");
            var json = JsonConvert.SerializeObject(SearchHistory, Formatting.Indented);
            File.WriteAllText(HistoryFilePath, json);
            Debug.WriteLine("Search history saved successfully.");
        }

        /// <summary>
        /// Adds a new entry to the search history and updates the saved history file.
        /// </summary>
        private void AddToSearchHistory(MusicEntry entry)
        {
            Debug.WriteLine($"Adding new entry to history: {entry.Name}, {entry.Link}");
            Application.Current.Dispatcher.Invoke(() =>
            {
                SearchHistory.Insert(0, entry); // Add to start of the list
                SaveHistory();
            });
        }
    }

    /// <summary>
    /// Represents a music entry for search history.
    /// </summary>
    public class MusicEntry
    {
        public string Name { get; set; }
        public string Link { get; set; }
    }
}