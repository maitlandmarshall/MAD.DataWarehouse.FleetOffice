using MIFCore.Hangfire.APIETL.Load;
using System.ComponentModel.DataAnnotations;

namespace MAD.DataWarehouse.FleetOffice.Api.Load
{
    [ApiEndpointModel("public-api/geofences")]
    internal class Geofence
    {
        [Key]
        [ApiEndpointModelProperty("id")]
        public long Id { get; set; }
    }
}
