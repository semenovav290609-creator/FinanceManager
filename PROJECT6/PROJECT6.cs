using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Encodings.Web;

public class Transaction
{
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public DateTime Date { get; set; }

    public Transaction(string name, decimal amount, string category, DateTime date)
    {
        Name = name;
        Amount = amount;
        Category = category;
        Date = date;
    }
}

public class FinanceManager
{
    private List<Transaction> _transactions = new List<Transaction>();
    private List<Transaction> _deletedTransactions = new List<Transaction>();

    private const string FileName = "finance.json";
    private const string DeletedFileName = "deleted_finance.json";

    public async Task AddTransactionAsync(Transaction t)
    {
        _transactions.Add(t);
        await SaveToFileAsync();
    }

    public decimal GetTotalSpend() => _transactions.Sum(x => x.Amount);

    public List<Transaction> GetAllTransactions() => _transactions;

    public List<Transaction> GetTransactionsByCategory(string category) =>
        _transactions.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

    public List<Transaction> GetTransactionsByDateRange(DateTime start, DateTime end)
         => _transactions.Where(t => t.Date >= start && t.Date <= end).ToList();

    public async Task SaveToFileAsync()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        string json = JsonSerializer.Serialize(_transactions, options);
        await File.WriteAllTextAsync(FileName, json);
    }

    public async Task SaveDeletedToFileAsync()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        string json = JsonSerializer.Serialize(_deletedTransactions, options);
        await File.WriteAllTextAsync(DeletedFileName, json);
    }

    public async Task LoadFromFileAsync()
    {
        try
        {
            if (File.Exists(FileName))
            {
                string json = await File.ReadAllTextAsync(FileName);
                _transactions = JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();
            }

            if (File.Exists(DeletedFileName))
            {
                string jsonDeleted = await File.ReadAllTextAsync(DeletedFileName);
                _deletedTransactions = JsonSerializer.Deserialize<List<Transaction>>(jsonDeleted) ?? new List<Transaction>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Ошибка при чтении файлов: {ex.Message}");
            _transactions = new List<Transaction>();
            _deletedTransactions = new List<Transaction>();
        }
    }

    public async Task<bool> RemoveTransactionByNameAsync(string name)
    {
        var toRemove = _transactions.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (toRemove != null)
        {
            _transactions.Remove(toRemove);
            _deletedTransactions.Add(toRemove);

            await SaveToFileAsync();
            await SaveDeletedToFileAsync();
            return true;
        }
        return false;
    }

    public async Task ClearAllTransactionsAsync()
    {
        _deletedTransactions.AddRange(_transactions);
        _transactions.Clear();

        await SaveToFileAsync();
        await SaveDeletedToFileAsync();
    }

    public void OpenDelTransaction()
    {
        if (!_deletedTransactions.Any())
        {
            Console.WriteLine("Корзина пуста.");
            return;
        }

        for (int i = 0; i < _deletedTransactions.Count; i++)
        {
            var t = _deletedTransactions[i];
            string rubFormat = t.Amount.ToString("N2", CultureInfo.InvariantCulture) + " ₽.";
            int displayIndex = i + 1;
            Console.WriteLine($"{displayIndex}. {t.Date.ToShortDateString()} | {t.Name,-25} | {rubFormat,16} | [{t.Category}]");
        }
    }

    public async Task ClearDeletedTransactionsAsync()
    {
        _deletedTransactions.Clear();
        await SaveDeletedToFileAsync();
        Console.WriteLine("🧹 Корзина полностью очищена на диске!");
    }

    public async Task<bool> RestoreTransactionAsync(int index)
    {
        int realIndex = index - 1;

        if (realIndex >= 0 && realIndex < _deletedTransactions.Count)
        {
            var target = _deletedTransactions[realIndex];

            _deletedTransactions.RemoveAt(realIndex);
            _transactions.Add(target);

            await SaveToFileAsync();
            await SaveDeletedToFileAsync();
            return true;
        }
        return false;
    }
}

class Program
{
    public static async Task Main(string[] args)
    {
        Console.InputEncoding = Console.OutputEncoding = System.Text.Encoding.Unicode;

        FinanceManager manager = new FinanceManager();
        await manager.LoadFromFileAsync();

        decimal dollarCourse = 72m;

        while (true)
        {
            Console.WriteLine("\n╔══════════════════════ МЕНЕДЖЕР ФИНАНСОВ ══════════════════════╗");
            Console.WriteLine("║  [1] Добавить          [5] Посмотреть все     [9] Открыть корзину  ║");
            Console.WriteLine("║  [2] Итог (Руб/$)      [6] Удалить одну       [10] Очистить корзину║");
            Console.WriteLine("║  [3] Найти категории   [7] Найти по датам     [11] Восстановить    ║");
            Console.WriteLine("║  [4] Выйти из таблицы  [8] Удалить ВСЁ                             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
            Console.Write("  Ваш выбор: ");



            if (!int.TryParse(Console.ReadLine(), out var result))
            {
                Console.WriteLine("❌ Ошибка: Введите число.");
                continue;
            }

            switch (result)
            {
                case 1:
                    Console.WriteLine("Введите через запятую: Название, Цена, Категория");
                    string rawInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(rawInput)) break;

                    string[] parts = rawInput.Split(',');

                    if (parts.Length == 3)
                    {
                        string nameParam = parts[0].Trim();
                        string priceStr = parts[1].Trim();
                        string categoryParam = parts[2].Trim();

                        if (decimal.TryParse(priceStr, out decimal price))
                        {
                            Transaction newTrans = new Transaction(nameParam, price, categoryParam, DateTime.Now);
                            await manager.AddTransactionAsync(newTrans);
                            Console.WriteLine("✅ Добавлено и сохранено!");
                        }
                        else Console.WriteLine("❌ Ошибка: неверный формат цены.");
                    }
                    else Console.WriteLine("❌ Ошибка: введите 3 значения через запятую.");
                    break;

                case 2:
                    decimal totalRub = manager.GetTotalSpend();
                    decimal totalUsd = totalRub / dollarCourse;

                    string rubString = totalRub.ToString("N2", CultureInfo.InvariantCulture);
                    string usdString = totalUsd.ToString("N2", CultureInfo.InvariantCulture);

                    Console.WriteLine($"💰 Итого потрачено: {rubString} руб. (${usdString})");
                    break;

                case 3:
                    Console.WriteLine("Введите название категории:");
                    string category = Console.ReadLine().Trim();
                    var foundByCategory = manager.GetTransactionsByCategory(category);

                    if (foundByCategory.Any())
                    {
                        foreach (var t in foundByCategory)
                        {
                            string amtString = t.Amount.ToString("N2", CultureInfo.InvariantCulture);
                            Console.WriteLine($"- {t.Name,-25} | {amtString,13} руб. [{t.Category}]");
                        }
                    }
                    else Console.WriteLine("Транзакции не найдены.");
                    break;

                case 4:
                    Console.WriteLine("Выход из программы...");
                    return;

                case 5:
                    var all = manager.GetAllTransactions();
                    if (all.Any())
                    {
                        foreach (var t in all)
                        {
                            string rubFormat = t.Amount.ToString("N2", CultureInfo.InvariantCulture) + " ₽.";
                            string usdFormat = $"(${(t.Amount / dollarCourse).ToString("N2", CultureInfo.InvariantCulture)})";

                            Console.WriteLine($"{t.Date.ToShortDateString()} | {t.Name,-25} | {rubFormat,16} | {usdFormat,-13} | {t.Category}");
                        }
                    }
                    else Console.WriteLine("Список пуст.");
                    break;

                case 6:
                    if (!manager.GetAllTransactions().Any())
                    {
                        Console.WriteLine("❌ Список транзакций пуст.");
                    }
                    else
                    {
                        Console.WriteLine("Введите название транзакции для удаления:");
                        string inputTransaction = Console.ReadLine().Trim();

                        if (await manager.RemoveTransactionByNameAsync(inputTransaction))
                            Console.WriteLine("✅ Транзакция успешно удалена!");
                        else
                            Console.WriteLine("❌ Транзакция с таким названием не найдена.");
                    }
                    break;

                case 7:
                    Console.WriteLine("Введите даты через запятую в формате: день.месяц.год, день.месяц.год");
                    string dateInput = Console.ReadLine().Trim();
                    string[] dateStrings = dateInput.Split(',');

                    if (dateStrings.Length == 2)
                    {
                        string firstDateStr = dateStrings[0].Trim();
                        string secondDateStr = dateStrings[1].Trim();

                        bool isFirstValid = DateTime.TryParse(firstDateStr, out DateTime startDate);
                        bool isSecondValid = DateTime.TryParse(secondDateStr, out DateTime endDate);

                        if (isFirstValid && isSecondValid)
                        {
                            endDate = endDate.Date.AddDays(1).AddTicks(-1);
                            var foundByDate = manager.GetTransactionsByDateRange(startDate, endDate);

                            if (foundByDate.Any())
                            {
                                Console.WriteLine($"\n--- Транзакции за период с {startDate.ToShortDateString()} по {endDate.ToShortDateString()} ---");
                                foreach (var t in foundByDate)
                                {
                                    string rubFormat = t.Amount.ToString("N2", CultureInfo.InvariantCulture) + " ₽.";
                                    string usdFormat = $"(${(t.Amount / dollarCourse).ToString("N2", CultureInfo.InvariantCulture)})";

                                    Console.WriteLine($"{t.Date.ToShortDateString()} | {t.Name,-25} | {rubFormat,16} | {usdFormat,-13} | {t.Category}");
                                }
                            }
                            else Console.WriteLine("За указанный период транзакций не найдено.");
                        }
                        else Console.WriteLine("❌ Ошибка: неверный формат одной или обеих дат.");
                    }
                    else Console.WriteLine("❌ Ошибка: пожалуйста, введите ровно две даты через запятую.");
                    break;

                case 8:
                    Console.WriteLine("Вы уверены, что хотите удалить ВСЕ транзакции? (да/нет)");
                    if (Console.ReadLine().Trim().Equals("да", StringComparison.OrdinalIgnoreCase))
                    {
                        await manager.ClearAllTransactionsAsync();
                        Console.WriteLine("🗑️ Все транзакции успешно удалены!");
                    }
                    else Console.WriteLine("Отменено.");
                    break;

                case 9:
                    Console.WriteLine("-----Ваши удалённые транзакции-----");
                    manager.OpenDelTransaction();
                    break;

                case 10:
                    Console.WriteLine("⚠️ Вы уверены, что хотите НАВСЕГДА удалить все транзакции из корзины? (да/нет)");
                    if (Console.ReadLine().Trim().Equals("да", StringComparison.OrdinalIgnoreCase))
                    {
                        await manager.ClearDeletedTransactionsAsync();
                    }
                    else Console.WriteLine("Очистка корзины отменена.");
                    break;

                case 11:
                    Console.WriteLine("----- Восстановление транзакции -----");
                    manager.OpenDelTransaction();

                    Console.WriteLine("Введите номер транзакции, которую хотите восстановить:");
                    if (int.TryParse(Console.ReadLine(), out int restoreIndex))
                    {
                        if (await manager.RestoreTransactionAsync(restoreIndex))
                        {
                            Console.WriteLine("✅ Транзакция успешно восстановлена в основной список!");
                        }
                        else Console.WriteLine("❌ Ошибка: Транзакции с таким номером нет в корзине.");
                    }
                    else Console.WriteLine("❌ Ошибка: Введите корректное число.");
                    break;

                default:
                    Console.WriteLine("❌ Неверный пункт меню.");
                    break;
            }
            Console.WriteLine();
        }
    }
}
