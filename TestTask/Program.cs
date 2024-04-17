using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestTask
{
    public class Program
    {

        /// <summary>
        /// Программа принимает на входе 2 пути до файлов.
        /// Анализирует в первом файле кол-во вхождений каждой буквы (регистрозависимо). Например А, б, Б, Г и т.д.
        /// Анализирует во втором файле кол-во вхождений парных букв (не регистрозависимо). Например АА, Оо, еЕ, тт и т.д.
        /// По окончанию работы - выводит данную статистику на экран.
        /// </summary>
        /// <param name="args">Первый параметр - путь до первого файла.
        /// Второй параметр - путь до второго файла.</param>
        static void Main(string[] args)
        {
            if (args.Length <= 0 || args == null)
            {
                Console.WriteLine("Неверный путь. Укажите другой");
                return;
            }

            IReadOnlyStream inputStream1 = GetInputStream(args[0]);
            IReadOnlyStream inputStream2 = GetInputStream(args[1]);

            IList<LetterStats> singleLetterStats = FillSingleLetterStats(inputStream1);
            IList<LetterStats> doubleLetterStats = FillDoubleLetterStats(inputStream2);

            RemoveCharStatsByType(singleLetterStats, CharType.Vowel);
            RemoveCharStatsByType(doubleLetterStats, CharType.Consonants);

            PrintStatistic(singleLetterStats);
            PrintStatistic(doubleLetterStats);

            Console.ReadLine();
        }

        /// <summary>
        /// Ф-ция возвращает экземпляр потока с уже загруженным файлом для последующего посимвольного чтения.
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        /// <returns>Поток для последующего чтения.</returns>
        private static IReadOnlyStream GetInputStream(string fileFullPath)
        {
            return new ReadOnlyStream(fileFullPath);
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения каждой буквы.
        /// Статистика РЕГИСТРОЗАВИСИМАЯ!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillSingleLetterStats(IReadOnlyStream stream)
        {
            var statisticsDict = new Dictionary<char, LetterStats>();
            try
            {
                stream.ResetPositionToStart();
                while (!stream.IsEof)
                {
                    char c = stream.ReadNextChar();
                    if (char.IsLetter(c))
                    {
                        if (!statisticsDict.ContainsKey(c))
                        {   
                            statisticsDict[c] = new LetterStats { Letter = c.ToString(), Count = 0 };
                        }
                        IncStatistic(statisticsDict[c]);
                    }
                }
            }
            catch (EndOfStreamException)
            {
                throw new EndOfStreamException("Конец файла...");
            }

            return statisticsDict.Values.ToList();
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения парных букв.
        /// В статистику должны попадать только пары из одинаковых букв, например АА, СС, УУ, ЕЕ и т.д.
        /// Статистика - НЕ регистрозависимая!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillDoubleLetterStats(IReadOnlyStream stream)
        {
            var statisticsDict = new Dictionary<string, LetterStats>();
            char? lastSymbol = null;
            try
            {
                stream.ResetPositionToStart();
                while (!stream.IsEof)
                {
                    char c = char.ToLower(stream.ReadNextChar());
                    if (char.IsLetter(c))
                    {
                        if (lastSymbol.HasValue && lastSymbol == c)
                        {
                            string charsPairs = new string(new[] { c, c });
                            if (!statisticsDict.ContainsKey(charsPairs))
                            {
                                statisticsDict[charsPairs] = new LetterStats { Letter = c.ToString(), Count = 0 };
                            }
                            IncStatistic(statisticsDict[charsPairs]);    
                        }
                        lastSymbol = c;
                    }
                }
            }
            catch (EndOfStreamException)
            {
                throw new EndOfStreamException("Конец файла...");
            }

            return statisticsDict.Values.ToList();
        }

        /// <summary>
        /// Ф-ция перебирает все найденные буквы/парные буквы, содержащие в себе только гласные или согласные буквы.
        /// (Тип букв для перебора определяется параметром charType)
        /// Все найденные буквы/пары соответствующие параметру поиска - удаляются из переданной коллекции статистик.
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="charType">Тип букв для анализа</param>
        private static void RemoveCharStatsByType(IList<LetterStats> letters, CharType charType)
        {
            HashSet<char> charSet = new HashSet<char>();
            switch (charType)
            {
                case CharType.Consonants:
                    charSet.UnionWith("aeiouAEIOU".ToCharArray());
                    charSet.UnionWith("bcdfghjklmnpqrstvwxyzBCDFGHJKLMNPQRSTVWXYZ".ToCharArray());
                    break;
                case CharType.Vowel:
                    charSet.UnionWith("aeiouAEIOU".ToCharArray());
                    break;
            }

            for (int i = letters.Count - 1; i >= 0; i--)
            {
                if (letters[i].Letter.All(c => charSet.Contains(c)))
                {
                    letters.RemoveAt(i);
                }
            }
            
        }

        /// <summary>
        /// Ф-ция выводит на экран полученную статистику в формате "{Буква} : {Кол-во}"
        /// Каждая буква - с новой строки.
        /// Выводить на экран необходимо предварительно отсортировав набор по алфавиту.
        /// В конце отдельная строчка с ИТОГО, содержащая в себе общее кол-во найденных букв/пар
        /// </summary>
        /// <param name="letters">Коллекция со статистикой</param>
        private static void PrintStatistic(IEnumerable<LetterStats> letters)
        {
            var sortedLetters = letters.OrderBy(x => x.Letter);

            foreach (var letter in sortedLetters)
            {
                Console.WriteLine($"{letter.Letter} - {letter.Count}");
            }
            Console.WriteLine($"Итого: {sortedLetters.Sum(x => x.Count)}");
        }

        /// <summary>
        /// Метод увеличивает счётчик вхождений по переданной структуре.
        /// </summary>
        /// <param name="letterStats"></param>
        private static void IncStatistic(LetterStats letterStats)
        {
            letterStats.Count++;
        }
    }
}
