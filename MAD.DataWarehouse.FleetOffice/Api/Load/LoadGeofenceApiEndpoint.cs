using ETLBox.Connection;
using ETLBox.ControlFlow;
using ETLBox.ControlFlow.Tasks;
using MIFCore.Hangfire.APIETL;
using MIFCore.Hangfire.APIETL.Load;
using MIFCore.Hangfire.APIETL.Transform;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.DataWarehouse.FleetOffice.Api.Load
{
    [ApiEndpoint("public-api/geofences")]
    internal class LoadGeofenceApiEndpoint : IHandleResponse
    {
        private readonly AppConfig appConfig;

        public LoadGeofenceApiEndpoint(AppConfig appConfig)
        {
            this.appConfig = appConfig;
        }

        public async Task OnHandleResponse(HandleResponseArgs args)
        {
            // Get the response as a helpful ExpandoObject
            var json = JsonConvert.DeserializeObject<IEnumerable<ExpandoObject>>(args.ApiData.Data, new ExpandoObjectConverter());
            json.FlattenGraph();

            // Get all the different object sets and group them by the ParentKey
            var graphObjectSets = json.ExtractDistinctGraphObjectSets(
                new ExtractDistinctGraphObjectSetsExtensions.ExtractDistinctGraphObjectSetsArgs
                {
                    Transform = (args) =>
                    {
                        // Add missing foreign validKeyTypes for child objects
                        if (args.GraphObjectSet.ParentKey == "fence")
                        {
                            args.Object.Add("GeofenceId", args.GraphObjectSet.Parent["id"]);
                            args.Object.Add("Index", args.GraphObjectSet.Objects.IndexOf(args.Object));
                        }
                    }
                })
                .GroupBy(y => y.ParentKey)
                .ToList();

            var connMan = new SqlConnectionManager(this.appConfig.ConnectionString);

            // Now we know how many different entities to map into the database
            foreach (var set in graphObjectSets)
            {
                var apiEndpointModel = typeof(LoadGeofenceApiEndpoint)
                    .Assembly
                    .GetTypes()
                    .Select(y => y.GetApiEndpointModel())
                    .Where(y => y != null)
                    .FirstOrDefault(y => y?.EndpointName == args.Endpoint.Name && y?.InputPath == set.Key);

                // If there is no model mapping, do nothing.
                if (apiEndpointModel is null)
                    continue;

                TableDefinition tableDefinition;

                if (IfTableOrViewExistsTask.IsExisting(connMan, apiEndpointModel.DestinationName) == false)
                {
                    this.AutoMapMissingModelProperties(set, apiEndpointModel);

                    tableDefinition = new TableDefinition(
                        name: apiEndpointModel.DestinationName,
                        columns: apiEndpointModel.MappedProperties.Select(kvp =>
                        {
                            var (key, value) = kvp;
                            return new TableColumn(value.DestinationName, value.DestinationType, value.IsKey == false, value.IsKey, false);
                        }).ToList());

                    CreateTableTask.Create(connMan, tableDefinition);
                }
                else
                {
                    tableDefinition = TableDefinition.FromTableName(connMan, apiEndpointModel.DestinationName);
                }
            }
        }

        private void AutoMapMissingModelProperties(IGrouping<string, GraphObjectSet> set, ApiEndpointModel apiEndpointModel)
        {
            var allKeyTypes = set.GetKeyTypes();

            // Get the validKeyTypes and types from all objects with the same ParentKey
            var validKeyTypes = allKeyTypes

                // Filter out the validKeyTypes which have collections
                .Where(y => y.Value.Any(y => typeof(IEnumerable<object>).IsAssignableFrom(y)) == false)

                // Filter out the validKeyTypes which only have null values
                .Where(y => y.Value.All(y => y is null) == false)
                .ToList();

            // Now we have validKeyTypes which represent the destination table schema
            foreach (var (key, types) in validKeyTypes)
            {
                if (apiEndpointModel.MappedProperties.TryGetValue(key, out var apiEndpointModelProperty) == false)
                {
                    apiEndpointModelProperty = new ApiEndpointModelProperty
                    {
                        SourceName = key,
                        DestinationName = key,
                        DestinationType = this.GetDestinationType(key, types),
                        SourceType = types,
                        IsKey = false
                    };

                    apiEndpointModel.MappedProperties.Add(key, apiEndpointModelProperty);
                }
                else
                {
                    apiEndpointModelProperty.DestinationType = this.GetDestinationType(key, types);
                }
            }
        }

        private string GetDestinationType(string key, HashSet<Type> types)
        {
            var clrType = types.FirstOrDefault(y => y != null);

            return Type.GetTypeCode(clrType) switch
            {
                TypeCode.Empty => throw new NotImplementedException(),
                TypeCode.Object => throw new NotImplementedException(),
                TypeCode.DBNull => throw new NotImplementedException(),
                TypeCode.Boolean => "bit",
                TypeCode.Char => "char(max)",
                TypeCode.SByte or TypeCode.Byte => "binary",
                TypeCode.Int16 => "smallint",
                TypeCode.UInt16 => "smallint",
                TypeCode.Int32 or TypeCode.UInt32 => "int",
                TypeCode.Int64 or TypeCode.UInt64 => "bigint",
                TypeCode.Single => "real",
                TypeCode.Double => "float",
                TypeCode.Decimal => "decimal(18,4)",
                TypeCode.DateTime => "datetimeoffset",
                TypeCode.String => "nvarchar(max)",
                _ => throw new NotImplementedException(),
            };
        }
    }
}
