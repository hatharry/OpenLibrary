using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace OpenLibrary
{
    public class OpenLibraryImageProvider : IRemoteImageProvider
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _json;
        private readonly string baseUrl = "https://openlibrary.org/api/books?format=json&jscmd=data&bibkeys=";
        private readonly string searchUrl = "https://openlibrary.org/search.json?title=";
        private readonly string authorUrl = "https://openlibrary.org/search/authors.json?q=";

        public string Name => "Open Library";

        public OpenLibraryImageProvider(IHttpClient httpClient, ILogger logger, IJsonSerializer json)
        {
            _httpClient = httpClient;
            _logger = logger;
            _json = json;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClient.GetResponse(new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
                Url = url
            });
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken)
        {
            var list = new List<RemoteImageInfo>();

            var httpRequestOptions = new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
            };
            if (item is MusicAlbum || item is Book)
            {
                if (item.ProviderIds.TryGetValue("isbn", out string isbn))
                {
                    httpRequestOptions.Url = $"{baseUrl}ISBN:{isbn}";
                    using (var resp = await _httpClient.GetResponse(httpRequestOptions).ConfigureAwait(false))
                    {
                        var openLibrarySearch = await _json.DeserializeFromStreamAsync<Dictionary<string, OpenLibraryResp>>(resp.Content).ConfigureAwait(false);
                        foreach (var book in openLibrarySearch)
                        {
                            list.Add(new RemoteImageInfo
                            {
                                Url = book.Value.cover.large,
                                ThumbnailUrl = book.Value.cover.medium,
                                ProviderName = Name
                            });
                        }
                    }
                }
                else
                {
                    httpRequestOptions.Url = $"{searchUrl}\"{HttpUtility.UrlEncode(item.Name)}\"";

                    using (var resp = await _httpClient.GetResponse(httpRequestOptions).ConfigureAwait(false))
                    {
                        var openLibrarySearch = await _json.DeserializeFromStreamAsync<OpenLibrarySearch>(resp.Content).ConfigureAwait(false);
                        foreach (var book in openLibrarySearch.docs)
                        {
                            list.Add(new RemoteImageInfo
                            {
                                Url = $"https://covers.openlibrary.org/b/id/{book.cover_i}-L.jpg",
                                ThumbnailUrl = $"https://covers.openlibrary.org/b/id/{book.cover_i}-M.jpg",
                                ProviderName = Name
                            });

                        }
                    }
                }
            }
            if (item is MusicArtist)
            {
                httpRequestOptions.Url = $"{authorUrl}\"{HttpUtility.UrlEncode(item.Name)}\"";

                using (var resp = await _httpClient.GetResponse(httpRequestOptions).ConfigureAwait(false))
                {
                    var openLibrarySearch = await _json.DeserializeFromStreamAsync<OpenLibrarySearch>(resp.Content).ConfigureAwait(false);
                    foreach (var book in openLibrarySearch.docs)
                    {
                        list.Add(new RemoteImageInfo
                        {
                            Url = $"https://covers.openlibrary.org/a/olid/{book.key}-L.jpg",
                            ThumbnailUrl = $"https://covers.openlibrary.org/a/olid/{book.key}-M.jpg",
                            ProviderName = Name
                        });

                    }
                }
            }

            return list;
        }

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new List<ImageType>
            {
                ImageType.Primary
            };
        }

        public bool Supports(BaseItem item)
        {
            return item is MusicAlbum || item is MusicArtist || item is Book;
        }
    }
}
