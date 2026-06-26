using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatBot_Generation
{
    public partial class MainWindow : Window
    {
        private Chatbot bot;
        private SpeechSynthesizer speaker;
        private bool isSpeaking = false;
        private Queue<string> speechQueue = new Queue<string>();

        public MainWindow()
        {
            InitializeComponent();

            speaker = new SpeechSynthesizer();
            speaker.Volume = 100;
            speaker.Rate = 0;

            bot = new Chatbot();

            SetAsciiArt();
            WelcomeMessage();
            RefreshTaskList();
            RefreshLogList();
        }

        private void SetAsciiArt()
        {
            string ascii = @"
    ╔═══════════════════════════════════════════════════════════╗
    ║          ██████╗██╗   ██╗██████╗ ███████╗██████╗        ║
    ║         ██╔════╝██║   ██║██╔══██╗██╔════╝██╔══██╗       ║
    ║        ██║     ██║   ██║██████╔╝█████╗  ██████╔╝       ║
             ██║     ██║   ██║██╔══██╗██╔══╝  ██╔══██╗       ║
             ╚██████╗██████╝██║  ██║███████╗██║  ██║       ║
              ═════╝ ═════╝ ╚═╝  ═╝══════╝═╝  ╚═╝       
    ║              CYBERSECURITY AWARENESS BOT                ║
    ╚═══════════════════════════════════════════════════════════╝";

            txtAscii.Text = ascii;
        }


        private void WelcomeMessage()
        {
            string welcome = "Hello! I'm your Cybersecurity Awareness Bot.\n\n" +
                             "I'm here to help you stay safe online. Ask me about: passwords, scams, phishing, privacy, security, or malware.\n\n" +
                             "What's your name?";

            AppendChat("Bot: " + welcome);
            Speak("Hello! Welcome to the Cybersecurity Awareness Bot. What is your name?");
        }

        private void Speak(string text)
        {
            if (isSpeaking)
            {
                speechQueue.Enqueue(text);
                return;
            }

            isSpeaking = true;
            speaker.SpeakCompleted += (s, e) =>
            {
                isSpeaking = false;
                if (speechQueue.Count > 0)
                    Speak(speechQueue.Dequeue());
            };
            speaker.SpeakAsync(text);
        }

        private void AppendChat(string text)
        {
            txtChat.AppendText(text + Environment.NewLine);
            txtChat.ScrollToEnd();
        }

        // ========== CHAT TAB EVENTS ==========
        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            string input = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            AppendChat("You: " + input);
            txtMessage.Clear();

            string response = bot.GetResponse(input);
            AppendChat("Bot: " + response);

            Speak(response);

            RefreshTaskList();
            RefreshLogList();
        }

        // ========== TASKS TAB EVENTS ==========
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string taskDesc = txtTaskInput.Text.Trim();
            if (!string.IsNullOrEmpty(taskDesc))
            {
                bot.AddTask(taskDesc);
                txtTaskInput.Clear();
                RefreshTaskList();
                MessageBox.Show("Task added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RefreshTaskList()
        {
            lstTasks.ItemsSource = null;
            lstTasks.ItemsSource = bot.Tasks;
        }

        // ========== QUIZ TAB EVENTS ==========
        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            bot.StartQuiz();
            btnStartQuiz.Visibility = Visibility.Collapsed;
            btnSubmitAnswer.Visibility = Visibility.Visible;
            txtQuizFeedback.Visibility = Visibility.Collapsed;
            btnNextQuestion.Content = "Next";
            DisplayCurrentQuestion();
        }

        private void DisplayCurrentQuestion()
        {
            var q = bot.GetCurrentQuestion();
            if (q != null)
            {
                txtQuizTitle.Text = "Cybersecurity Quiz";
                txtQuizProgress.Text = $"Question {bot.CurrentQuestionNumber} of {bot.TotalQuestions}";
                txtQuestion.Text = q.Question;
                optA.Content = "A) " + q.Options[0];
                optB.Content = "B) " + q.Options[1];
                optC.Content = "C) " + q.Options[2];
                optD.Content = "D) " + q.Options[3];

                optA.IsChecked = false;
                optB.IsChecked = false;
                optC.IsChecked = false;
                optD.IsChecked = false;
            }
        }

        private void SubmitAnswer_Click(object sender, RoutedEventArgs e)
        {
            string answer = "";
            if (optA.IsChecked == true) answer = "a";
            else if (optB.IsChecked == true) answer = "b";
            else if (optC.IsChecked == true) answer = "c";
            else if (optD.IsChecked == true) answer = "d";

            if (string.IsNullOrEmpty(answer))
            {
                MessageBox.Show("Please select an answer.", "No Answer Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string feedback = bot.SubmitAnswer(answer);
            txtQuizFeedback.Text = feedback;
            txtQuizFeedback.Visibility = Visibility.Visible;
            btnSubmitAnswer.Visibility = Visibility.Collapsed;

            if (bot.HasMoreQuestions)
            {
                btnNextQuestion.Visibility = Visibility.Visible;
            }
            else
            {
                txtQuizTitle.Text = "Quiz Complete!";
                txtQuizProgress.Text = $"Score: {bot.QuizScore} / {bot.TotalQuestions}";
                btnNextQuestion.Content = "Restart Quiz";
                btnNextQuestion.Visibility = Visibility.Visible;
            }
        }

        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (bot.HasMoreQuestions)
            {
                bot.NextQuestion();
                txtQuizFeedback.Visibility = Visibility.Collapsed;
                btnNextQuestion.Visibility = Visibility.Collapsed;
                btnSubmitAnswer.Visibility = Visibility.Visible;
                DisplayCurrentQuestion();
            }
            else
            {
                btnNextQuestion.Content = "Next";
                StartQuiz_Click(sender, e);
            }
        }

        // ========== ACTIVITY LOG TAB EVENTS ==========
        private void RefreshLog_Click(object sender, RoutedEventArgs e)
        {
            RefreshLogList();
        }

        private void RefreshLogList()
        {
            lstActivityLog.ItemsSource = null;
            lstActivityLog.ItemsSource = bot.ActivityLogs;
        }
    }

    // ========== CHATBOT CLASS ==========
    public class Chatbot
    {
        private static readonly Random random = new Random();
        private Dictionary<string, string> userMemory = new Dictionary<string, string>();
        private string currentTopic = "";
        private string lastResponse = "";

        public List<TaskItem> Tasks { get; private set; } = new List<TaskItem>();
        public List<ActivityLog> ActivityLogs { get; private set; } = new List<ActivityLog>();

        private bool quizActive = false;
        private int quizIndex = 0;
        public int QuizScore { get; private set; } = 0;
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();

        public bool IsQuizActive => quizActive;
        public bool HasMoreQuestions => quizIndex < quizQuestions.Count;
        public int CurrentQuestionNumber => quizIndex + 1;
        public int TotalQuestions => quizQuestions.Count;

        public class TaskItem
        {
            public string Description { get; set; } = "";
            public DateTime? Reminder { get; set; }
            public bool IsCompleted { get; set; }
            public DateTime Created { get; set; } = DateTime.Now;
        }

        public class ActivityLog
        {
            public DateTime Timestamp { get; set; } = DateTime.Now;
            public string Action { get; set; } = "";
            public string Details { get; set; } = "";
        }

        public class QuizQuestion
        {
            public string Question { get; set; } = "";
            public string[] Options { get; set; } = new string[0];
            public int CorrectAnswer { get; set; }
            public string Explanation { get; set; } = "";
        }

        private Dictionary<string, List<string>> keywordResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["password"] = new List<string>
            {
                "Make sure to use strong, unique passwords for each account. Avoid using personal details.",
                "Consider using a password manager to generate and store complex passwords securely.",
                "Enable multi-factor authentication wherever possible for an extra layer of security.",
                "A strong password should be at least 12 characters with uppercase, lowercase, numbers, and symbols."
            },
            ["scam"] = new List<string>
            {
                "Be cautious of unsolicited emails asking for personal information. Scammers often disguise themselves.",
                "Never click on suspicious links. Hover over them to see the actual URL before clicking.",
                "Legitimate organizations will never ask for sensitive information via email unexpectedly.",
                "If an offer seems too good to be true, it probably is a scam."
            },
            ["phishing"] = new List<string>
            {
                "Be cautious of emails asking for personal information. Verify the sender's address.",
                "Check for spelling and grammar errors - they're common in phishing attempts.",
                "Never enter login credentials on a page you reached via an email link.",
                "Phishing emails often create a sense of urgency to make you act without thinking."
            },
            ["privacy"] = new List<string>
            {
                "Review privacy settings on your social media accounts regularly.",
                "Be mindful of what you share online. Once posted, it's hard to remove completely.",
                "Use privacy-focused browsers and search engines to protect your activity.",
                "Avoid using public Wi-Fi for sensitive transactions without a VPN."
            },
            ["security"] = new List<string>
            {
                "Keep your software and operating system updated to protect against vulnerabilities.",
                "Use reputable antivirus software and keep it updated.",
                "Enable two-factor authentication on all important accounts.",
                "Regularly backup your important files to an external drive or cloud storage."
            },
            ["malware"] = new List<string>
            {
                "Only download software from trusted sources to avoid malware infections.",
                "Be careful with email attachments, even from known contacts.",
                "Run regular scans with your antivirus software.",
                "Keep your firewall enabled to block unauthorized access."
            }
        };

        private Dictionary<string, string> sentimentKeywords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["worried"] = "concerned",
            ["anxious"] = "concerned",
            ["scared"] = "concerned",
            ["afraid"] = "concerned",
            ["curious"] = "curious",
            ["interested"] = "curious",
            ["want to know"] = "curious",
            ["wonder"] = "curious",
            ["frustrated"] = "frustrated",
            ["annoyed"] = "frustrated",
            ["confused"] = "frustrated",
            ["overwhelmed"] = "frustrated"
        };

        public Chatbot()
        {
            InitializeQuiz();
            LogActivity("System", "Chatbot initialized");
        }

        public void AddTask(string description)
        {
            Tasks.Add(new TaskItem { Description = description });
            LogActivity("TaskAdded", description);
        }

        public QuizQuestion GetCurrentQuestion()
        {
            if (quizActive && quizIndex < quizQuestions.Count)
                return quizQuestions[quizIndex];
            return null;
        }

        public void StartQuiz()
        {
            quizActive = true;
            quizIndex = 0;
            QuizScore = 0;
            LogActivity("QuizStarted", "New quiz session");
        }

        public string SubmitAnswer(string answer)
        {
            if (!quizActive || quizIndex >= quizQuestions.Count) return "Quiz is not active.";

            int answerIndex = answer switch
            {
                "a" or "1" => 0,
                "b" or "2" => 1,
                "c" or "3" => 2,
                "d" or "4" => 3,
                _ => -1
            };

            if (answerIndex == -1) return "Invalid answer.";

            var q = quizQuestions[quizIndex];
            bool isCorrect = answerIndex == q.CorrectAnswer;

            if (isCorrect)
                QuizScore++;

            LogActivity("QuizAnswer", $"Q{CurrentQuestionNumber}: {(isCorrect ? "Correct" : "Incorrect")}");

            string result = (isCorrect ? "[Correct] " : $"[Incorrect] The answer was {(char)('A' + q.CorrectAnswer)}.") + "\n" + q.Explanation;
            return result;
        }

        public void NextQuestion()
        {
            if (quizIndex < quizQuestions.Count)
                quizIndex++;
        }

        public string GetResponse(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return "I didn't catch that. Could you please type your question?";

            LogActivity("UserQuery", userInput);

            if (userInput.IndexOf("help", StringComparison.OrdinalIgnoreCase) >= 0 || userInput == "?")
            {
                return "Available commands:\n" +
                       "  - 'add task [description]' - Create a new task\n" +
                       "  - 'show tasks' - List your tasks\n" +
                       "  - 'start quiz' - Begin cybersecurity quiz\n" +
                       "  - 'show log' - View activity log\n" +
                       "  - 'generate password' - Create a strong password\n" +
                       "  - Ask about: passwords, scams, phishing, privacy, security, malware";
            }

            if (!userMemory.ContainsKey("name"))
            {
                userMemory["name"] = userInput.Trim('.', ' ');
                LogActivity("UserRegistered", userMemory["name"]);
                return "Nice to meet you, " + userMemory["name"] + "! I'm here to help with cybersecurity. What would you like to know?";
            }

            if (userInput.IndexOf("add task", StringComparison.OrdinalIgnoreCase) >= 0 ||
                userInput.IndexOf("create task", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string taskDesc = userInput.Substring(userInput.IndexOf("task") + 4).Trim();
                if (!string.IsNullOrEmpty(taskDesc))
                {
                    AddTask(taskDesc);
                    return $"[OK] Task added: '{taskDesc}'\nCheck the Tasks tab to view it.";
                }
                return "Please specify a task. Example: 'add task Enable 2FA on email'";
            }

            if (userInput.IndexOf("show tasks", StringComparison.OrdinalIgnoreCase) >= 0 ||
                userInput.IndexOf("my tasks", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (Tasks.Count == 0)
                {
                    LogActivity("TaskView", "No tasks found");
                    return "No tasks yet. Use 'add task [description]' to create one.";
                }

                string list = $"Your Tasks ({Tasks.Count}):\n";
                for (int i = 0; i < Tasks.Count; i++)
                {
                    string status = Tasks[i].IsCompleted ? "[x]" : "[ ]";
                    list += $"  {i + 1}. {status} {Tasks[i].Description}\n";
                }
                LogActivity("TaskView", $"Displayed {Tasks.Count} tasks");
                return list;
            }

            if (userInput.IndexOf("delete task", StringComparison.OrdinalIgnoreCase) >= 0 ||
                userInput.IndexOf("remove task", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var parts = userInput.Split(new[] { ' ' }, 3);
                if (parts.Length >= 3)
                {
                    string keyword = parts[2].ToLower();
                    var task = Tasks.FirstOrDefault(t => t.Description.ToLower().Contains(keyword));
                    if (task != null)
                    {
                        Tasks.Remove(task);
                        LogActivity("TaskDeleted", task.Description);
                        return $"Deleted task: '{task.Description}'";
                    }
                }
                return "Task not found. Use 'show tasks' to see available tasks.";
            }

            if (userInput.IndexOf("complete task", StringComparison.OrdinalIgnoreCase) >= 0 ||
                userInput.IndexOf("done task", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var parts = userInput.Split(new[] { ' ' }, 3);
                if (parts.Length >= 3)
                {
                    string keyword = parts[2].ToLower();
                    var task = Tasks.FirstOrDefault(t => t.Description.ToLower().Contains(keyword));
                    if (task != null)
                    {
                        task.IsCompleted = true;
                        LogActivity("TaskCompleted", task.Description);
                        return $"[Done] Marked task as complete: '{task.Description}'";
                    }
                }
                return "Task not found. Use 'show tasks' to see available tasks.";
            }

            if (userInput.IndexOf("start quiz", StringComparison.OrdinalIgnoreCase) >= 0 ||
                userInput.IndexOf("begin quiz", StringComparison.OrdinalIgnoreCase) >= 0 ||
                userInput.IndexOf("take quiz", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                StartQuiz();
                return "Quiz started! Check the Quiz tab to begin answering questions.";
            }

            if (userInput.IndexOf("show log", StringComparison.OrdinalIgnoreCase) >= 0 ||
                userInput.IndexOf("activity log", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (ActivityLogs.Count == 0)
                    return "Activity log is empty.";

                string log = $"Recent Activity ({ActivityLogs.Count} entries):\n";
                var recent = ActivityLogs.Skip(Math.Max(0, ActivityLogs.Count - 10));
                foreach (var entry in recent)
                {
                    log += $"  - [{entry.Timestamp:HH:mm:ss}] {entry.Action}: {entry.Details}\n";
                }
                return log;
            }

            if (userInput.IndexOf("generate password", StringComparison.OrdinalIgnoreCase) >= 0 ||
                userInput.IndexOf("create password", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string pwd = GeneratePassword(16);
                LogActivity("PasswordGenerated", "Strong password created");
                return $"Your secure password:\n{pwd}\n\nSave it in a password manager!";
            }

            if (userInput.IndexOf("interested in", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int index = userInput.IndexOf("interested in", StringComparison.OrdinalIgnoreCase);
                string interest = userInput.Substring(index + 15).Split(' ', '.', ',')[0];
                if (!string.IsNullOrEmpty(interest))
                {
                    userMemory["interest"] = interest.Trim();
                    LogActivity("InterestNoted", interest);
                    return "Great! I'll remember you're interested in " + userMemory["interest"] + ". What specific aspect would you like to learn about?";
                }
            }

            if (IsFollowUpQuestion(userInput))
            {
                LogActivity("FollowUp", "Requested more information");
                return HandleFollowUp();
            }

            string sentiment = DetectSentiment(userInput);
            string keywordResponse = FindKeywordResponse(userInput, sentiment);
            if (keywordResponse != null)
            {
                LogActivity("TopicDiscussed", currentTopic);
                return keywordResponse;
            }

            if (userMemory.ContainsKey("interest") && random.Next(3) == 0)
                return "Since you're interested in " + userMemory["interest"] + ", remember that staying informed is key. What specific aspect would you like to know more about?";

            if (userMemory.ContainsKey("name") && random.Next(3) == 0)
                return "I'm not sure I understand, " + userMemory["name"] + ". Can you try rephrasing? You can ask about: passwords, scams, phishing, privacy, security, or malware.";

            return GetDefaultResponse(sentiment);
        }

        private string DetectSentiment(string input)
        {
            foreach (KeyValuePair<string, string> kw in sentimentKeywords)
            {
                if (input.IndexOf(kw.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                    return kw.Value;
            }
            return "neutral";
        }

        private string FindKeywordResponse(string input, string sentiment)
        {
            foreach (string kw in keywordResponses.Keys)
            {
                if (input.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    currentTopic = kw;
                    List<string> responses = keywordResponses[kw];
                    lastResponse = responses[random.Next(responses.Count)];
                    return AddSentimentAdjustment(lastResponse, sentiment);
                }
            }
            return null;
        }

        private string AddSentimentAdjustment(string response, string sentiment)
        {
            if (sentiment == "concerned")
                return "It's understandable to feel that way. " + response + " Let me know if you need more help.";
            if (sentiment == "frustrated")
                return "I understand this can be frustrating. " + response + " Take your time.";
            if (sentiment == "curious")
                return "Great question! " + response + " Would you like to know more?";
            return response;
        }

        private bool IsFollowUpQuestion(string input)
        {
            string[] phrases = { "tell me more", "explain more", "give me another", "more information", "what else", "anything else" };
            foreach (string p in phrases)
            {
                if (input.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private string HandleFollowUp()
        {
            if (!string.IsNullOrEmpty(currentTopic) && keywordResponses.ContainsKey(currentTopic))
            {
                List<string> responses = keywordResponses[currentTopic];
                string newResp = responses[random.Next(responses.Count)];
                while (newResp == lastResponse && responses.Count > 1)
                    newResp = responses[random.Next(responses.Count)];
                lastResponse = newResp;
                return "Here's another tip about " + currentTopic + ": " + newResp;
            }
            return "I'd be happy to provide more information. Which topic would you like to know more about?";
        }

        private string GetDefaultResponse(string sentiment)
        {
            if (sentiment == "concerned")
                return "I understand your concerns. What aspect of cybersecurity would you like to know about? I can help with passwords, scams, phishing, privacy, security, and malware.";
            if (sentiment == "frustrated")
                return "I understand this can be overwhelming. Let's take it step by step. What topic would you like to learn about?";
            return "I'm not sure I understand. Try asking about: passwords, scams, phishing, privacy, security, or malware. Or type 'help' for commands.";
        }

        public string GeneratePassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            return new string(result);
        }

        private void LogActivity(string action, string details)
        {
            ActivityLogs.Add(new ActivityLog { Action = action, Details = details });
            if (ActivityLogs.Count > 50)
                ActivityLogs.RemoveAt(0);
        }

        private void InitializeQuiz()
        {
            quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion {
                    Question = "What should you do if you receive a suspicious email asking for your password?",
                    Options = new[] { "Reply immediately", "Click the link", "Report as phishing", "Forward to friends" },
                    CorrectAnswer = 2,
                    Explanation = "Phishing emails try to steal credentials. Always report them to IT or delete them immediately."
                },
                new QuizQuestion {
                    Question = "What does 2FA stand for?",
                    Options = new[] { "Two-Factor Authentication", "Two-Firewall Access", "Two-File Archive", "Total File Access" },
                    CorrectAnswer = 0,
                    Explanation = "Two-Factor Authentication adds an extra security layer beyond passwords, like a code from your phone."
                },
                new QuizQuestion {
                    Question = "Which is the STRONGEST password?",
                    Options = new[] { "password123", "John2024", "Tr0ub4dor&3", "qwerty" },
                    CorrectAnswer = 2,
                    Explanation = "Strong passwords use mixed case, numbers, and symbols. Avoid personal information or common words."
                },
                new QuizQuestion {
                    Question = "What does HTTPS in a website URL indicate?",
                    Options = new[] { "Fast loading", "Encrypted connection", "New website", "Free service" },
                    CorrectAnswer = 1,
                    Explanation = "HTTPS encrypts data between you and the website, protecting your information from interception."
                },
                new QuizQuestion {
                    Question = "How often should you update your software?",
                    Options = new[] { "Never", "When convenient", "As soon as updates are available", "Once yearly" },
                    CorrectAnswer = 2,
                    Explanation = "Updates patch security vulnerabilities. Install them as soon as possible to stay protected."
                },
                new QuizQuestion {
                    Question = "What is malware?",
                    Options = new[] { "Hardware issue", "Malicious software", "Email type", "Network cable" },
                    CorrectAnswer = 1,
                    Explanation = "Malware (malicious software) includes viruses, ransomware, spyware, and trojans designed to harm your system."
                },
                new QuizQuestion {
                    Question = "Why should you use a VPN on public Wi-Fi?",
                    Options = new[] { "Faster speed", "Encrypt connection", "Save battery", "Block ads" },
                    CorrectAnswer = 1,
                    Explanation = "VPNs encrypt your traffic on unsecured public networks, protecting your data from hackers."
                },
                new QuizQuestion {
                    Question = "What is a phishing attack?",
                    Options = new[] { "Fishing technique", "Malware type", "Fraudulent information theft", "Network protocol" },
                    CorrectAnswer = 2,
                    Explanation = "Phishing uses deceptive emails or websites to trick you into revealing sensitive information."
                },
                new QuizQuestion {
                    Question = "Why is it important to backup your data?",
                    Options = new[] { "Free up space", "Protect against data loss", "Speed up PC", "Not important" },
                    CorrectAnswer = 1,
                    Explanation = "Backups protect against data loss from hardware failure, ransomware, or accidental deletion."
                },
                new QuizQuestion {
                    Question = "What makes a good password policy?",
                    Options = new[] { "Same password everywhere", "Simple words", "Complex unique passwords", "Write on sticky notes" },
                    CorrectAnswer = 2,
                    Explanation = "Use complex, unique passwords for each account and change them regularly. Consider a password manager."
                }
            };
        }
    }
}