﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using InfluxData.Net.Common.Helpers;
using InfluxData.Net.Common.Infrastructure;
using InfluxData.Net.InfluxData.Helpers;
using InfluxData.Net.InfluxDb.Helpers;
using InfluxData.Net.InfluxDb.Models.Responses;
using InfluxData.Net.InfluxDb.RequestClients;

namespace InfluxData.Net.InfluxDb.ClientModules
{
    public class ClientModuleBase
    {
        protected IInfluxDbRequestClient RequestClient { get; private set; }

        protected IConfiguration Configuration { get; private set; }

        public ClientModuleBase(IInfluxDbRequestClient requestClient)
        {
            this.RequestClient = requestClient;
        }

        protected virtual async Task<IInfluxDataApiResponse> GetAndValidateQueryAsync(string query)
        {
            return await this.RequestAndValidateQueryAsync(query, HttpMethod.Get).ConfigureAwait(false);
        }

        protected virtual async Task<IInfluxDataApiResponse> PostAndValidateQueryAsync(string query)
        {
            return await this.RequestAndValidateQueryAsync(query, HttpMethod.Post).ConfigureAwait(false);
        }

        protected virtual async Task<IInfluxDataApiResponse> RequestAndValidateQueryAsync(string query, HttpMethod method)
        {
            var response = await this.RequestClient.QueryAsync(query, method).ConfigureAwait(false);
            response.ValidateQueryResponse(this.RequestClient.Configuration.ThrowOnWarning);

            return response;
        }

        protected virtual async Task<IInfluxDataApiResponse> GetAndValidateQueryAsync(string dbName, string query)
        {
            return await this.RequestAndValidateQueryAsync(dbName, query, HttpMethod.Get).ConfigureAwait(false);
        }

        protected virtual async Task<IInfluxDataApiResponse> PostAndValidateQueryAsync(string dbName, string query)
        {
            return await this.RequestAndValidateQueryAsync(dbName, query, HttpMethod.Post).ConfigureAwait(false);
        }

        protected virtual async Task<IInfluxDataApiResponse> RequestAndValidateQueryAsync(string dbName, string query, HttpMethod method)
        {
            var response = await this.RequestClient.QueryAsync(dbName, query, method).ConfigureAwait(false);
            response.ValidateQueryResponse(this.RequestClient.Configuration.ThrowOnWarning);

            return response;
        }

        protected virtual async Task<IEnumerable<Serie>> ResolveSingleGetSeriesResultAsync(string query)
        {
            var response = await this.RequestClient.GetQueryAsync(query).ConfigureAwait(false);
            var series = ResolveSingleGetSeriesResult(response);

            return series;
        }

        protected virtual async Task<IEnumerable<Serie>> ResolveSingleGetSeriesResultAsync(string dbName, string query)
        {
            var response = await this.RequestClient.GetQueryAsync(dbName, query).ConfigureAwait(false);
            var series = ResolveSingleGetSeriesResult(response);

            return series;
        }

        protected virtual IEnumerable<Serie> ResolveSingleGetSeriesResult(IInfluxDataApiResponse response)
        {
            var queryResponse = response.ReadAs<QueryResponse>().Validate(this.RequestClient.Configuration.ThrowOnWarning);
            var result = queryResponse.Results.Single();
            Validate.IsNotNull(result, "result");

            var series = result.Series != null ? result.Series.ToList() : new List<Serie>();

            return series;
        }

        protected virtual async Task<IEnumerable<SeriesResult>> ResolveGetSeriesResultAsync(string dbName, string query)
        {
            var response = await this.RequestClient.GetQueryAsync(dbName, query).ConfigureAwait(false);
            return response.ReadAs<QueryResponse>().Validate(this.RequestClient.Configuration.ThrowOnWarning).Results;
        }

        protected virtual async Task<IEnumerable<Serie>> ResolveSingleGetSeriesResultChunkedAsync(string dbName, string query, long chunkSize)
        {
            var response = await this.RequestClient.GetQueryChunkedAsync(dbName, query, chunkSize).ConfigureAwait(false);
            var series = ResolveSingleGetSeriesResultChunked(response);

            return series;
        }

        protected virtual IEnumerable<Serie> ResolveSingleGetSeriesResultChunked(IInfluxDataApiResponse response)
        {
            //Split response body for individual chunks
            var responseBodies = response.Body.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var series = new List<Serie>();
            foreach (var responseBody in responseBodies)
            {
                var queryResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<QueryResponse>(responseBody).Validate(this.RequestClient.Configuration.ThrowOnWarning);
                var result = queryResponse.Results.Single();
                Validate.IsNotNull(result, "result");
                if (result.Series != null)
                {
                    series.AddRange(result.Series.ToList());
                }
            }
            return series;
        }


        protected virtual async Task<IEnumerable<SeriesResult>> ResolveGetSeriesResultChunkedAsync(string dbName, string query, long chunkSize)
        {
            var response = await this.RequestClient.GetQueryChunkedAsync(dbName, query, chunkSize).ConfigureAwait(false);
            var responseBodies = response.Body.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var results = new List<SeriesResult>();
            foreach (var responseBody in responseBodies)
            {
                var queryResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<QueryResponse>(responseBody).Validate(this.RequestClient.Configuration.ThrowOnWarning);
                results.AddRange(queryResponse.Results);
            }
            return results;
        }
    }
}
