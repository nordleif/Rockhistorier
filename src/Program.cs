using Rockhistorier;

var path = "D:\\Nordleif\\Rockhistorier";

// https://podcastaddict.com/podcast/rockhistorier/2497821
// https://www.spreaker.com/show/4170168/episodes/feed
//var url = "https://www.spreaker.com/show/4170168/episodes/feed";
//await Downloader.DownloadAsync(url, path);

await Transcriber.Transcribe(path);
