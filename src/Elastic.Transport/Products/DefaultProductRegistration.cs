/*
 * Licensed to Elasticsearch B.V. under one or more contributor
 * license agreements. See the NOTICE file distributed with
 * this work for additional information regarding copyright
 * ownership. Elasticsearch B.V. licenses this file to you under
 * the Apache License, Version 2.0 (the "License"); you may
 * not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Elastic.Transport.Products
{
	/// <summary>
	/// A default non-descriptive product registration that does not support sniffing and pinging.
	/// Can be used to connect to unknown services before they develop their own <see cref="IProductRegistration"/>
	/// implementations
	/// </summary>
	public class ProductRegistration : IProductRegistration
	{
		/// <summary> A static instance of <see cref="ProductRegistration"/> to promote reuse </summary>
		public static ProductRegistration Default { get; } = new ProductRegistration();

		/// <inheritdoc cref="IProductRegistration.Name"/>
		public string Name { get; } = "elastic-transport-net";

		/// <inheritdoc cref="IProductRegistration.SupportsPing"/>
		public bool SupportsPing { get; } = false;

		/// <inheritdoc cref="IProductRegistration.SupportsSniff"/>
		public bool SupportsSniff { get; } = false;

		/// <inheritdoc cref="IProductRegistration.SniffOrder"/>
		public int SniffOrder(Node node) => -1;

		/// <inheritdoc cref="IProductRegistration.NodePredicate"/>
		public bool NodePredicate(Node node) => true;

		/// <inheritdoc cref="IProductRegistration.HttpStatusCodeClassifier"/>
		public bool HttpStatusCodeClassifier(HttpMethod method, int statusCode) =>
			statusCode >= 200 && statusCode < 300;

		/// <inheritdoc cref="IProductRegistration.TryGetServerErrorReason{TResponse}"/>>
		public bool TryGetServerErrorReason<TResponse>(TResponse response, out string reason)
			where TResponse : ITransportResponse
		{
			reason = null;
			return false;
		}

		/// <inheritdoc cref="IProductRegistration.CreateSniffRequestData"/>
		public RequestData CreateSniffRequestData(Node node, IRequestConfiguration requestConfiguration, ITransportConfiguration settings, IMemoryStreamFactory memoryStreamFactory) =>
			throw new NotImplementedException();

		/// <inheritdoc cref="IProductRegistration.SniffAsync"/>
		public Task<Tuple<IApiCallDetails, IReadOnlyCollection<Node>>> SniffAsync(IConnection connection, bool forceSsl, RequestData requestData, CancellationToken cancellationToken) =>
			throw new NotImplementedException();

		/// <inheritdoc cref="IProductRegistration.Sniff"/>
		public Tuple<IApiCallDetails, IReadOnlyCollection<Node>> Sniff(IConnection connection, bool forceSsl, RequestData requestData) =>
			throw new NotImplementedException();

		/// <inheritdoc cref="IProductRegistration.CreatePingRequestData"/>
		public RequestData CreatePingRequestData(Node node, RequestConfiguration requestConfiguration, ITransportConfiguration global, IMemoryStreamFactory memoryStreamFactory) =>
			throw new NotImplementedException();

		/// <inheritdoc cref="IProductRegistration.PingAsync"/>
		public Task<IApiCallDetails> PingAsync(IConnection connection, RequestData pingData, CancellationToken cancellationToken) =>
			throw new NotImplementedException();

		/// <inheritdoc cref="IProductRegistration.Ping"/>
		public IApiCallDetails Ping(IConnection connection, RequestData pingData) =>
			throw new NotImplementedException();
	}
}
