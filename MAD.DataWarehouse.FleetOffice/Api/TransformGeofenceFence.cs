using MIFCore.Hangfire.APIETL;
using MIFCore.Hangfire.APIETL.Transform;
using System.Threading.Tasks;

namespace MAD.DataWarehouse.FleetOffice.Api
{
    [ApiEndpoint("public-api/geofences", "fence")]
    internal class TransformGeofenceFence : ITransformModel
    {
        public Task OnTransformModel(TransformModelArgs args)
        {
            var transform = args.Transform;
            var objectToTransform = transform.Object;

            objectToTransform.Add("GeofenceId", transform.GraphObjectSet.Parent["id"]);
            objectToTransform.Add("Index", transform.GraphObjectSet.Objects.IndexOf(objectToTransform));

            return Task.CompletedTask;
        }
    }
}
