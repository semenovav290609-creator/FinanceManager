using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

// 1. Базовый класс транзакции
[JsonDerivedType(typeof(Income), "inc")]
[JsonDerivedType(typeof(Expense), "exp")]
abstract class Transaction(int sum, DateTime date, string category)
{
    public int Sum { get; } = sum;
    public DateTime Date { get; } = date;
    public string Category { get; } = category;

    public virtual void Print() =>
        Console.WriteLine($"{Date.ToShortDateString()} | {Category} | {Sum} руб.");
}

// 2. Классы-наследники
class Income(int sum, DateTime date, string category) : Transaction(sum, date, category)
{
    public override void Print() => Console.Write("[+] ");
}

class Expense(int sum, DateTime date, string category) : Transaction(sum, date, category)
{
    public override void Print() => Console.Write("[-] ");
}

// 3. Класс для управления финансами
class Wallet
{
    public List<Transaction> Transactions { get; set; } = new();

    // Тот самый метод баланса
    public int GetBalance()
    {
        var plus = Transactions.OfType<Income>().Sum(t => t.Sum);
        var minus = Transactions.OfType<Expense>().Sum(t => t.Sum);
        return plus - minus;
    }
}

class Program
{
    private const string FilePath = "wallet.json";

    static void Main()
    {
        Wallet myWallet = LoadData();

        while (true)
        {
            Console.WriteLine($"\nТЕКУЩИЙ БАЛАНС: {myWallet.GetBalance()} руб.");
            Console.WriteLine("1 - Доход, 2 - Расход, 3 - История, 4 - Выход");

            string choice = Console.ReadLine();
            if (choice == "4") break;

            if (choice == "1" || choice == "2")
            {
                Console.Write("Введите сумму: ");
                if (!int.TryParse(Console.ReadLine(), out int amount)) continue;

                Console.Write("Введите категорию: ");
                string cat = Console.ReadLine() ?? "Общее";

                if (choice == "1")
                {
                    myWallet.Transactions.Add(new Income(amount, DateTime.Now, cat));
                    Console.WriteLine("Доход добавлен!");
                }
                else
                {
                    // ПРОВЕРКА БАЛАНСА
                    if (myWallet.GetBalance() - amount < 0)
                    {
                        Console.WriteLine("⚠️ ВНИМАНИЕ: Баланс станет отрицательным!");
                    }

                    myWallet.Transactions.Add(new Expense(amount, DateTime.Now, cat));
                    Console.WriteLine("Расход учтен.");
                }
            }
            else if (choice == "3")
            {
                Console.WriteLine("\n--- ИСТОРИЯ ТРАТ ---");
                foreach (var t in myWallet.Transactions.OrderByDescending(x => x.Date))
                {
                    t.Print();
                    Console.WriteLine($"{t.Category}: {t.Sum} руб.");
                }
            }
        }

        SaveData(myWallet);
    }

    // Сохранение и загрузка
    static void SaveData(Wallet wallet)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(FilePath, JsonSerializer.Serialize(wallet, options));
    }

    static Wallet LoadData()
    {
        if (!File.Exists(FilePath)) return new Wallet();
        try
        {
            return JsonSerializer.Deserialize<Wallet>(File.ReadAllText(FilePath)) ?? new Wallet();
        }
        catch { return new Wallet(); }
    }
}
