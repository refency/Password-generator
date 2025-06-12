using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Generated_passwords
{
    public partial class Form1 : Form
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IPasswordGenerator _passwordGenerator;

        public Form1()
        {
            InitializeComponent();

            // Создаем зависимости
            _settingsManager = new SettingsManager();
            _passwordGenerator = new PasswordGenerator();

            auto_loaded_options();
        }

        private void ShowNotification(string message)
        {
            NotificationForm notification = new NotificationForm(message);
            notification.Show();
        }

        private void auto_loaded_options()
        {
            _settingsManager.ReadSettings();

            richTextBox1.Text = _settingsManager.Alphabet;
            numericUpDown1.Value = _settingsManager.Length;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (richTextBox2.Text.Trim() != "")
            {
                Clipboard.SetText(richTextBox2.Text);
                ShowNotification("Пароль скопирован");
            }
            else
            {
                ShowNotification("Ошибка при копировании");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string password = _passwordGenerator.GeneratePassword(
                Decimal.ToInt32(numericUpDown1.Value),
                richTextBox1.Text
            );

            richTextBox2.Text = password;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _settingsManager.Alphabet = richTextBox1.Text;
            _settingsManager.Length = Decimal.ToInt32(numericUpDown1.Value);
            _settingsManager.SaveSettings();

            ShowNotification("Настройки сохранены");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            File.Delete("configure.bin");

            _settingsManager.ReadSettings();
            auto_loaded_options();
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {
            label3.Text = PasswordStrengthChecker.EvaluatePasswordStrength(richTextBox2.Text);
        }

        #region Interfaces

        public interface ISettingsManager
        {
            string Alphabet { get; set; }
            int Length { get; set; }
            void ReadSettings();
            void SaveSettings();
        }

        public interface IPasswordGenerator
        {
            string GeneratePassword(int length, string alphabet);
        }

        #endregion

        #region Classes

        public class SettingsManager : ISettingsManager
        {
            private string alphabet;
            private int length;

            public SettingsManager()
            {
                this.alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890@#%\"&*()_-+={}<>?:[].~";
                this.length = 12;
            }

            public string Alphabet
            {
                get => alphabet;
                set => alphabet = value;
            }

            public int Length
            {
                get => length;
                set => length = value;
            }

            public void ReadSettings()
            {
                if (!File.Exists("configure.bin")) return;

                using (BinaryReader reader = new BinaryReader(File.Open("configure.bin", FileMode.Open)))
                {
                    this.alphabet = reader.ReadString();
                    this.length = reader.ReadInt32();
                }
            }

            public void SaveSettings()
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open("configure.bin", FileMode.OpenOrCreate)))
                {
                    writer.Write(string.IsNullOrEmpty(this.alphabet) ? " " : this.alphabet);
                    writer.Write(this.length);
                }
            }
        }

        public class PasswordGenerator : IPasswordGenerator
        {
            public string GeneratePassword(int length, string alphabet)
            {
                if (length <= 0)
                    throw new ArgumentException("Длина пароля должна быть положительной.");
                if (string.IsNullOrEmpty(alphabet))
                    throw new ArgumentException("Алфавит не должен быть пустым.");

                StringBuilder password = new StringBuilder(length);
                Random rnd = new Random();

                for (int i = 0; i < length; i++)
                {
                    int index = rnd.Next(alphabet.Length);
                    password.Append(alphabet[index]);
                }

                return password.ToString();
            }
        }

        public static class PasswordStrengthChecker
        {
            public static string EvaluatePasswordStrength(string password)
            {
                int score = 0;

                // 1. Длина пароля
                if (password.Length >= 12)
                    score += 1;

                // 2. Наличие заглавных букв
                if (Regex.IsMatch(password, "[A-Z]"))
                    score += 1;

                // 3. Наличие строчных букв
                if (Regex.IsMatch(password, "[a-z]"))
                    score += 1;

                // 4. Наличие цифр
                if (Regex.IsMatch(password, "[0-9]"))
                    score += 1;

                // 5. Наличие специальных символов
                if (Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]"))
                    score += 1;

                // 6. Отсутствие повторяющихся символов
                if (!Regex.IsMatch(password, @"(.)\1"))
                    score += 1;

                // Определение уровня надёжности на основе баллов
                if (score <= 2) return "Слабый";
                if (score <= 4) return "Средний";
                if (score <= 6) return "Хороший";
                return "Отличный";
            }
        }

        public class NotificationForm : Form
        {
            private System.Windows.Forms.Timer timer;

            public NotificationForm(string message)
            {
                // Настройка формы
                this.Text = "Уведомление";
                this.FormBorderStyle = FormBorderStyle.None;
                this.Size = new Size(200, 50);
                this.StartPosition = FormStartPosition.CenterScreen;

                // Добавляем текстовое поле
                Label label = new Label();
                label.Text = message;
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.Font = new Font("Arial", 12);
                this.Controls.Add(label);

                // Таймер для автоматического закрытия
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 1000; // 1 секунда
                timer.Tick += Timer_Tick;
                timer.Start();
            }

            private void Timer_Tick(object sender, EventArgs e)
            {
                timer.Stop(); // Останавливаем таймер
                this.Close(); // Закрываем форму
            }
        }

        #endregion
    }
}