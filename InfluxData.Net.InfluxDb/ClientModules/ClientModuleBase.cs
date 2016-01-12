﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using InfluxData.Net.InfluxDb.Infrastructure;
using InfluxData.Net.InfluxDb.RequestClients;
using System.Threading.Tasks;
using InfluxData.Net.Common.Helpers;
using InfluxData.Net.InfluxDb.Models.Responses;
using InfluxData.Net.InfluxDb.Helpers;

namespace InfluxData.Net.InfluxDb.ClientModules
{
    public class ClientModuleBase
    {
        protected IInfluxDbRequestClient RequestClient { get; private set; }

        public ClientModuleBase(IInfluxDbRequestClient requestClient)
        {
            this.RequestClient = requestClient;
        }

        protected async Task<IInfluxDbApiResponse> GetAndValidateQueryAsync(string query)
        {
            var response = await this.RequestClient.QueryAsync(query);
            response.ValidateQueryResponse();

            return response;
        }

        protected async Task<IInfluxDbApiResponse> GetAndValidateQueryAsync(string dbName, string query)
        {
            var response = await this.RequestClient.QueryAsync(dbName, query);
            response.ValidateQueryResponse();

            return response;
        }

        protected async Task<IEnumerable<Serie>> ResolveSingleGetSeriesResultAsync(string query)
        {
            var response = await this.RequestClient.QueryAsync(query);
            var series = ResolveSingleGetSeriesResult(response);

            return series;
        }

        protected async Task<IEnumerable<Serie>> ResolveSingleGetSeriesResultAsync(string dbName, string query)
        {
            var response = await this.RequestClient.QueryAsync(dbName, query);
            var series = ResolveSingleGetSeriesResult(response);

            return series;
        }

        private IEnumerable<Serie> ResolveSingleGetSeriesResult(IInfluxDbApiResponse response)
        {
            var queryResponse = response.ReadAs<QueryResponse>().Validate();
            var result = queryResponse.Results.Single();
            Validate.IsNotNull(result, "result");

            var series = result.Series != null ? result.Series.ToList() : new List<Serie>();

            return series;
        }
    }
}
