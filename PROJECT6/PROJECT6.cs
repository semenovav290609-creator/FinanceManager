using System;
using System.Collections.Generic;
using System.Linq;
using System.IO; // Для работы с файлами
using System.Text.Json; // Для работы с JSON

public class Transaction(string name, decimal amount, string category, DateTime date)
{
    public string Name { get; set; } = name;
    public decimal Amount { get; set; } = amount;
    public string Category { get; set; } = category;
    public DateTime Date { get; set; } = date;
}

public class FinanceManager
{
    private List<Transaction> _transactions = new List<Transaction>();
    private const string FileName = "finance.json";

    public void AddTransaction(Transaction t)
    {
        _transactions.Add(t);
        SaveToFile(); // Сохраняем сразу после добавления
    }

    public decimal GetTotalSpend() => _transactions.Sum(x => x.Amount);

    public List<Transaction> GetAllTransactions() => _transactions;

    public List<Transaction> GetTransactionsByCategory(string category) =>
        _transactions.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

    public List<Transaction> GetTransactionsByDateRange(DateTime start, DateTime end)
         => _transactions.Where(t => t.Date >= start && t.Date <= end).ToList();

    // Сохранение в JSON
    public void SaveToFile()
    {
        string json = JsonSerializer.Serialize(_transactions, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FileName, json);
    }

    // Загрузка из JSON
    public void LoadFromFile()
    {
        try
        {
            if (File.Exists(FileName))
            {
                string json = File.ReadAllText(FileName);
                _transactions = JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Ошибка при чтении файла: {ex.Message}");
            _transactions = new List<Transaction>(); // Создаем пустой список, если файл битый
        }
    }


    public bool RemoveTransactionByName(string name)
    {
        // Ищем транзакцию (игнорируя регистр)
        var toRemove = _transactions.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (toRemove != null)
        {
            _transactions.Remove(toRemove);
            SaveToFile(); // Перезаписываем файл после удаления
            return true;
        }
        return false;
    }
}

class Program
{
    public static void Main(string[] args)
    {
        FinanceManager manager = new FinanceManager();
        manager.LoadFromFile();

        while (true)
        {
            Console.WriteLine("\n--- МЕНЮ ---");
            Console.WriteLine("1 - Добавить транзакцию");
            Console.WriteLine("2 - Узнать итоговую сумму");
            Console.WriteLine("3 - Найти по категории");
            Console.WriteLine("4 - Выйти");
            Console.WriteLine("5 - Посмотреть все транзакции");
            Console.WriteLine("6 - Удалить транзакцию");
            Console.WriteLine("7 - Открыть транзакции за определённые даты");

            if (!int.TryParse(Console.ReadLine(), out var result))
            {
                Console.WriteLine("❌ Ошибка: Введите число.");
                continue;
            }

            switch (result)
            {
                case 1:
                    Console.WriteLine("Введите через запятую: Название, Цена, Категория");
                    string[] input = Console.ReadLine().Split(",");

                    if (input.Length == 3 && decimal.TryParse(input[1].Trim(), out decimal price))
                    {
                        manager.AddTransaction(new Transaction(input[0].Trim(), price, input[2].Trim(), DateTime.Now));
                        Console.WriteLine("✅ Добавлено и сохранено!");
                    }
                    else Console.WriteLine("❌ Ошибка: неверный формат ввода.");
                    break;

                case 2:
                    Console.WriteLine($"💰 Итого потрачено: {manager.GetTotalSpend()} руб");
                    break;

                case 3:
                    Console.WriteLine("Введите название категории:");
                    string category = Console.ReadLine();
                    var foundByCategory = manager.GetTransactionsByCategory(category);

                    if (foundByCategory.Any())
                    {
                        foreach (var t in foundByCategory)
                            Console.WriteLine($"- {t.Name}: {t.Amount} руб. [{t.Category}]");
                    }
                    else Console.WriteLine("Транзакции не найдены.");
                    break;

                case 4:
                    Console.WriteLine("Выход из программы...");
                    return; // Полностью останавливает метод Main и закрывает программу

                case 5:
                    var all = manager.GetAllTransactions();
                    if (all.Any())
                    {
                        foreach (var t in all)
                            Console.WriteLine($"{t.Date.ToShortDateString()} | {t.Name,-25} | {t.Amount,8} руб. | {t.Category}");
                    }
                    else Console.WriteLine("Список пуст.");
                    break;

                case 6:
                    Console.WriteLine("Введите название транзакции для удаления:");
                    string inputTransaction = Console.ReadLine();

                    if (manager.RemoveTransactionByName(inputTransaction))
                        Console.WriteLine("✅ Транзакция успешно удалена!");
                    else
                        Console.WriteLine("❌ Транзакция с таким названием не найдена.");
                    break;

                case 7:
                    Console.WriteLine("Введите даты через запятую в формате: день.месяц.год, день.месяц.год");
                    string dateInput = Console.ReadLine();
                    string[] dateStrings = dateInput.Split(',');

                    if (dateStrings.Length == 2)
                    {
                        bool isFirstValid = DateTime.TryParse(dateStrings[0].Trim(), out DateTime startDate);
                        bool isSecondValid = DateTime.TryParse(dateStrings[1].Trim(), out DateTime endDate);

                        if (isFirstValid && isSecondValid)
                        {
                            endDate = endDate.Date.AddDays(1).AddTicks(-1);
                            var foundByDate = manager.GetTransactionsByDateRange(startDate, endDate);

                            if (foundByDate.Any())
                            {
                                Console.WriteLine($"\n--- Транзакции за период с {startDate.ToShortDateString()} по {endDate.ToShortDateString()} ---");
                                foreach (var t in foundByDate)
                                    Console.WriteLine($"{t.Date.ToShortDateString()} | {t.Name,-25} | {t.Amount,8} руб. | {t.Category}");
                            }
                            else Console.WriteLine("За указанный период транзакций не найдено.");
                        }
                        else Console.WriteLine("❌ Ошибка: неверный формат одной или обеих дат.");
                    }
                    else Console.WriteLine("❌ Ошибка: пожалуйста, введите ровно две даты через запятую.");
                    break;

                default:
                    Console.WriteLine("❌ Неверный пункт меню.");
                    break;
            }
            Console.WriteLine();
        }
    }
}
