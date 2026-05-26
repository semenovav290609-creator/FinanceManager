using System;
using System.Collections.Generic;
using System.Linq;

public interface IDiscount
{
    // Методы должны возвращать сумму скидки для конкретной позиции
    decimal GetPercentDiscount(decimal price, int quantity);
    decimal GetCategoryDiscount(string category, decimal price, int quantity);
    decimal GetWholesaleDiscount(int quantity, decimal price);
}

public class Product(string name, decimal price, string category)
{
    public string Name { get; set; } = name;
    public decimal Price { get; set; } = price;
    public string Category { get; set; } = category;
}

public class CartItem(Product product, int quantity)
{
    public Product Product { get; set; } = product;
    public int Quantity { get; set; } = quantity;   
}

public class DiscountService : IDiscount
{    
    public decimal GetPercentDiscount(decimal price, int quantity) 
        => (price * quantity * 10) / 100; // 10% на всё

    public decimal GetCategoryDiscount(string category, decimal price, int quantity)
        => category == "Еда" ? (price * quantity * 20) / 100 : 0;

    public decimal GetWholesaleDiscount(int quantity, decimal price)
        => quantity >= 5 ? (price * quantity * 30) / 100 : 0;
}

public class Program
{
    public static void Main(string[] args)
    {
        var service = new DiscountService();
        var cart = new List<CartItem>
        {
            new (new ("Яблоки", 100, "Еда"), 6),
            new (new ("Футболка", 1000, "Одежда"), 1),
            new (new ("Хлеб", 50, "Еда"), 2)
        };

        Console.WriteLine($"{DateTime.Now}\n");
        Console.WriteLine($"{"Товар",-10} | {"До",-8} | {"Скидка",-8} | {"После",-8}");
        Console.WriteLine(new string('-', 45));

        foreach (var item in cart)
        {
            decimal total = item.Product.Price * item.Quantity;
            
            // Выбираем лучшую скидку из трех по вашему интерфейсу
            decimal d1 = service.GetPercentDiscount(item.Product.Price, item.Quantity);
            decimal d2 = service.GetCategoryDiscount(item.Product.Category, item.Product.Price, item.Quantity);
            decimal d3 = service.GetWholesaleDiscount(item.Quantity, item.Product.Price);
            
            decimal bestDiscount = Math.Max(d1, Math.Max(d2, d3));
            decimal final = total - bestDiscount;

            Console.WriteLine($"{item.Product.Name,-10} | {total,8:0} | {bestDiscount,8:0} | {final,8:0}");
        }

        decimal totalSum = cart.Sum(i => {
            decimal t = i.Product.Price * i.Quantity;
            decimal d = new[] {
                service.GetPercentDiscount(i.Product.Price, i.Quantity),
                service.GetCategoryDiscount(i.Product.Category, i.Product.Price, i.Quantity),
                service.GetWholesaleDiscount(i.Quantity, i.Product.Price)
            }.Max();
            return t - d;
        });

        Console.WriteLine(new string('-', 45));
        Console.WriteLine($"ИТОГО К ОПЛАТЕ: {totalSum:0}");
    }
}
