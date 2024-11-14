using CelebrityQuiz.Model;
using CelebrityQuiz.Service;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using MaterialDesignColors;

namespace CelebrityQuiz
{
    public partial class MainWindow : Window
    {
        private int currentScore = 0;
        private Actor currentActor;
        private List<string> currentOptions;
        private Random random = new Random();
        private DispatcherTimer timer;
        private int timeLeft = 30;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            StartNewQuestion();
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timeLeft--;
            TimerText.Text = timeLeft.ToString();

            if (timeLeft <= 0)
            {
                timer.Stop();
                DisableAnswerButtons();
                ShowCorrectAnswer();
            }
        }

        private async void StartNewQuestion()
        {
            try
            {
                TmdbService tmdbService = new TmdbService();
                currentActor = await tmdbService.GetRandomActorAsync();

                if (currentActor != null)
                {
                    if (!string.IsNullOrEmpty(currentActor.ImageUrl))
                    {
                        ActorImage.Source = new BitmapImage(new Uri(currentActor.ImageUrl, UriKind.Absolute));
                    }
                    else
                    {
                        ActorImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/no_image.png"));
                    }

                    currentOptions = await GenerateOptions(tmdbService, currentActor);

                    Option1.Content = currentOptions[0];
                    Option2.Content = currentOptions[1];
                    Option3.Content = currentOptions[2];
                    Option4.Content = currentOptions[3];

                    EnableAnswerButtons();
                    ResetButtonStyles();
                    ResetTimer();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading question: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<List<string>> GenerateOptions(TmdbService tmdbService, Actor correctActor)
        {
            var options = new List<string> { correctActor.Name };

            while (options.Count < 4)
            {
                var wrongActor = await tmdbService.GetRandomActorAsync();
                if (wrongActor != null && !options.Contains(wrongActor.Name) && correctActor.Gender.Equals(wrongActor.Gender))
                {
                    options.Add(wrongActor.Name);
                }
            }

            return options.OrderBy(x => random.Next()).ToList();
        }

        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            var button = (Button)sender;
            var selectedAnswer = button.Content.ToString();

            DisableAnswerButtons();

            if (selectedAnswer == currentActor.Name)
            {
                button.Background = new SolidColorBrush(Colors.Green);
                currentScore++;
                ScoreText.Text = currentScore.ToString();
            }
            else
            {
                button.Background = new SolidColorBrush(Colors.Red);
                ShowCorrectAnswer();
            }
        }

        private void GetRandomActorButton_Click(object sender, RoutedEventArgs e)
        {
            StartNewQuestion();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            currentScore = 0;
            ScoreText.Text = "0";
            StartNewQuestion();
        }

        private void EnableAnswerButtons()
        {
            Option1.IsEnabled = true;
            Option2.IsEnabled = true;
            Option3.IsEnabled = true;
            Option4.IsEnabled = true;
        }

        private void DisableAnswerButtons()
        {
            Option1.IsEnabled = false;
            Option2.IsEnabled = false;
            Option3.IsEnabled = false;
            Option4.IsEnabled = false;
        }

        private void ResetButtonStyles()
        {
            Option1.ClearValue(Button.BackgroundProperty);
            Option2.ClearValue(Button.BackgroundProperty);
            Option3.ClearValue(Button.BackgroundProperty);
            Option4.ClearValue(Button.BackgroundProperty);
        }

        private void ShowCorrectAnswer()
        {
            if (Option1.Content.ToString() == currentActor.Name)
                Option1.Background = new SolidColorBrush(Colors.Green);
            else if (Option2.Content.ToString() == currentActor.Name)
                Option2.Background = new SolidColorBrush(Colors.Green);
            else if (Option3.Content.ToString() == currentActor.Name)
                Option3.Background = new SolidColorBrush(Colors.Green);
            else if (Option4.Content.ToString() == currentActor.Name)
                Option4.Background = new SolidColorBrush(Colors.Green);
        }

        private void ResetTimer()
        {
            timeLeft = 30;
            TimerText.Text = timeLeft.ToString();
            timer.Start();
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            if (ThemeToggle.IsChecked == true)
            {
                theme.SetBaseTheme(BaseTheme.Dark);
                ((PackIcon)ThemeToggle.Content).Kind = PackIconKind.WeatherSunny;
            }
            else
            {
                theme.SetBaseTheme(BaseTheme.Light);
                ((PackIcon)ThemeToggle.Content).Kind = PackIconKind.WeatherNight;
            }

            paletteHelper.SetTheme(theme);
        }
    }
}