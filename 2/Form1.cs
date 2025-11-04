using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace _2
{
    /// Главная форма приложения для поиска подстрок с использованием конечного автомата
    public partial class Form1 : Form
    {
        private FiniteStateAutomaton automaton; // Экземпляр конечного автомата

        public Form1()
        {
            InitializeComponent();
            automaton = new FiniteStateAutomaton(); // Инициализация автомата
        }

        // Обработчик события поиска подстроки в тексте
        private void searchButton_Click(object sender, EventArgs e)
        {
            string text = txt_in.Text;       // Исходный текст для поиска
            string pattern = patternBox.Text; // Подстрока (шаблон) для поиска

            // Проверка ввода подстроки
            if (string.IsNullOrEmpty(pattern))
            {
                resultBox.Text = "Введите подстроку для поиска!";
                return;
            }

            // Построение автомата для заданного шаблона и поиск вхождений
            automaton.BuildAutomaton(pattern);
            List<int> positions = automaton.Search(text);

            // Формирование результата поиска
            resultBox.Text = $"Поиск подстроки: '{pattern}' в тексте: '{text}'\r\n";
            resultBox.Text += $"Длина подстроки: {pattern.Length}\r\n";
            resultBox.Text += $"Количество состояний автомата: {pattern.Length + 1}\r\n\r\n";

            // Вывод результатов поиска
            if (positions.Count > 0)
            {
                resultBox.Text += $"Найдено в позициях: {string.Join(", ", positions)}\r\n";
                resultBox.Text += $"Всего вхождений: {positions.Count}";
            }
            else
            {
                resultBox.Text += "Подстрока не найдена";
            }
        }

        // Обработчик события для отображения таблицы переходов автомата
        private void showTableButton_Click(object sender, EventArgs e)
        {
            string pattern = patternBox.Text; // Подстрока для построения таблицы

            // Проверка ввода подстроки
            if (string.IsNullOrEmpty(pattern))
            {
                resultBox.Text = "Введите подстроку для построения таблицы!";
                return;
            }

            // Построение автомата и вывод таблицы переходов
            automaton.BuildAutomaton(pattern);
            resultBox.Text = automaton.AnalyzeAutomaton();
        }
    }

    /// Класс, реализующий конечный автомат для поиска подстрок
    /// Использует алгоритм построения таблицы переходов для эффективного поиска
    public class FiniteStateAutomaton
    {
        private int[,] transitionTable; // Таблица переходов между состояниями
        private string pattern;         // Шаблон для поиска
        private int patternLength;      // Длина шаблона

        /// Построение автомата для заданного шаблона
        /// Создает таблицу переходов размером (patternLength+1) x 256

        public void BuildAutomaton(string pattern)
        {
            this.pattern = pattern;
            this.patternLength = pattern.Length;

            int totalStates = patternLength + 1; // Количество состояний (включая начальное и конечное)
            transitionTable = new int[totalStates, 256]; // Таблица для ASCII символов

            // Заполнение таблицы переходов
            for (int state = 0; state < totalStates; state++)
            {
                for (int ch = 0; ch < 256; ch++)
                {
                    if (state < patternLength && (char)ch == pattern[state])
                    {
                        // Переход к следующему состоянию при совпадении символа
                        transitionTable[state, ch] = state + 1;
                    }
                    else
                    {
                        // Поиск наиболее длинного префикса, который является суффиксом
                        string current = pattern.Substring(0, state) + (char)ch;
                        transitionTable[state, ch] = FindLongestPrefixSuffix(current, pattern);
                    }
                }
            }
        }

        /// Поиск длины наибольшего префикса шаблона, который является суффиксом текущей строки
        /// </summary>
        /// <param name="text">Текущая строка</param>
        /// <param name="pattern">Шаблон поиска</param>
        /// <returns>Длина наибольшего совпадения</returns>
        private int FindLongestPrefixSuffix(string text, string pattern)
        {
            int maxLength = Math.Min(text.Length, pattern.Length);

            // Поиск от максимальной возможной длины к минимальной
            for (int length = maxLength; length > 0; length--)
            {
                if (text.EndsWith(pattern.Substring(0, length)))
                {
                    return length;
                }
            }

            return 0; // Нет совпадений - возврат в начальное состояние
        }

        /// Поиск всех вхождений шаблона в тексте с использованием построенного автомата
        /// <param name="text">Текст для поиска</param>
        /// <returns>Список позиций начала вхождений</returns>
        public List<int> Search(string text)
        {
            List<int> positions = new List<int>(); // Список найденных позиций
            int currentState = 0;                  // Текущее состояние автомата

            // Посимвольный проход по тексту
            for (int i = 0; i < text.Length; i++)
            {
                // Переход в следующее состояние по текущему символу
                currentState = transitionTable[currentState, text[i]];

                // Если достигнуто конечное состояние - найдено вхождение
                if (currentState == patternLength)
                {
                    positions.Add(i - patternLength + 1); // Добавление позиции начала вхождения
                }
            }

            return positions;
        }

        /// Анализ автомата и построение текстового представления таблицы переходов
        /// <returns>Строка с подробным анализом автомата</returns>
        public string AnalyzeAutomaton()
        {
            string analysis = "ТАБЛИЦА ПЕРЕХОДОВ ДЛЯ ПОДСТРОКИ: " + pattern + "\r\n\r\n";
            analysis += $"Количество состояний: {patternLength + 1}\r\n";
            analysis += "Конечное состояние: q" + patternLength + "\r\n\r\n";

            // Создаем таблицу с правильными отступами
            analysis += "Состояние | ";

            // Заголовки для символов из шаблона
            foreach (char ch in pattern)
            {
                analysis += $"{ch} | ";
            }
            analysis += "другие\r\n";

            // Разделительная линия
            analysis += new string('-', 12 + pattern.Length * 5) + "\r\n";

            // Данные таблицы переходов
            for (int state = 0; state <= patternLength; state++)
            {
                analysis += $"q{state}".PadRight(9) + " | ";

                // Переходы для символов из шаблона
                foreach (char ch in pattern)
                {
                    int nextState = transitionTable[state, ch];
                    analysis += $"q{nextState}".PadRight(2) + " | ";
                }

                // Переход для других символов (в качестве примера используется 'x')
                int otherState = transitionTable[state, 'x'];
                analysis += $"q{otherState}";

                analysis += "\r\n";
            }

            // Добавление пояснений
            analysis += "\r\n" + new string('=', 60) + "\r\n\r\n";
            analysis += "ОБЪЯСНЕНИЕ ПЕРЕХОДОВ:\r\n";
            analysis += "• q₀ - начальное состояние\r\n";
            analysis += "• При правильном символе - переход к следующему состоянию\r\n";
            analysis += "• При неправильном символе - переход к состоянию, соответствующему\r\n";
            analysis += "  наибольшему префиксу, который является суффиксом текущей строки\r\n\r\n";

            return analysis;
        }
    }
}