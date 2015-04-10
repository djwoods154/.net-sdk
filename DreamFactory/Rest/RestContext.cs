﻿namespace DreamFactory.Rest
{
    using System;
    using System.Threading.Tasks;
    using DreamFactory.Http;
    using DreamFactory.Model;
    using DreamFactory.Serialization;

    /// <inheritdoc />
    public class RestContext : IRestContext
    {
        private readonly HttpAddress address;

        private HttpHeaders httpHeaders;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestContext"/> class.
        /// </summary>
        /// <param name="baseAddress">Base address (URL).</param>
        /// <param name="apiVersion">REST API version to use.</param>
        public RestContext(string baseAddress, RestApiVersion apiVersion = RestApiVersion.V1)
            : this(baseAddress, new UnirestHttpFacade(), new JsonContentSerializer(), apiVersion)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestContext"/> class.
        /// </summary>
        /// <param name="baseAddress">Base address (URL).</param>
        /// <param name="httpFacade">User defined instance of <see cref="IHttpFacade"/>.</param>
        /// <param name="serializer">User defined instance of <see cref="IContentSerializer"/>.</param>
        /// <param name="apiVersion">REST API version to use.</param>
        public RestContext(string baseAddress, IHttpFacade httpFacade, IContentSerializer serializer, RestApiVersion apiVersion = RestApiVersion.V1)
        {
            HttpUtils.CheckUrlString(baseAddress);

            if (httpFacade == null)
            {
                throw new ArgumentNullException("httpFacade");
            }

            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            address = new HttpAddress(baseAddress, apiVersion);

            HttpFacade = httpFacade;
            ContentSerializer = serializer;

            SetBaseHeaders();

            Factory = new ServiceFactory(address, HttpFacade, ContentSerializer, httpHeaders);
        }

        /// <inheritdoc />
        public IHttpFacade HttpFacade { get; private set; }

        /// <inheritdoc />
        public IContentSerializer ContentSerializer { get; private set; }

        /// <inheritdoc />
        public IHttpHeaders BaseHeaders { get { return httpHeaders; } }

        /// <inheritdoc />
        public IServiceFactory Factory { get; private set; }

        /// <inheritdoc />
        public async Task<Services> GetServicesAsync()
        {
            IHttpRequest request = new HttpRequest(HttpMethod.Get, address.Build(), BaseHeaders);

            IHttpResponse response = await HttpFacade.SendAsync(request);
            HttpUtils.ThrowOnBadStatus(response, ContentSerializer);

            return ContentSerializer.Deserialize<Services>(response.Body);
        }

        /// <inheritdoc />
        public async Task<Resources> GetResourcesAsync(string serviceName)
        {
            IHttpRequest request = new HttpRequest(HttpMethod.Get, address.WithResources(serviceName).Build(), BaseHeaders);

            IHttpResponse response = await HttpFacade.SendAsync(request);
            HttpUtils.ThrowOnBadStatus(response, ContentSerializer);

            return ContentSerializer.Deserialize<Resources>(response.Body);
        }

        private void SetBaseHeaders()
        {
            httpHeaders = new HttpHeaders();
            httpHeaders.AddOrUpdate(HttpHeaders.DreamFactoryApplicationHeader, "admin");
            httpHeaders.AddOrUpdate(HttpHeaders.ContentTypeHeader, ContentSerializer.ContentType);
            httpHeaders.AddOrUpdate(HttpHeaders.AcceptHeader, ContentSerializer.ContentType);
        }
    }
}