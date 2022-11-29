using OAuthB0ner.Storage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MAD.DataWarehouse.FleetOffice
{
    internal class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly ITokenStorage tokenStorage;

        public AuthenticationDelegatingHandler(ITokenStorage tokenStorage)
        {
            this.tokenStorage = tokenStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await this.tokenStorage.GetAccessToken();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}