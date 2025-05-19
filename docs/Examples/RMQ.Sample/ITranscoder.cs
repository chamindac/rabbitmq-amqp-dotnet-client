using System.Threading.Tasks;

namespace RMQ.Consumer
{
    public interface ITranscoder
    {
        Task TranscodeAsync(string messageText);
    }
}
