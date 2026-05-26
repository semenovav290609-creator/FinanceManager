using System;
using System.Collections.Generic;

// Сокращение №1: Records. Свойства Model, Price и HorsePower создаются автоматически
public record Car(string Model, int Price, int HorsePower)
{
    public int CalculationOfAnnualTax() => HorsePower > 250 ? HorsePower * 150 : HorsePower * 75;
}

public record User(string Name, double MonthlySalary)
{
    public int CanAfford(Car car)
    {
        double annualSavings = (MonthlySalary * 0.5) * 12;
        double currentPrice = car.Price;
        int years = 0;

        while (currentPrice > 0 && years < 40)
        {
            years++;
            currentPrice = (currentPrice * 1.10) - annualSavings;
        }
        return years;
    }
}

public class Program
{
    public static void Main()
    {
        Console.InputEncoding = Console.OutputEncoding = System.Text.Encoding.Unicode;

        // Сокращение №2: Не пишем new Car, просто new()
        var cars = new List<Car>
        {
            new("E63 AMG", 8_000_000, 612),
            new("C63 AMG", 5_000_000, 476),
            new("A45 AMG", 3_500_000, 381)
        };

        Console.WriteLine("Введите через пробел ваше имя и зарплату в месяц: ");
        var parts = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts?.Length >= 2 && double.TryParse(parts[1], out var salary))
        {
            var user = new User(parts[0], salary);
            Console.WriteLine($"\nПривет, {user.Name}! Расчет:");

            foreach (var c in cars)
            {
                var years = user.CanAfford(c);
                Console.WriteLine($"- {c.Model} ({c.Price:N0}₽): {years} лет. Налог: {c.CalculationOfAnnualTax():N0}₽");

                // Сокращение №3: Короткое условие
                Console.WriteLine(years >= 40 ? " (!) Нереально" : years > 10 ? " (!) Долго" : " (+) Можно брать!");
                Console.WriteLine();
            }
        }
    }
}
