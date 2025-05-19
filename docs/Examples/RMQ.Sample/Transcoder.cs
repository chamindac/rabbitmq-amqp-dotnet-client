using System;
using System.Threading.Tasks;

namespace RMQ.Consumer
{
    internal class Transcoder : ITranscoder
    {
        public Transcoder()
        {
        }

        public async Task TranscodeAsync(string messageText)
        {
            await DoTranscodeAsync(1000, "Transcode 1", messageText);
            await DoTranscodeAsync(2000, "Transcode 2", messageText);
        }

        private async Task DoTranscodeAsync(int milliseconds, string taskName, string messageText)
        {
            int cycles = Random.Shared.Next(5, 10);
            for (int i = 0; i < cycles; i++)
            {
                await Task.Delay(milliseconds);
                Console.WriteLine($"{taskName} still transcoding {i} for {messageText}");
            }
            Console.WriteLine($"{taskName} completed for {messageText}");
        }
    }
}
