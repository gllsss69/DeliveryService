using System;
using System.Collections.Generic;
using System.Linq;

namespace DeliveryServiceCore
{
    // Базові структури даних
    public record Point(int X, int Y);
    public enum OrderStatus { Created, Assigned, Delivered }

    // Ядро системи (Логіка MVP Етапу 1)
    public class DeliveryService
    {
        private List<Courier> _couriers = new();
        private List<Order> _orders = new();
        private int _cCounter = 1;
        private int _oCounter = 1;

        public void AddCourier(int x, int y) =>
            _couriers.Add(new Courier { Id = _cCounter++, Location = new Point(x, y) });

        // Реалізація логіки пошуку найближчого кур'єра
        public string CreateOrder(int x, int y)
        {
            var order = new Order { Id = _oCounter++, Location = new Point(x, y) };
            _orders.Add(order);

            // 1. Знаходимо всіх вільних кур'єрів
            var freeCouriers = _couriers.Where(c => c.IsAvailable).ToList();

            // 2. Якщо вільних немає — повертаємо відповідний статус
            if (!freeCouriers.Any()) return "Немає кур'єрів";

            // 3. Обчислюємо відстань та знаходимо найближчого (Евклідова відстань)
            var nearestCourier = freeCouriers
                .OrderBy(c => Math.Sqrt(Math.Pow(x - c.Location.X, 2) + Math.Pow(y - c.Location.Y, 2)))
                .First();

            // 4. Змінюємо статус кур'єра на Busy та призначаємо замовлення
            nearestCourier.IsAvailable = false;
            order.Status = OrderStatus.Assigned;

            return $"Успіх! Замовлення #{order.Id} (Ресторан: {x},{y}) призначено кур'єру #{nearestCourier.Id}.";
        }

        public List<Courier> GetAllCouriers() => _couriers;
        public List<Order> GetAllOrders() => _orders;

        public bool RemoveOrder(int id) => _orders.RemoveAll(o => o.Id == id) > 0;
        public bool RemoveCourier(int id) => _couriers.RemoveAll(c => c.Id == id && c.IsAvailable) > 0;

        public bool CompleteOrder(int id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id && o.Status == OrderStatus.Assigned);
            if (order == null) return false;

            order.Status = OrderStatus.Delivered;

            // Звільняємо кур'єра (спрощений пошук для MVP)
            var courier = _couriers.FirstOrDefault(c => !c.IsAvailable);
            if (courier != null) courier.IsAvailable = true;

            return true;
        }
    }

    class Program
    {
        static DeliveryService _service = new DeliveryService();

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== ГОЛОВНЕ МЕНЮ СИСТЕМИ ДОСТАВКИ ===");
                Console.WriteLine("1. Керування кур'єрами");
                Console.WriteLine("2. Керування замовленнями (MVP)");
                Console.WriteLine("0. Вихід");
                Console.Write("\nОберіть розділ: ");

                switch (Console.ReadLine())
                {
                    case "1": CourierMenu(); break;
                    case "2": OrderMenu(); break;
                    case "0": exit = true; break;
                }
            }
        }

        static void OrderMenu()
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("--- МЕНЮ ЗАМОВЛЕНЬ ---");
                Console.WriteLine("1. Нове замовлення");
                Console.WriteLine("2. Список замовлень");
                Console.WriteLine("3. Завершити доставку");
                Console.WriteLine("0. Назад у головне меню");
                Console.Write("\nДія: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        Console.Write("Введіть координати ресторану (X Y): ");
                        var pos = ReadCoords();
                        string result = _service.CreateOrder(pos.x, pos.y);
                        Console.WriteLine($"\n[РЕЗУЛЬТАТ]: {result}");
                        Pause();
                        break;
                    case "2":
                        Console.WriteLine("\nСПИСОК ЗАМОВЛЕНЬ:");
                        var orders = _service.GetAllOrders();
                        if (!orders.Any()) Console.WriteLine("Порожньо.");
                        else orders.ForEach(o => Console.WriteLine(o));
                        Pause();
                        break;
                    case "3":
                        Console.Write("Введіть ID замовлення для завершення: ");
                        if (int.TryParse(Console.ReadLine(), out int id))
                        {
                            if (_service.CompleteOrder(id)) Console.WriteLine("Статус змінено на Доставлено. Кур'єр вільний.");
                            else Console.WriteLine("Помилка: замовлення не знайдено або вже виконано.");
                        }
                        Pause();
                        break;
                    case "0": back = true; break;
                }
            }
        }

        static void CourierMenu()
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("--- МЕНЮ КУР'ЄРІВ ---");
                Console.WriteLine("1. Додати кур'єра");
                Console.WriteLine("2. Список кур'єрів");
                Console.WriteLine("3. Видалити кур'єра");
                Console.WriteLine("0. Назад у головне меню");
                Console.Write("\nДія: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        Console.Write("Введіть початкові координати (X Y): ");
                        var pos = ReadCoords();
                        _service.AddCourier(pos.x, pos.y);
                        Console.WriteLine("Кур'єра додано.");
                        Pause();
                        break;
                    case "2":
                        Console.WriteLine("\nСПИСОК КУР'ЄРІВ:");
                        var couriers = _service.GetAllCouriers();
                        if (!couriers.Any()) Console.WriteLine("Кур'єрів немає.");
                        else couriers.ForEach(c => Console.WriteLine(c));
                        Pause();
                        break;
                    case "3":
                        Console.Write("ID кур'єра для видалення: ");
                        if (int.TryParse(Console.ReadLine(), out int id))
                        {
                            if (_service.RemoveCourier(id)) Console.WriteLine("Кур'єр видалений.");
                            else Console.WriteLine("Помилка: кур'єр зайнятий або не існує.");
                        }
                        Pause();
                        break;
                    case "0": back = true; break;
                }
            }
        }

        static (int x, int y) ReadCoords()
        {
            string input = Console.ReadLine() ?? "0 0";
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int x = parts.Length > 0 && int.TryParse(parts[0], out int resX) ? resX : 0;
            int y = parts.Length > 1 && int.TryParse(parts[1], out int resY) ? resY : 0;
            return (x, y);
        }

        static void Pause()
        {
            Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
            Console.ReadKey();
        }
    }
}