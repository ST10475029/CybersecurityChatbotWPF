
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CybersecurityChatbotWPF
{
    public class Chatbot
    {
        // Internal access for state checking from MainWindow.xaml.cs if needed
        internal Dictionary<string, string> userMemory = new Dictionary<string, string>();
        private string currentTopic = "";
        private List<string> conversationHistory = new List<string>();
        private List<CybersecurityTask> tasks = new List<CybersecurityTask>();

        // Events to communicate with the UI layer
        public event EventHandler<string> BotResponseReady;
        public event EventHandler RequestUserInput;

        // Predefined dictionaries for structural mapping (OOP/Data Structure Optimization)
        private Dictionary<string, string[]> generalResponses = new Dictionary<string, string[]>
        {
            { "how are you?", new string[] { "I'm a bot, so I don't have feelings, but I'm here to help!", "I'm functioning optimally.", "Ready to assist you!" } },
            { "what's your purpose?", new string[] { "I provide cybersecurity tips to keep you safe online.", "My purpose is to raise awareness about cybersecurity threats and best practices.", "I'm here to educate you on how to protect yourself in the digital world." } },
            { "what can i ask you about?", new string[] { "You can ask me about password safety, phishing, scams, privacy, safe browse, malware, and social engineering.", "I can provide information on various cybersecurity topics.", "Feel free to ask me anything related to online security." } },
            { "exit", new string[] { "Goodbye! Stay safe online.", "Thank you for chatting. Be secure!", "Have a safe digital experience!" } }
        };

        private Dictionary<string, string[]> cybersecurityResponses = new Dictionary<string, string[]>
        {
            { "password", new string[] {
                "Make sure to use strong, unique passwords for each account. Avoid using personal details in your passwords.",
                "A strong password should include a mix of uppercase and lowercase letters, numbers, and symbols.",
                "Consider using a password manager to securely store and generate complex passwords.",
                "Enable two-factor authentication (2FA) whenever possible for an extra layer of security on your accounts.",
                "Change your passwords regularly, especially for critical accounts."
            }},
            { "phishing", new string[] {
                "Be cautious of emails, messages, or calls asking for personal information, login credentials, or financial details. Scammers often disguise themselves as trusted organizations.",
                "Never click on suspicious links or download attachments from unknown senders. Verify the sender's authenticity through official channels.",
                "Pay attention to the sender's email address. Look for unusual spellings or domains that don't match the legitimate organization.",
                "If something seems too good to be true (like winning a lottery you didn't enter), it's likely a phishing attempt.",
                "Hover your mouse over links before clicking to see the actual URL. Be wary of URLs that look unfamiliar or use misleading subdomains."
            }},
            { "scam", new string[] {
                "If an online offer sounds too good to be true, it's highly likely a scam. Never send money, crypto, or gift cards to unverified entities.",
                "Be wary of urgent requests from strangers online demanding payment or claims that your accounts will be locked immediately.",
                "Always double-check website URLs before making purchases or typing credentials. Look out for slightly misspelled brand names."
            }},
            { "privacy", new string[] {
                "Review the privacy and safety settings on your social media accounts regularly to control what information strangers can see.",
                "Be mindful of the personal details you share online. Avoid broadcasting your location, birth date, or phone number publicly.",
                "Consider using privacy-focused browsers or extensions that actively block malicious web trackers and third-party cookies."
            }},
            { "safe browse", new string[] {
                "Keep your web browser and its extensions up to date to benefit from the latest security patches.",
                "Avoid visiting suspicious or unverified websites. Look for the 'HTTPS' and a padlock icon in the address bar, indicating a secure connection.",
                "Be cautious about downloading files or installing software from untrusted sources.",
                "Use a reputable antivirus and anti-malware software and keep it updated.",
                "Consider using browser extensions that enhance privacy and security, such as ad blockers and tracker blockers."
            }},
            { "malware", new string[] {
                "Malware is malicious software that can harm your device and steal your information. Be cautious of suspicious downloads and links.",
                "Install and regularly update antivirus and anti-malware software to protect your system.",
                "Avoid opening email attachments from unknown senders.",
                "Be wary of software offered for free from unofficial websites.",
                "Regularly scan your system for malware."
            }},
            { "social engineering", new string[] {
                "Social engineering involves manipulating people into divulging confidential information. Be skeptical of unexpected requests for personal details.",
                "Never share your passwords or sensitive information with anyone you don't trust, especially over the phone or email.",
                "Be wary of individuals claiming to be from technical support asking for remote access to your computer.",
                "Verify the identity of anyone asking for sensitive information through official channels.",
                "Educate yourself about common social engineering tactics."
            }}
        };

        public Chatbot()
        {
            // Indicate that the bot is awaiting the user's name initially
            userMemory["awaitingName"] = "true";
        }

        // Method to start the chatbot's initial interaction
        public void StartChatbot()
        {
            OnBotResponseReady("Hello! Welcome to your Cybersecurity Awareness Bot.");
            OnBotResponseReady("You can ask me about cybersecurity topics like passwords, phishing, scams, privacy, safe browse, malware, or social engineering.");
            OnBotResponseReady("Enter your name: ");
            OnRequestUserInput();
        }

        // Main method to process user input from the UI
        public void ProcessUserInput(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                OnBotResponseReady("Please enter a valid question.");
                return;
            }

            // Standardize string casing for keyword checking robustness
            string lowerQuestion = question.ToLower().Trim();

            conversationHistory.Add($"User: {question}");

            // State management: captures name
            if (userMemory.ContainsKey("awaitingName"))
            {
                string name = question.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    OnBotResponseReady("Name cannot be empty. Please enter your name: ");
                    OnRequestUserInput();
                }
                else
                {
                    userMemory["name"] = name;
                    userMemory.Remove("awaitingName");
                    OnBotResponseReady($"Hello, {name}! Welcome to your Cybersecurity Awareness Bot.");
                    OnBotResponseReady("You can ask me about cybersecurity topics like passwords, phishing, scams, privacy, safe browse, malware, or social engineering.");
                    OnBotResponseReady("You can also ask me to 'add task' or 'show tasks'.");
                    OnBotResponseReady("What cybersecurity topic are you most interested in? ");
                    OnRequestUserInput();
                    userMemory["awaitingFavoriteTopic"] = "true";
                }
                return;
            }

            // State management: captures favorite topic
            if (userMemory.ContainsKey("awaitingFavoriteTopic"))
            {
                string favoriteTopic = lowerQuestion;
                if (!string.IsNullOrWhiteSpace(favoriteTopic))
                {
                    userMemory["favoriteTopic"] = favoriteTopic;
                    OnBotResponseReady($"Great! I'll remember that you're interested in {question.Trim()}. It's a crucial part of staying safe online.");
                    userMemory.Remove("awaitingFavoriteTopic");
                }
                return;
            }

            bool found = false;

            // Check for Task Assistant commands
            if (lowerQuestion.Contains("add task") || lowerQuestion.Contains("create task") || lowerQuestion.Contains("new task"))
            {
                AddTaskPrompt();
                found = true;
            }
            else if (lowerQuestion.Contains("show tasks") || lowerQuestion.Contains("list tasks") || lowerQuestion.Contains("what are my tasks"))
            {
                DisplayTasks();
                found = true;
            }
            else if (lowerQuestion.Contains("complete task") || lowerQuestion.Contains("mark task complete"))
            {
                MarkTaskCompletePrompt();
                found = true;
            }
            else if (lowerQuestion.Contains("delete task") || lowerQuestion.Contains("remove task"))
            {
                DeleteTaskPrompt();
                found = true;
            }
            // Check for general conversation history
            else if (lowerQuestion.Contains("conversation history") || lowerQuestion.Contains("chat history"))
            {
                DisplayConversationHistory();
                found = true;
            }

            // Core Cybersecurity Keyword Detection & Random Array Response Pulling
            if (!found)
            {
                foreach (var keywordResponsePair in cybersecurityResponses)
                {
                    if (lowerQuestion.Contains(keywordResponsePair.Key))
                    {
                        currentTopic = keywordResponsePair.Key;
                        Random random = new Random();
                        string response = keywordResponsePair.Value[random.Next(keywordResponsePair.Value.Length)];

                        OnBotResponseReady(response);
                        conversationHistory.Add($"Bot: {response}");
                        found = true;
                        break;
                    }
                }
            }

            // General Static Interactions Matching 
            if (!found)
            {
                foreach (var generalResponsePair in generalResponses)
                {
                    if (lowerQuestion == generalResponsePair.Key)
                    {
                        currentTopic = generalResponsePair.Key;
                        Random random = new Random();
                        string response = generalResponsePair.Value[random.Next(generalResponsePair.Value.Length)];
                        OnBotResponseReady(response);
                        conversationHistory.Add($"Bot: {response}");
                        found = true;

                        if (generalResponsePair.Key == "exit")
                        {
                            SaveConversation();
                        }
                        break;
                    }
                }
            }

            // Conversation Flow: Maintains Context and Tracks Follow-up Expressions
            if (!found && !string.IsNullOrEmpty(currentTopic))
            {
                if (lowerQuestion.Contains("more") || lowerQuestion.Contains("details") || lowerQuestion.Contains("explain") || lowerQuestion.Contains("another tip"))
                {
                    string additionalInfo = "";
                    switch (currentTopic)
                    {
                        case "password":
                            additionalInfo = "For more details on password safety, consider using a long passphrase, which is a sentence that's easy to remember but hard for computer scripts to guess. Also, use multi-factor verification wherever possible.";
                            break;
                        case "phishing":
                            additionalInfo = "To protect yourself further against phishing, look closely at email headers. Legitimate banks or services will never request multi-factor tokens or private verification over SMS codes out of the blue.";
                            break;
                        case "scam":
                            additionalInfo = "When dealing with online scams, remember that high-pressure sales strategies or artificial urgency are massive red flags. Legitimate authorities never threaten arrest over online payments.";
                            break;
                        case "privacy":
                            additionalInfo = "To tighten up your online footprint privacy, remove data permissions from apps you don't use, delete stale old online accounts, and restrict search engines from indexing your social profiles.";
                            break;
                        case "safe browse":
                            additionalInfo = "When trying to browse safely, verify that your home network router firmware is updated frequently and your internal firewalls are working effectively to isolate data packets.";
                            break;
                        case "malware":
                            additionalInfo = "To limit modern automated malware vectors, maintain a zero-trust perspective. Never drop mystery flash memory drives into your computer ports, and let your security suite perform deep periodic scans.";
                            break;
                        case "social engineering":
                            additionalInfo = "Keep in mind that social engineers systematically manipulate baseline human instincts like fear or sympathy. Always hang up and call an organization using public official phone listings.";
                            break;
                        default:
                            additionalInfo = "Cybersecurity is continuous! Review your settings, stick to long passwords, and remain hyper-vigilant about links.";
                            break;
                    }
                    OnBotResponseReady(additionalInfo);
                    conversationHistory.Add($"Bot: {additionalInfo}");
                    found = true;
                }
            }

            // Context Recall Interaction Framework
            if (!found && lowerQuestion.Contains("remember") && userMemory.ContainsKey("name"))
            {
                string memoryResponse = $"Yes, {userMemory["name"]}, I remember you!";
                if (userMemory.ContainsKey("favoriteTopic"))
                {
                    memoryResponse += $" You mentioned earlier that you are highly focused on learning about '{userMemory["favoriteTopic"]}'.";
                }
                OnBotResponseReady(memoryResponse);
                conversationHistory.Add($"Bot: {memoryResponse}");
                found = true;
            }

            if (!found && lowerQuestion.Contains("interested in") && userMemory.ContainsKey("favoriteTopic"))
            {
                string favoriteTopicResponse = $"Since you are interested in {userMemory["favoriteTopic"]}, here's another tip related to it...";
                OnBotResponseReady(favoriteTopicResponse);
                conversationHistory.Add($"Bot: {favoriteTopicResponse}");
                switch (userMemory["favoriteTopic"])
                {
                    case "password":
                        OnBotResponseReady("Consider using a password strength checker tool online to evaluate the robustness of your passwords.");
                        conversationHistory.Add($"Bot: Consider using a password strength checker tool online to evaluate the robustness of your passwords.");
                        break;
                    case "phishing":
                        OnBotResponseReady("Be aware that phishing attempts can also occur via SMS (smishing) or phone calls (vishing).");
                        conversationHistory.Add($"Bot: Be aware that phishing attempts can also occur via SMS (smishing) or phone calls (vishing).");
                        break;
                    case "safe browse":
                        OnBotResponseReady("Regularly clear your browse history, cookies, and cache to protect your privacy.");
                        conversationHistory.Add($"Bot: Regularly clear your browse history, cookies and cache to protect your privacy.");
                        break;
                    case "malware":
                        OnBotResponseReady("Enable automatic updates for your operating system and applications to patch security vulnerabilities.");
                        conversationHistory.Add($"Bot: Enable automatic updates for your operating system and applications to patch security vulnerabilities.");
                        break;
                    case "social engineering":
                        OnBotResponseReady("Be cautious of sharing too much personal information on social media platforms, as this can be used for social engineering attacks.");
                        conversationHistory.Add($"Bot: Be cautious of sharing too much personal information on social media platforms.");
                        break;
                    default:
                        OnBotResponseReady("That's an interesting topic!");
                        conversationHistory.Add($"Bot: That's an interesting topic!");
                        break;
                }
                found = true;
            }

            // Sentiment Detection & Response Customization
            if (!found && (lowerQuestion.Contains("worried") || lowerQuestion.Contains("concerned") || lowerQuestion.Contains("anxious") || lowerQuestion.Contains("scared")))
            {
                Random random = new Random();
                string tip = cybersecurityResponses["scam"][random.Next(cybersecurityResponses["scam"].Length)];

                string empathyResponse = $"It's completely understandable to feel worried about digital safety. Threats can seem overwhelming, but staying informed keeps you secure! Here is an essential tip right now to protect you from threats: {tip}";

                OnBotResponseReady(empathyResponse);
                conversationHistory.Add($"Bot: {empathyResponse}");
                found = true;
            }
            else if (!found && (lowerQuestion.Contains("curious") || lowerQuestion.Contains("learn more") || lowerQuestion.Contains("interested")))
            {
                Random random = new Random();
                string tip = cybersecurityResponses["privacy"][random.Next(cybersecurityResponses["privacy"].Length)];

                string curiosityResponse = $"It is wonderful that you are curious! Proactive learning is your best cybersecurity defense tool. Here is a direct insight on protecting personal metrics: {tip}";

                OnBotResponseReady(curiosityResponse);
                conversationHistory.Add($"Bot: {curiosityResponse}");
                found = true;
            }
            else if (!found && (lowerQuestion.Contains("frustrated") || lowerQuestion.Contains("confused") || lowerQuestion.Contains("difficult") || lowerQuestion.Contains("hard")))
            {
                Random random = new Random();
                string tip = cybersecurityResponses["password"][random.Next(cybersecurityResponses["password"].Length)];

                string frustrationResponse = $"I completely understand how frustrating and tricky cybersecurity configurations can feel. Let's make things painless. Try focusing solely on this one clear practice first: {tip}";

                OnBotResponseReady(frustrationResponse);
                conversationHistory.Add($"Bot: {frustrationResponse}");
                found = true;
            }

            // Global Safe Exception Fallback / Unknown Input Edge Case Handler
            if (!found)
            {
                string unknownResponse = "I'm not sure I understand that request. Can you try rephrasing? You can ask me questions about passwords, phishing, scams, privacy, malware, or tracking tasks.";
                OnBotResponseReady(unknownResponse);
                conversationHistory.Add($"Bot: {unknownResponse}");
            }
        }

        private void AddTaskPrompt()
        {
            OnBotResponseReady("Okay, let's add a new cybersecurity task. What is the title of the task? (e.g., 'Update antivirus software'): ");
            userMemory["awaitingTaskTitle"] = "true";
            OnRequestUserInput();
        }

        public void SetTaskTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                OnBotResponseReady("Task title cannot be empty. Task not added. Please enter a valid title:");
                OnRequestUserInput();
                return;
            }

            userMemory["currentTaskTitle"] = title;
            OnBotResponseReady("Do you want to set a reminder for this task? (yes/no): ");
            userMemory["awaitingReminderChoice"] = "true";
            OnRequestUserInput();
        }

        public void SetReminderChoice(string choice)
        {
            string title = userMemory["currentTaskTitle"];
            userMemory.Remove("awaitingReminderChoice");

            if (choice.ToLower() == "yes")
            {
                OnBotResponseReady("When should I remind you? (e.g., 'tomorrow', 'in 3 days', '2025-12-25'): ");
                userMemory["awaitingReminderInput"] = "true";
                OnRequestUserInput();
            }
            else
            {
                tasks.Add(new CybersecurityTask(title));
                OnBotResponseReady($"Task '{title}' has been added.");
                userMemory.Remove("currentTaskTitle");
            }
        }

        public void SetReminderDate(string reminderInput)
        {
            string title = userMemory["currentTaskTitle"];
            userMemory.Remove("awaitingReminderInput");
            userMemory.Remove("currentTaskTitle");

            DateTime? reminderDate = null;
            string reminderDescription = reminderInput;

            if (!string.IsNullOrWhiteSpace(reminderInput))
            {
                if (reminderInput.Contains("tomorrow"))
                {
                    reminderDate = DateTime.Now.AddDays(1);
                }
                else if (reminderInput.Contains("in ") && reminderInput.Contains("day"))
                {
                    if (int.TryParse(reminderInput.Split(' ')[1], out int days))
                    {
                        reminderDate = DateTime.Now.AddDays(days);
                    }
                }
                else if (DateTime.TryParse(reminderInput, out DateTime parsedDate))
                {
                    reminderDate = parsedDate;
                }
            }

            tasks.Add(new CybersecurityTask(title, reminderDate, reminderDescription));
            OnBotResponseReady($"Task '{title}' has been added.");
            if (reminderDate.HasValue)
            {
                OnBotResponseReady($"Reminder set for {reminderDate.Value.ToShortDateString()}.");
            }
        }

        private void DisplayTasks()
        {
            StringBuilder taskOutput = new StringBuilder();
            taskOutput.AppendLine("--- Your Cybersecurity Tasks ---");
            if (tasks.Any())
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    var task = tasks[i];
                    string status = task.IsCompleted ? "[COMPLETED]" : "[PENDING]";
                    string reminder = task.ReminderDate.HasValue ? $" (Reminder: {task.ReminderDate.Value.ToShortDateString()} - {task.ReminderDescription})" : "";
                    taskOutput.AppendLine($"{i + 1}. {status} {task.Title}{reminder}");
                }
            }
            else
            {
                taskOutput.AppendLine("No cybersecurity tasks added yet. Why not add one?");
            }
            taskOutput.AppendLine("--- End of Tasks ---");
            OnBotResponseReady(taskOutput.ToString());
        }

        private void MarkTaskCompletePrompt()
        {
            if (!tasks.Any())
            {
                OnBotResponseReady("You don't have any tasks to mark as complete.");
                return;
            }
            DisplayTasks();
            OnBotResponseReady("Enter the number of the task to mark as complete: ");
            userMemory["awaitingTaskCompletionNumber"] = "true";
            OnRequestUserInput();
        }

        public void MarkTaskComplete(string input)
        {
            userMemory.Remove("awaitingTaskCompletionNumber");
            if (int.TryParse(input, out int taskNumber) && taskNumber > 0 && taskNumber <= tasks.Count)
            {
                tasks[taskNumber - 1].MarkComplete();
                OnBotResponseReady($"Task '{tasks[taskNumber - 1].Title}' marked as complete!");
            }
            else
            {
                OnBotResponseReady("Invalid task number.");
            }
        }

        private void DeleteTaskPrompt()
        {
            if (!tasks.Any())
            {
                OnBotResponseReady("You don't have any tasks to delete.");
                return;
            }
            DisplayTasks();
            OnBotResponseReady("Enter the number of the task to delete: ");
            userMemory["awaitingTaskDeletionNumber"] = "true";
            OnRequestUserInput();
        }

        public void DeleteTask(string input)
        {
            userMemory.Remove("awaitingTaskDeletionNumber");
            if (int.TryParse(input, out int taskNumber) && taskNumber > 0 && taskNumber <= tasks.Count)
            {
                string deletedTaskTitle = tasks[taskNumber - 1].Title;
                tasks.RemoveAt(taskNumber - 1);
                OnBotResponseReady($"Task '{deletedTaskTitle}' has been deleted.");
            }
            else
            {
                OnBotResponseReady("Invalid task number.");
            }
        }

        private void SaveConversation()
        {
            try
            {
                string filePath = "conversation.txt";
                File.WriteAllLines(filePath, conversationHistory);
                OnBotResponseReady($"Conversation saved to {filePath}");
            }
            catch (Exception ex)
            {
                OnBotResponseReady($"Error saving conversation: {ex.Message}");
            }
        }

        private void DisplayConversationHistory()
        {
            StringBuilder historyOutput = new StringBuilder();
            historyOutput.AppendLine("--- Conversation History ---");
            foreach (string line in conversationHistory)
            {
                historyOutput.AppendLine(line);
            }
            historyOutput.AppendLine("--- End of History ---");
            OnBotResponseReady(historyOutput.ToString());
        }

        protected virtual void OnBotResponseReady(string message)
        {
            BotResponseReady?.Invoke(this, message);
        }

        protected virtual void OnRequestUserInput()
        {
            RequestUserInput?.Invoke(this, EventArgs.Empty);
        }

        // --- Nested Domain-Specific Model Classes (Encapsulation Practice) ---
        public class CybersecurityTask
        {
            public string Title { get; private set; }
            public DateTime? ReminderDate { get; private set; }
            public string ReminderDescription { get; private set; }
            public bool IsCompleted { get; private set; }
            public CybersecurityTask(string title, DateTime? reminderDate = null, string reminderDescription = null)
            {
                Title = title;
                ReminderDate = reminderDate;
                ReminderDescription = reminderDescription;
                IsCompleted = false;
            }
            public void MarkComplete()
            {
                IsCompleted = true;
            }
        }
    }
}