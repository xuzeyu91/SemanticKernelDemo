using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xzy.SK.Domain.Common.Utils
{
    public class OpenAIHttpClientHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            UriBuilder uriBuilder;
            switch (request.RequestUri.LocalPath)
            {
                case "/v1/chat/completions":
                    uriBuilder = new UriBuilder(request.RequestUri)
                    {
                        // 这里是你要修改的 URL
                        Scheme = "https://ipsapro.isoftstone.com/",
                        Host = "ipsapro.isoftstone.com",
                        Path = "oneapi/v1/chat/completions",
                    };
                    request.RequestUri = uriBuilder.Uri;
                    break;
                case "/v1/embeddings":
                    uriBuilder = new UriBuilder(request.RequestUri)
                    {
                        // 这里是你要修改的 URL
                        Scheme = "https://ipsapro.isoftstone.com/",
                        Host = "ipsapro.isoftstone.com",
                        Path = "oneapi/v1/embeddings",
                    };
                    request.RequestUri = uriBuilder.Uri;

                    break;
            }

            // 接着，调用基类的 SendAsync 方法将你的修改后的请求发出去
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            return response;
        }
    }
}
