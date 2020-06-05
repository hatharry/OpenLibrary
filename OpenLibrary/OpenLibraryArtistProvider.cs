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
    public class OpenLibraryArtistProvider : IRemoteMetadataProvider<MusicArtist, ArtistInfo>
    {
        private readonly IHttpClient _httpClient;
        private readonly IJsonSerializer _json;
        private readonly ILogger _logger;

        private readonly string searchUrl = "https://openlibrary.org/search/authors.json?q=";
        public string Name => "Open Library";

        public OpenLibraryArtistProvider(IHttpClient httpClient, IJsonSerializer json, ILogger logger)
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

        public async Task<MetadataResult<MusicArtist>> GetMetadata(ArtistInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<MusicArtist>
            {
                Item = new MusicArtist()
            };
            var httpRequestOptions = new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
            };

            httpRequestOptions.Url = $"{searchUrl}\"{HttpUtility.UrlEncode(info.Name)}\"";

            using (var resp = await _httpClient.GetResponse(httpRequestOptions).ConfigureAwait(false))
            {
                var openLibrarySearch = await _json.DeserializeFromStreamAsync<OpenLibrarySearch>(resp.Content).ConfigureAwait(false);

                var author = openLibrarySearch.docs.FirstOrDefault();
                if (author != null)
                {
                    result.HasMetadata = true;
                    result.Item.Name = author.name;
                }
            }

            return result;
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(ArtistInfo searchInfo, CancellationToken cancellationToken)
        {
            var results = new List<RemoteSearchResult>();
            var httpRequestOptions = new HttpRequestOptions
            {
                CancellationToken = cancellationToken,

            };

            httpRequestOptions.Url = $"{searchUrl}\"{HttpUtility.UrlEncode(searchInfo.Name)}\"";

            using (var resp = await _httpClient.GetResponse(httpRequestOptions).ConfigureAwait(false))
            {
                var openLibrarySearch = await _json.DeserializeFromStreamAsync<OpenLibrarySearch>(resp.Content).ConfigureAwait(false);

                foreach (var doc in openLibrarySearch.docs)
                {
                    var remoteSearchResult = new RemoteSearchResult
                    {
                        Name = doc.name,
                        SearchProviderName = Name
                    };
                    if (doc.key != null)
                    {
                        remoteSearchResult.ImageUrl = $"http://covers.openlibrary.org/a/olid/{doc.key}-L.jpg";
                    }
                    results.Add(remoteSearchResult);
                }
            }

            return results;
        }
    }
}
