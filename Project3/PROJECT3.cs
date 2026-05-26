using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization; // Нужно для атрибутов

// 1. Интерфейс
interface IElectric
{
    void Charge();
}

// 2. Базовый класс с атрибутами для правильного сохранения в JSON
[JsonDerivedType(typeof(Car), "car")]
[JsonDerivedType(typeof(Motorcycle), "moto")]
abstract class Vehicle(string mark, string model, int year)
{
    public string Mark { get; } = mark;
    public string Model { get; } = model;

    private int _yearOfRelease = year > DateTime.Now.Year ? DateTime.Now.Year : year;
    public int YearOfRelease => _yearOfRelease;

    public abstract void Move();

    public void PrintInfo() =>
        Console.WriteLine($"[{this.GetType().Name}] {Mark} {Model} ({YearOfRelease} г.)");
}

// 3. Класс Машина
class Car(string mark, string model, int year, int doors)
    : Vehicle(mark, model, year), IElectric
{
    public int NumberOfDoors { get; } = doors;

    public override void Move() => Console.WriteLine($"{Mark} плавно едет по трассе.");
    public void Charge() => Console.WriteLine($"{Mark} подключена к зарядной станции...");
}

// 4. Класс Мотоцикл
class Motorcycle(string mark, string model, int year, bool hasSidecar)
    : Vehicle(mark, model, year)
{
    public bool HasSidecar { get; } = hasSidecar;

    public override void Move() => Console.WriteLine($"{Mark} пролетает между рядами.");
}

class Program
{
    private const string FilePath = "garage.json";

    public static void Main()
    {
        // Загружаем старые данные при старте
        List<Vehicle> vehicles = LoadData();
        Console.WriteLine($"Загружено объектов из базы: {vehicles.Count}");

        while (true)
        {
            Console.WriteLine("\nМЕНЮ: 1 - Показать список, 2 - Добавить машину, 3 - Сохранить и выйти");
            string choice = Console.ReadLine();

            if (choice == "3") break;

            if (choice == "1")
            {
                if (vehicles.Count == 0) Console.WriteLine("Гараж пуст.");
                foreach (var v in vehicles.OrderBy(v => v.Mark))
                {
                    v.PrintInfo();

                    // Проверяем мечту
                    if (v.Mark.ToLower() == "porsche")
                    {
                        Console.WriteLine("   ⭐ ЭТО МОЯ МЕЧТА! ⭐");
                    }
                    if (v is IElectric ev) ev.Charge();
                }
            }
            else if (choice == "2")
            {
                Console.WriteLine("Введите через запятую: Марка, Модель, Год, Двери");
                string input = Console.ReadLine() ?? "";
                string[] parts = input.Split(',');

                if (parts.Length == 4 &&
                    int.TryParse(parts[2].Trim(), out int y) &&
                    int.TryParse(parts[3].Trim(), out int d))
                {
                    vehicles.Add(new Car(parts[0].Trim(), parts[1].Trim(), y, d));
                    Console.WriteLine("Машина успешно добавлена!");
                }
                else
                {
                    Console.WriteLine("Ошибка ввода данных.");
                }
            }
        }

        SaveData(vehicles);
    }

    // Метод сохранения
    static void SaveData(List<Vehicle> data)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(FilePath, json);
        Console.WriteLine("Данные сохранены.");
    }

    // Метод загрузки
    static List<Vehicle> LoadData()
    {
        if (!File.Exists(FilePath)) return new List<Vehicle>();

        try
        {
            string json = File.ReadAllText(FilePath);
            // Если файл пустой, возвращаем новый список
            return JsonSerializer.Deserialize<List<Vehicle>>(json) ?? new List<Vehicle>();
        }
        catch
        {
            return new List<Vehicle>();
        }
    }
}
