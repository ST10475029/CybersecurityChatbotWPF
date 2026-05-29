using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading; // For Dispatcher
using System.Media; // Add this namespace for SoundPlayer
using System.IO;    // Add this namespace for Path

namespace CybersecurityChatbotWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Chatbot chatbot;

        public MainWindow()
        {
            InitializeComponent();
            chatbot = new Chatbot();
            // Subscribe to events from the chatbot
            chatbot.BotResponseReady += Chatbot_BotResponseReady;
            chatbot.RequestUserInput += Chatbot_RequestUserInput; // Listen for specific input requests

            // Play greeting audio
            PlayGreetingAudio();

            // Start the chatbot's initial conversation
            chatbot.StartChatbot();
            UserInputTextBox.Focus(); // Set focus to input box on startup
        }

        private void PlayGreetingAudio()
        {
            try
            {
                // Construct the full path to your WAV file in the output directory
                // Assumes 'Audio' folder exists in your project and 'greeting.wav' is inside it,
                // and its 'Copy to Output Directory' property is set to 'Copy if newer'.
                string audioFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio", "greeting.wav");

                if (File.Exists(audioFilePath))
                {
                    SoundPlayer player = new SoundPlayer(audioFilePath);
                    player.Load(); // Load the audio
                    player.Play(); // Play the audio once
                }
                else
                {
                    // Log or display an error if the file isn't found
                    Dispatcher.Invoke(() =>
                    {
                        ChatDisplay.Text += "Bot: Warning: Greeting audio file not found at " + audioFilePath + Environment.NewLine;
                    });
                }
            }
            catch (Exception ex)
            {
                // Log or display any errors during audio playback
                Dispatcher.Invoke(() =>
                {
                    ChatDisplay.Text += "Bot: Error playing greeting audio: " + ex.Message + Environment.NewLine;
                });
            }
        }


        private void Chatbot_BotResponseReady(object sender, string message)
        {
            // Update UI on the main thread
            Dispatcher.Invoke(() =>
            {
                ChatDisplay.Text += "Bot: " + message + Environment.NewLine;
                // Scroll to the end of the text
                ((ScrollViewer)ChatDisplay.Parent).ScrollToEnd();
            });
        }

        private void Chatbot_QuizEnded(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UserInputTextBox.IsEnabled = true; // Re-enable input if it was disabled for quiz
                SendButton.IsEnabled = true;
                UserInputTextBox.Focus();
            });
        }

        private void Chatbot_RequestUserInput(object sender, EventArgs e)
        {
            // This event indicates the chatbot is awaiting a specific input.
            // For now, we just ensure the input box is focused.
            Dispatcher.Invoke(() =>
            {
                UserInputTextBox.Focus();
            });
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void SendMessage()
        {
            string userMessage = UserInputTextBox.Text.Trim();
            UserInputTextBox.Clear(); // Clear input field

            if (string.IsNullOrEmpty(userMessage))
            {
                return;
            }

            ChatDisplay.Text += "You: " + userMessage + Environment.NewLine;
            // Scroll to the end of the text
            ((ScrollViewer)ChatDisplay.Parent).ScrollToEnd();

            // Directly call ProcessUserInput; the Chatbot class manages its internal state
            // and the specific input handling (e.g., awaiting name, task title)
            // is now entirely within the Chatbot.ProcessUserInput method.
            chatbot.ProcessUserInput(userMessage);

            UserInputTextBox.Focus(); // Keep focus on the input box
        }
    }
}