using System;
using System.Collections.Generic;
using System.Linq;

namespace DeliveryServiceCore
{
    // Базові структури даних
    public record Point(int X, int Y);
    public enum OrderStatus { Created, Assigned, Delivered }

    // Типи транспорту та їх максимально допустима вага
    public enum TransportType { Walker, Bicycle, Car }

    // Ядро системи (Логіка MVP Етапу 1)
    public class DeliveryService
    {
        private List<Courier> _couriers = new();
        private List<Order> _orders = new();
        private List<Order> _orderQueue = new(); // FIFO queue implemented as list for searching/removal
        private int _cCounter = 1;
        private int _oCounter = 1;

        public void AddCourier(int x, int y, TransportType transport) =>
            _couriers.Add(new Courier { Id = _cCounter++, Location = new Point(x, y), Transport = transport });

        // Реалізація логіки пошуку найближчого кур'єра з урахуванням ваги та пріоритетів
        public string CreateOrder(int x, int y, int weightKg)
        {
            var order = new Order { Id = _oCounter++, Location = new Point(x, y), WeightKg = weightKg };
            _orders.Add(order);

            // 1. Знаходимо всіх вільних кур'єрів, які можуть перевозити цю вагу
            var suitableCouriers = _couriers.Where(c => c.IsAvailable && c.CanCarry(weightKg)).ToList();

            // 2. Якщо підходящих немає — кладемо в чергу
            if (!suitableCouriers.Any())
            {
                _orderQueue.Add(order);
                return $"Додано в чергу. Позиція в черзі: {_orderQueue.Count}.";
            }

            // 3. Обчислюємо відстані
            var courierDistances = suitableCouriers
                .Select(c => new { Courier = c, Distance = Distance(c.Location, order.Location) })
                .ToList();

            // мінімальна відстань
            var minDistance = courierDistances.Min(cd => cd.Distance);

            // Кандидати — ті, що в межах 1 одиниці від мінімальної відстані
            var candidates = courierDistances.Where(cd => cd.Distance <= minDistance + 1.0).ToList();

            // Якщо більше одного кандидата — пріоритет тому, хто виконав менше замовлень сьогодні
            var chosen = candidates
                .OrderBy(cd => cd.Courier.CompletedOrdersToday) // priority by completed today if within 1 unit
                .ThenBy(cd => cd.Distance) // tiebreaker: closer distance
                .First()
                .Courier;

            // 4. Призначаємо
            AssignOrderToCourier(order, chosen);

            return $"Успіх! Замовлення #{order.Id} (Ресторан: {x},{y}, {weightKg}kg) призначено кур'єру #{chosen.Id}.";
        }

        // Helper to compute Euclidean distance
        private static double Distance(Point a, Point b) =>
            Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

        // Assign order to courier (used for both new orders and when dequeuing)
        private void AssignOrderToCourier(Order order, Courier courier)
        {
            courier.IsAvailable = false;
            order.Status = OrderStatus.Assigned;
            order.AssignedCourier = courier;
            // If the order was in queue, remove it
            _orderQueue.RemoveAll(o => o.Id == order.Id);
        }

        // Attempt to assign the first queued order that this courier can carry
        private bool TryAssignFirstQueuedForCourier(Courier courier)
        {
            var idx = _orderQueue.FindIndex(o => courier.CanCarry(o.WeightKg));
            if (idx < 0) return false;

            var order = _orderQueue[idx];
            AssignOrderToCourier(order, courier);
            return true;
        }

        public List<Courier> GetAllCouriers() => _couriers.ToList();
        public List<Order> GetAllOrders() => _orders.ToList();

        public bool RemoveOrder(int id) => _orders.RemoveAll(o => o.Id == id) > 0;
        public bool RemoveCourier(int id) => _couriers.RemoveAll(c => c.Id == id && c.IsAvailable) > 0;

        public bool CompleteOrder(int id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id && o.Status == OrderStatus.Assigned);
            if (order == null) return false;

            order.Status = OrderStatus.Delivered;

            // Звільняємо конкретного призначеного кур'єра
            var courier = order.AssignedCourier;
            if (courier != null)
            {
                // mark delivered
                courier.CompletedOrdersToday++;

                // make courier available first
                courier.IsAvailable = true;

                // try to assign first queued order that courier can carry
                // if successful, courier will become busy again
                if (TryAssignFirstQueuedForCourier(courier))
                {
                    // assigned from queue; leave courier.IsAvailable = false inside AssignOrderToCourier
                    // and update status/reference already done
                }
                else
                {
                    // remains available
                }
            }

            return true;
        }
    }

    // Rest of Program (menus) unchanged...
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
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(@"
  ____       _ _                      ____                  _          
 |  _ \  ___| (_)_   _____ _ __ _   _/ ___|  ___ _ ____   _(_) ___ ___ 
 | | | |/ _ \ | \ \ / / _ \ '__| | | \___ \ / _ \ '__\ \ / / |/ __/ _ \
 | |_| |  __/ | |\ V /  __/ |  | |_| |___) |  __/ |   \ V /| | (_|  __/
 |____/ \___|_|_| \_/ \___|_|   \__, |____/ \___|_|    \_/ |_|\___\___|
                                |___/                         

");
                Console.ResetColor();
                Console.WriteLine("----------------------------------------------------------------------------");
                Console.WriteLine("\nВітаємо у системі доставки! Оберіть розділ для керування:");
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
                        Console.Write("Введіть вагу замовлення (kg): ");
                        int.TryParse(Console.ReadLine(), out int weight);
                        string result = _service.CreateOrder(pos.x, pos.y, weight);
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
                            if (_service.CompleteOrder(id)) Console.WriteLine("Статус змінено на Доставлено. Кур'єр вільний або отримав нове завдання.");
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

                        Console.WriteLine("Оберіть тип транспорту: 1) Walker (до 5kg)  2) Bicycle (до 15kg)  3) Car (до 50kg)");
                        Console.Write("Введіть номер типу: ");
                        TransportType transport = TransportType.Walker;
                        var tInput = Console.ReadLine();
                        if (tInput == "2") transport = TransportType.Bicycle;
                        else if (tInput == "3") transport = TransportType.Car;

                        _service.AddCourier(pos.x, pos.y, transport);
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