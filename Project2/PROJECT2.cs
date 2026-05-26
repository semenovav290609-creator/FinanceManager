using System;
using System.Collections.Generic;

// 1. Базовый интерфейс
public interface ITextProcessor
{
    string Process(string input);
}

// 2. Реализация: Перевод в верхний регистр
public class UpperCaser : ITextProcessor
{
    public string Process(string input) => input?.ToUpper();
}

// 3. Реализация: Цензура
public class Censor : ITextProcessor
{
    private readonly List<string> _badWords;

    public Censor(List<string> badWords)
    {
        _badWords = badWords;
    }

    public string Process(string input) 
    {
        if (string.IsNullOrEmpty(input)) return input;

        string result = input;
        foreach (var word in _badWords)
        {
            // Используем OrdinalIgnoreCase, как ты и делал — это правильно
            result = result.Replace(word, "***", StringComparison.OrdinalIgnoreCase);
        }
        return result;
    }
}

// 4. Реализация: Обрезка строки
public class Trimmer : ITextProcessor
{
    private readonly int _maxLength;

    public Trimmer(int maxLength)
    {
        _maxLength = maxLength;
    }

    public string Process(string input)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= _maxLength) return input;
        return input.Substring(0, _maxLength) + "...";
    }
}

// 5. КОМПОЗИЦИЯ: Цепочка процессоров (Паттерн "Composite")
public class ChainProcessor : ITextProcessor
{
    private readonly List<ITextProcessor> _processors = new List<ITextProcessor>();

    public void AddProcessor(ITextProcessor processor)
    {
        _processors.Add(processor);
    }

    public string Process(string input)
    {
        string result = input;
        foreach (var processor in _processors)
        {
            result = processor.Process(result);
        }
        return result;
    }
}

// 6. ЭКСКЛЮЗИВ: Случайный выбор процессора
public class RandomProcessor : ITextProcessor
{
    private readonly ITextProcessor _first;
    private readonly ITextProcessor _second;
    private readonly Random _random = new Random();

    public RandomProcessor(ITextProcessor first, ITextProcessor second)
    {
        _first = first;
        _second = second;
    }

    public string Process(string input)
    {
        return _random.Next(2) == 0 ? _first.Process(input) : _second.Process(input);
    }
}

// ПРОВЕРКА
public class Program
{
    public static void Main()
    {
        // Создаем компоненты
        var censor = new Censor(new List<string> { "дурак", "тупой" });
        var upper = new UpperCaser();
        var trim = new Trimmer(20);

        // Создаем "Случайный" эффект (либо капс, либо ничего не делаем)
        // В качестве второго процессора передаем заглушку, которая просто возвращает текст
        var randomEffect = new RandomProcessor(upper, new IdentityProcessor());

        // Собираем Супер-Бота в цепочку
        var superBot = new ChainProcessor();
        superBot.AddProcessor(censor);       // 1. Убираем плохие слова
        superBot.AddProcessor(randomEffect); // 2. Либо КАПСИМ, либо нет (рандом)
        superBot.AddProcessor(trim);         // 3. Режем длину

        string text = "Этот дурак написал очень длинный и тупой текст для теста.";

        Console.WriteLine("Результат работы Супер-Бота:");
        Console.WriteLine(superBot.Process(text));
    }
}

// Вспомогательный класс: ничего не делает с текстом (паттерн Null Object)
public class IdentityProcessor : ITextProcessor
{
    public string Process(string input) => input;
}
