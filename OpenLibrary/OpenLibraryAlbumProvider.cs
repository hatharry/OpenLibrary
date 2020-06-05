using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace OpenLibrary
{
    public class OpenLibraryAlbumProvider : IRemoteMetadataProvider<MusicAlbum, AlbumInfo>
    {
        private readonly IHttpClient _httpClient;
        private readonly IJsonSerializer _json;
        private readonly ILogger _logger;

        private readonly string baseUrl = "https://openlibrary.org/api/books?format=json&jscmd=data&bibkeys=";
        private readonly string searchUrl = "https://openlibrary.org/search.json?title=";
        public string Name => "Open Library";

        public OpenLibraryAlbumProvider(IHttpClient httpClient, IJsonSerializer json, ILogger logger)
        {
            _httpClient = httpClient;
            _json = json;
            _logger = logger;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClient.GetResponse(new HttpRequestOptions
            {
                Url = url,
                CancellationToken = cancellationToken
            });
        }

        public async Task<MetadataResult<MusicAlbum>> GetMetadata(AlbumInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<MusicAlbum>
            {
                Item = new MusicAlbum()
            };
            var httpRequestOptions = new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
            };
            if (info.ProviderIds.TryGetValue("isbn", out string isbn))
            {
                httpRequestOptions.Url = $"{baseUrl}ISBN:{isbn}";

                using (var resp = await _httpClient.GetResponse(httpRequestOptions).ConfigureAwait(false))
                {
                    var openLibrarySearch = await _json.DeserializeFromStreamAsync<Dictionary<string, OpenLibraryResp>>(resp.Content).ConfigureAwait(false);

                    if (openLibrarySearch.TryGetValue($"ISBN:{isbn}", out OpenLibraryResp book))
                    {
                        result.HasMetadata = true;
                        result.Item.Album = book.title;
                        result.Item.AlbumArtists = book.authors.Select(i => i.name).ToArray();
                        result.Item.Artists = book.authors.Select(i => i.name).ToArray();
                        foreach (var dewey in book.classifications.dewey_decimal_class)
                        {
                            result.Item.AddGenre(dewey);
                        }
                    }
                }
            }
            else
            {
                httpRequestOptions.Url = $"{searchUrl}\"{HttpUtility.UrlEncode(info.Name)}\"";

                using (var resp = await _httpClient.GetResponse(httpRequestOptions).ConfigureAwait(false))
                {
                    var openLibrarySearch = await _json.DeserializeFromStreamAsync<OpenLibrarySearch>(resp.Content).ConfigureAwait(false);

                    var book = openLibrarySearch.docs.FirstOrDefault();
                    if (book != null)
                    {
                        result.HasMetadata = true;
                        result.Item.Album = book.title;
                        result.Item.AlbumArtists = book.author_name;
                        result.Item.Artists = book.author_name;
                        result.Item.ProductionYear = book.first_publish_year;
                        if (book.isbn != null)
                        {
                            result.Item.ProviderIds.Add("isbn", book.isbn.FirstOrDefault());
                        }

                    }
                }
            }

            return result;
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(AlbumInfo searchInfo, CancellationToken cancellationToken)
        {
            var results = new List<RemoteSearchResult>();
            var httpRequestOptions = new HttpRequestOptions
            {
                CancellationToken = cancellationToken,

            };
            if (searchInfo.ProviderIds.TryGetValue("isbn", out string isbn))
            {
                httpRequestOptions.Url = $"{baseUrl}ISBN:{isbn}";

                using (var resp = await _httpClient.GetResponse(httpRequestOptions).ConfigureAwait(false))
                {
                    var openLibrarySearch = await _json.DeserializeFromStreamAsync<Dictionary<string, OpenLibraryResp>>(resp.Content).ConfigureAwait(false);

                    foreach (var book in openLibrarySearch)
                    {
                        var remoteSearchResult = new RemoteSearchResult
                        {
                            Name = book.Value.title,
                            SearchProviderName = Name
                        };
                        if (book.Value.authors != null)
                        {
                            remoteSearchResult.AlbumArtist = new RemoteSearchResult { Name = book.Value.authors.FirstOrDefault().name, SearchProviderName = Name };
                            var artists = new List<RemoteSearchResult>();
                            foreach (var author in book.Value.authors)
                            {
                                artists.Add(new RemoteSearchResult { Name = author.name, SearchProviderName = Name });
                            }
                            remoteSearchResult.Artists = artists.ToArray();
                        }
                        if (book.Key != null)
                        {
                            remoteSearchResult.ProviderIds.Add("isbn", book.Key);
                        }
                        if (book.Value.cover != null)
                        {
                            remoteSearchResult.ImageUrl = book.Value.cover.large;
                        }
                        results.Add(remoteSearchResult);
                    }
                }
            }
            else
            {
                httpRequestOptions.Url = $"{searchUrl}\"{HttpUtility.UrlEncode(searchInfo.Name)}\"";

                using (var resp = await _httpClient.GetResponse(httpRequestOptions).ConfigureAwait(false))
                {
                    var openLibrarySearch = await _json.DeserializeFromStreamAsync<OpenLibrarySearch>(resp.Content).ConfigureAwait(false);

                    foreach (var doc in openLibrarySearch.docs)
                    {
                        var remoteSearchResult = new RemoteSearchResult
                        {
                            Name = doc.title,
                            SearchProviderName = Name
                        };
                        if (doc.isbn != null)
                        {
                            remoteSearchResult.ProviderIds.Add("isbn", doc.isbn.FirstOrDefault());
                        }
                        if (doc.cover_i != null)
                        {
                            remoteSearchResult.ImageUrl = $"https://covers.openlibrary.org/b/id/{doc.cover_i}-L.jpg";
                        }
                        if (doc.author_name != null)
                        {
                            remoteSearchResult.AlbumArtist = new RemoteSearchResult { Name = doc.author_name.FirstOrDefault(), SearchProviderName = Name };
                            var artists = new List<RemoteSearchResult>();
                            foreach (var author in doc.author_name)
                            {
                                artists.Add(new RemoteSearchResult { Name = author, SearchProviderName = Name });
                            }
                            remoteSearchResult.Artists = artists.ToArray();
                        }
                        if (doc.first_publish_year != null)
                        {
                            remoteSearchResult.ProductionYear = doc.first_publish_year;
                        }
                        results.Add(remoteSearchResult);
                    }
                }
            }

            return results;
        }
    }
}
