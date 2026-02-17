namespace DeliveryServiceCore
{
   
    // Клас Кур'єра
    public class Courier
    {
        public int Id { get; set; }
        public Point Location { get; set; }
        public TransportType Transport { get; set; } = TransportType.Walker;
        public bool IsAvailable { get; set; } = true;

        // Скільки замовлень кур'єр виконав сьогодні
        public int CompletedOrdersToday { get; set; } = 0;

        // Перевіряє, чи може кур'єр перевезти вантаж із заданою вагою (кг)
        public bool CanCarry(int weightKg) => Transport switch
        {
            TransportType.Walker => weightKg <= 5,
            TransportType.Bicycle => weightKg <= 15,
            TransportType.Car => weightKg <= 50,
            _ => false
        };

        public override string ToString() =>
            $"[ID:{Id}] Кур'єр | Поз: ({Location.X},{Location.Y}) | Транспорт: {Transport} | Статус: {(IsAvailable ? "Вільний" : "Зайнятий")} | Виконано сьогодні: {CompletedOrdersToday}";
    }
}