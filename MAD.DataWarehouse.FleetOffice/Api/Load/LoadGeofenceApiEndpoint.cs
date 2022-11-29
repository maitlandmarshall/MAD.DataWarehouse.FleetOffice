using MIFCore.Hangfire.APIETL;
using MIFCore.Hangfire.APIETL.Transform;
using System.Threading.Tasks;

namespace MAD.DataWarehouse.FleetOffice.Api.Load
{
    [ApiEndpoint("public-api/geofences")]
    internal class LoadGeofenceApiEndpoint : IHandleResponse
    {
        public Task OnHandleResponse(HandleResponseArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
