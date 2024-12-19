using System.Windows;
using API_music.Service;
using API_music.ViewModels;
using LottieSharp.WPF;

namespace MusicRecognitionApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            LoadAnimations.LoadAnimation(LottieAnimation1, "girl_music.json");
            //LoadAnimations.LoadAnimation(LottieAnimation2, "loading.json");
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}