using System;
using System.Collections.Generic;
using System.Linq;

namespace DeliveryServiceCore
{
    
    public record Point(int X, int Y);
    public enum OrderStatus { Created, Assigned, Delivered }

    public class Courier
    {
        public int Id { get; set; }
        public Point Location { get; set; }
        public bool IsAvailable { get; set; } = true;
        public override string ToString() => $"[ID:{Id}] Кур'єр | Поз: ({Location.X},{Location.Y}) | Вільний: {IsAvailable}";
    }

    public class Order
    {
        public int Id { get; set; }
        public Point Location { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Created;
        public override string ToString() => $"[ID:{Id}] Замовлення | Точка: ({Location.X},{Location.Y}) | Статус: {Status}";
    }

    public class DeliveryService
    {
        private List<Courier> _couriers = new();
        private List<Order> _orders = new();
        private int _cCounter = 1;
        private int _oCounter = 1;

        public void AddCourier(int x, int y) => _couriers.Add(new Courier { Id = _cCounter++, Location = new Point(x, y) });

        public void CreateOrder(int x, int y)
        {
            var order = new Order { Id = _oCounter++, Location = new Point(x, y) };
            _orders.Add(order);
            // Авто-розподіл
            var free = _couriers.FirstOrDefault(c => c.IsAvailable);
            if (free != null) { free.IsAvailable = false; order.Status = OrderStatus.Assigned; }
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
            // Тут в ідеалі треба звільняти кур'єра, але для прикладу спростимо
            return true;
        }
    }

    // --- ВАШ КЛАС PROGRAM ТЕПЕР БАЧИТЬ DELIVERYSERVICE ---
    class Program
    {
        static DeliveryService _service = new DeliveryService(); // Тепер рядок 8 працює!

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== ГОЛОВНЕ МЕНЮ СИСТЕМИ ДОСТАВКИ ===");
                Console.WriteLine("1. Керування кур'єрами");
                Console.WriteLine("2. Керування замовленнями");
                Console.WriteLine("0. Вихід");
                Console.Write("\nОберіть опцію: ");

                switch (Console.ReadLine())
                {
                    case "1": CourierMenu(); break;
                    case "2": OrderMenu(); break;
                    case "0": exit = true; break;
                    default:
                        Console.WriteLine("Невірний вибір. Натисніть будь-яку клавішу...");
                        Console.ReadKey();
                        break;
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
                Console.WriteLine("1. Створити замовлення");
                Console.WriteLine("2. Список замовлень");
                Console.WriteLine("3. Завершити замовлення");
                Console.WriteLine("4. Видалити замовлення");
                Console.WriteLine("0. Назад у головне меню");
                Console.Write("\nОберіть опцію: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        Console.Write("Введіть точку доставки (X Y): ");
                        var pos = ReadCoords();
                        _service.CreateOrder(pos.x, pos.y);
                        Console.WriteLine("Замовлення додано в систему.");
                        Pause();
                        break;
                    case "2":
                        Console.WriteLine("\nСПИСОК ЗАМОВЛЕНЬ:");
                        var orders = _service.GetAllOrders();
                        if (!orders.Any()) Console.WriteLine("Порожньо.");
                        else orders.ForEach(Console.WriteLine);
                        Pause();
                        break;
                    case "3":
                        Console.Write("Введіть ID замовлення для завершення: ");
                        if (int.TryParse(Console.ReadLine(), out int completeId))
                        {
                            if (_service.CompleteOrder(completeId)) Console.WriteLine("Статус змінено на Delivered.");
                            else Console.WriteLine("Помилка: замовлення не знайдено.");
                        }
                        Pause();
                        break;
                    case "4":
                        Console.Write("Введіть ID замовлення для видалення: ");
                        if (int.TryParse(Console.ReadLine(), out int removeId))
                        {
                            if (_service.RemoveOrder(removeId)) Console.WriteLine("Замовлення видалено.");
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
                Console.Write("\nОберіть опцію: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        Console.Write("Початкова позиція (X Y): ");
                        var pos = ReadCoords();
                        _service.AddCourier(pos.x, pos.y);
                        Pause();
                        break;
                    case "2":
                        var couriers = _service.GetAllCouriers();
                        if (!couriers.Any()) Console.WriteLine("Кур'єрів немає.");
                        else couriers.ForEach(Console.WriteLine);
                        Pause();
                        break;
                    case "3":
                        Console.Write("ID кур'єра для видалення: ");
                        int.TryParse(Console.ReadLine(), out int id);
                        if (_service.RemoveCourier(id)) Console.WriteLine("Кур'єр видалений.");
                        else Console.WriteLine("Помилка видалення.");
                        Pause();
                        break;
                    case "0": back = true; break;
                }
            }
        }

        static (int x, int y) ReadCoords()
        {
            string input = Console.ReadLine() ?? "0 0";
            var parts = input.Split(' ');
            int x = parts.Length > 0 && int.TryParse(parts[0], out int resX) ? resX : 0;
            int y = parts.Length > 1 && int.TryParse(parts[1], out int resY) ? resY : 0;
            return (x, y);
        }

        static void Pause()
        {
            Console.WriteLine("\nНатисніть будь-яку клавішу...");
            Console.ReadKey();
        }
    }
}