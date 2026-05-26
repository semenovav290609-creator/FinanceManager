using System;

class Program
{
    static void Main(string[] args)
    {
        int playerGold = 500; // 💰 Кошелек игрока (создаем в Main)

        // 1. Метод посчитал нам сумму и вернул её в переменную totalCost
        int totalCost = GetUpgradeCost(1, 4, 100);

        Console.WriteLine($"Общая стоимость улучшения: {totalCost} золотых");

        // 2. ПРОВЕРКА (прямо здесь, в методе Main):
        if (totalCost <= playerGold)
        {
            Console.WriteLine("✅ Улучшение успешно куплено!");
        }
        else
        {
            // Считаем, сколько именно монет не хватает игроку
            int missingGold = totalCost - playerGold;
            Console.WriteLine($"❌ Недостаточно золота! Вам не хватает {missingGold} монет.");
        }
    }

    // Этот метод теперь занимается ТОЛЬКО математикой (считает цену)
    static int GetUpgradeCost(int currentLevel, int targetLevel, int basePrice)
    {
        int sum = 0;

        for (int i = currentLevel; i < targetLevel; i++)
        {
            sum += basePrice * i;
        }

        return sum; // Вернули результат и вышли
    }
}
