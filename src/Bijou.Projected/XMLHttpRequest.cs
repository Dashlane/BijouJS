using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Windows.Foundation.Metadata;

namespace Bijou.Projected
{
    public delegate void XHREventHandler();

    /// <summary>
    /// Class projected to JS context, implementing Http request feature.
    /// All public methods and members are accessible from JS context.
    /// Projected methods and members names are lowerCase as projection force them to lowerCase.
    /// </summary>
    public sealed class XMLHttpRequest
    {
        private enum enReadyState : int
        {
            Unsent = 0,
            Opened = 1,
            HeadersReceived = 2,
            Loading = 3,
            Done = 4,
        }

        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
        private Uri _uri;
        private string _httpMethod;

        private enReadyState _readyState = enReadyState.Unsent;
        public int readyState
        {
            get => (int)_readyState;
            private set
            {
                _readyState = (enReadyState)value;
                onreadystatechange?.Invoke();
            }
        }

        public string response => responseText;

        public string responseText { get; private set; }

        public string responseType { get; private set; }

        public bool withCredentials { get; set; }

        public XHREventHandler onreadystatechange { get; set; }
        public XHREventHandler onerror { get; set; }

        public void setRequestHeader(string key, string value)
        {
            _headers[key] = value;
        }

        public string getResponseHeader(string key)
        {
            if (_headers.ContainsKey(key)) {
                return _headers[key];
            }

            return null;
        }

        [DefaultOverload]
        public void open(string method, string url)
        {
            open(method, new Uri(url));
        }

        public void open(string method, Uri uri)
        {
            _httpMethod = method;
            _uri = uri;

            readyState = (int)enReadyState.Opened;
        }

        public void send()
        {
            SendAsync(string.Empty);
        }

        public void send(string data)
        {
            SendAsync(data);
        }

        private const string MsAppXHeader = "ms-appx:///";

        private async void SendAsync(string data)
        {
            if (_httpMethod == "GET" && _uri.OriginalString.StartsWith(MsAppXHeader, StringComparison.InvariantCulture)) {
                responseType = "text";
                responseText = File.ReadAllText(_uri.OriginalString.Substring(MsAppXHeader.Length));
                readyState = (int)enReadyState.Done;
                return;
            };

            using (var httpClient = new HttpClient()) {
                // Content* related headers are set on the content, not on the client.
                // Let's use them later
                var requestContentHeaders = new Dictionary<string, string>();

                foreach (var header in _headers) {
                    if (header.Key.StartsWith("Content", StringComparison.Ordinal))
                    {
                        requestContentHeaders.Add(header.Key, header.Value);
                    }
                    else
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                readyState = (int)enReadyState.HeadersReceived;

                HttpResponseMessage responseMessage = null;

                try {
                    switch (_httpMethod) {
                        case "DELETE":
                            responseMessage = await httpClient.DeleteAsync(_uri);
                            break;
                        case "PATCH":
                        case "POST":
                            using (var content = new StringContent(data)) {
                                // At this point, we want to override content headers set by default
                                // and use the ones from Carbon.
                                foreach(var entry in requestContentHeaders) {
                                    content.Headers.Remove(entry.Key);
                                    content.Headers.Add(entry.Key, entry.Value);
                                }
                                responseMessage = await httpClient.PostAsync(_uri, content);
                            }
                            break;
                        case "GET":
                            responseMessage = await httpClient.GetAsync(_uri);
                            break;
                    }
                } catch (ArgumentNullException anu) {
                    Console.Error.WriteLine($"[SendAsync][{_httpMethod}] invalid null argument: {anu.Message}");
                    onerror?.Invoke();
                } catch (InvalidOperationException ioe) {
                    Console.Error.WriteLine($"[SendAsync][{_httpMethod}] invalid operation exception: {ioe.Message}");
                    onerror?.Invoke();
                } catch (HttpRequestException reqe) {
                    Console.Error.WriteLine($"[SendAsync][{_httpMethod}] Http request exception: {reqe.Message}");
                    onerror?.Invoke();
                } catch (Exception e) {
                    Console.Error.WriteLine($"[SendAsync][{_httpMethod}] generic exception: {e.Message}");
                    onerror?.Invoke();
                    throw;
                }

                if (responseMessage == null) return;
                
                using (responseMessage) {
                    using (var content = responseMessage.Content) {
                        responseType = "text";
                        responseText = await content.ReadAsStringAsync();
                        readyState = (int)enReadyState.Done;
                    }
                }
            }
        }
    }
}
