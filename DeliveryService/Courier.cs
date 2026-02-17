

namespace DeliveryServiceCore
{
    // Клас Кур'єра
    public class Courier
    {
        public int Id { get; set; }
        public Point Location { get; set; }
        public bool IsAvailable { get; set; } = true;
        public override string ToString() =>
            $"ID:{Id} Кур'єр | Позиція: ({Location.X},{Location.Y}) | Статус: {(IsAvailable ? "Вільний" : "Зайнятий")}";
    }
}