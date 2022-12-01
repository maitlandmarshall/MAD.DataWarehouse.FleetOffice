using MIFCore.Hangfire.APIETL.Load;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAD.DataWarehouse.FleetOffice.Api.Models
{
    [ApiEndpointModel("public-api/geofences", InputPath = "fence")]
    [Table("GeofenceFence")]
    internal class GeofenceFence
    {
        [Key]
        public long GeofenceId { get; set; }

        [Key]
        public int Index { get; set; }
    }
}
