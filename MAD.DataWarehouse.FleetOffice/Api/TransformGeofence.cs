using MIFCore.Hangfire.APIETL;
using MIFCore.Hangfire.APIETL.Transform;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace MAD.DataWarehouse.FleetOffice.Api
{
    [ApiEndpoint("public-api/geofences")]
    internal class TransformGeofence : ITransformModel, IParseResponse
    {
        public Task<IEnumerable<IDictionary<string, object>>> OnParse(ParseResponseArgs args)
        {
            var json = JsonConvert.DeserializeObject<IEnumerable<ExpandoObject>>(args.ApiData.Data, new ExpandoObjectConverter());
            var enumerable = json as IEnumerable<IDictionary<string, object>>;

            return Task.FromResult(enumerable);
        }

        public Task OnTransformModel(TransformModelArgs args)
        {
            args.Transform.Object.FlattenGraph();
            return Task.CompletedTask;
        }
    }
}
