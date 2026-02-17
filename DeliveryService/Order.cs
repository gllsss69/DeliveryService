namespace DeliveryServiceCore
{
    // Клас Замовлення
    public class Order
    {
        public int Id { get; set; }
        public Point Location { get; set; } // Координати ресторану
        public int WeightKg { get; set; } = 0; // Вага замовлення у кілограмах
        public OrderStatus Status { get; set; } = OrderStatus.Created;

        // Зберігаємо посилання на призначеного кур'єра (null якщо не призначено)
        public Courier? AssignedCourier { get; set; }

        public override string ToString() =>
            $"[ID:{Id}] Замовлення | Ресторан: ({Location.X},{Location.Y}) | Вага: {WeightKg}kg | Статус: {Status}" +
            (AssignedCourier is not null ? $" | Кур'єр: {AssignedCourier.Id}" : "");
    }
}