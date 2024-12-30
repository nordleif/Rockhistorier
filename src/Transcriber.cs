using NAudio.Wave;
using System.Diagnostics;
using System.Formats.Tar;
using Whisper.net;
using Whisper.net.Ggml;

namespace Rockhistorier
{
    internal static class Transcriber
    {
        private const string TempTextFileName = "D:\\Temp\\temp.txt";
        private const string TempWavFileName = "D:\\Temp\\temp.wav";

        public async static Task Transcribe(string path)
        {
            var files = Directory.GetFiles(path).OrderByDescending(f => f).ToList();
            foreach(var file in files)
            {
                var extension = Path.GetExtension(file);
                if (!extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase))
                    continue;

                var textFileName = $"{file}.txt";
                if (File.Exists(textFileName))
                    continue;

                Console.WriteLine(Path.GetFileNameWithoutExtension(file));

                var sw = Stopwatch.StartNew();
                
                File.Delete(TempTextFileName);
                File.Delete(TempWavFileName);

                if (!File.Exists(TempWavFileName)) 
                    ConvertMp3ToWav(file, TempWavFileName);

                var ggmlType = GgmlType.LargeV3Turbo;
                var modelName = $"D:\\Temp\\ggml-{ggmlType.ToString().ToLower()}.bin";
                if (!File.Exists(modelName))
                {
                    Console.WriteLine("Downloading model...");
                    using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
                    using var fileWriter = File.OpenWrite(modelName);
                    await modelStream.CopyToAsync(fileWriter);
                }
                
                using var whisperFactory = WhisperFactory.FromPath(modelName);
                using var processor = whisperFactory.CreateBuilder()
                    .WithLanguage("da-DK")
                    .Build();

                using var fileStream = File.OpenRead(TempWavFileName);

                Console.WriteLine("Transcribing...");
                await foreach (var result in processor.ProcessAsync(fileStream))
                {
                    var text = $"{result.Start.ToString(@"hh\:mm\:ss")} -> {result.End.ToString(@"hh\:mm\:ss")} : {result.Text}";
                    Console.WriteLine(text);
                    File.AppendAllLines(TempTextFileName, [text]);
                }

                File.Copy(TempTextFileName, textFileName);

                Console.WriteLine($"Transcription completed: {sw}");
            }
        }

        static void ConvertMp3ToWav(string mp3FileName, string wavFileName)
        {
            Console.WriteLine("Converting to wav file...");

            using (var mp3Reader = new Mp3FileReader(mp3FileName))
            using (var resampler = new MediaFoundationResampler(mp3Reader, new WaveFormat(16000, mp3Reader.WaveFormat.Channels)))
            {
                resampler.ResamplerQuality = 60; // Set quality (0-60, higher is better)
                WaveFileWriter.CreateWaveFile(wavFileName, resampler);
            }


        }
    }
}
