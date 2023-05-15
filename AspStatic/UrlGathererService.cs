using System.Collections.Concurrent;

namespace AspStatic;

public record UrlGathererItem(string Url, HttpStatusCode? StatusCode);

public interface IUrlGathererService
{
    IEnumerable<UrlGathererItem> GetGatheredUrls();
    void Add(UrlGathererItem item);
    void Clear();
}

class UrlGathererService : IUrlGathererService
{
    readonly ConcurrentDictionary<string, UrlGathererItem> urls = new();

    public void Add(UrlGathererItem item)
    {
        urls.TryAdd(item.Url, item);
    }

    public void Clear()
    {
        urls.Clear();
    }

    public IEnumerable<UrlGathererItem> GetGatheredUrls()
    {
        return urls.Values;
    }

}