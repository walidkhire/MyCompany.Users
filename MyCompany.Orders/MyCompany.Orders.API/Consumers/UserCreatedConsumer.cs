using MassTransit;
using MyCompany.Shared.Events;

namespace MyCompany.Orders.API.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
    {
        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var message = context.Message;

            Console.WriteLine(
                $"[Orders.API] User reçu : {message.UserId} - {message.Email}"
            );

            // Ici :
            // - créer un client local
            // - stocker une projection
            // - initialiser des données Orders

            await Task.CompletedTask;
        }
    }
}
