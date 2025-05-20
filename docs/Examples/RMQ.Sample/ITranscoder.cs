using System.Threading.Tasks;
using RabbitMQ.AMQP.Client;

namespace RMQ.Consumer
{
    public interface ITranscoder
    {
        Task TranscodeAsync(string messageText, IContext context);
    }
}
