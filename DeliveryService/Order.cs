namespace DeliveryServiceCore
{
    // Клас Замовлення
    public class Order
    {
        public int Id { get; set; }
        public Point Location { get; set; } // Координати ресторану
        public OrderStatus Status { get; set; } = OrderStatus.Created;
        public override string ToString() =>
            $"ID:{Id} Замовлення | Ресторан: ({Location.X},{Location.Y}) | Статус: {Status}";
    }
}