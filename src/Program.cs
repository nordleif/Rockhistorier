using Rockhistorier;

// https://podcastaddict.com/podcast/rockhistorier/2497821
// https://www.spreaker.com/show/4170168/episodes/feed

var url = "https://www.spreaker.com/show/4170168/episodes/feed";
var path = "D:\\Nordleif\\Rockhistorier";

await Downloader.DownloadAsync(url, path);
await Transcriber.Transcribe(path);
