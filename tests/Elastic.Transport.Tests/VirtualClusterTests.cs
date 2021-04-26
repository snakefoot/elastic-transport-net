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

using System.Linq;
using System.Threading.Tasks;
using Elastic.Transport.VirtualizedCluster;
using Elastic.Transport.VirtualizedCluster.Audit;
using Elastic.Transport.VirtualizedCluster.Rules;
using FluentAssertions;
using Xunit;
using static Elastic.Transport.Diagnostics.Auditing.AuditEvent;

namespace Elastic.Transport.Tests
{
	public class VirtualClusterTests
	{
		[Fact] public async Task ThrowsExceptionWithNoRules()
		{
			var audit = new Auditor(() => Virtual.Elasticsearch
				.Bootstrap(1)
				.StaticConnectionPool()
				.Settings(s => s.DisablePing().EnableDebugMode())
			);
			var e = await Assert.ThrowsAsync<UnexpectedTransportException>(
				async () => await audit.TraceCalls(new ClientCall()));

			e.Message.Should().Contain("No ClientCalls defined for the current VirtualCluster, so we do not know how to respond");
		}

		[Fact] public async Task ThrowsExceptionAfterDepleedingRules()
		{
			var audit = new Auditor(() => Virtual.Elasticsearch
				.Bootstrap(1)
				.ClientCalls(r => r.Succeeds(TimesHelper.Once).ReturnResponse(new { x = 1 }))
				.StaticConnectionPool()
				.Settings(s => s.DisablePing().EnableDebugMode())
			);
			audit = await audit.TraceCalls(
				new ClientCall {

					{ HealthyResponse, 9200, response =>
					{
						response.ApiCall.Success.Should().BeTrue();
						response.ApiCall.HttpStatusCode.Should().Be(200);
						response.ApiCall.DebugInformation.Should().Contain("x\":1");
					} },
				}
			);
			var e = await Assert.ThrowsAsync<UnexpectedTransportException>(
				async () => await audit.TraceCalls(new ClientCall()));

			e.Message.Should().Contain("No global or port specific ClientCalls rule (9200) matches any longer after 2 calls in to the cluster");
		}

		[Fact] public async Task AGlobalRuleStaysValidForever()
		{
			var audit = new Auditor(() => Virtual.Elasticsearch
				.Bootstrap(1)
				.ClientCalls(c=>c.SucceedAlways())
				.StaticConnectionPool()
				.Settings(s => s.DisablePing())
			);

			_ = await audit.TraceCalls(
				Enumerable.Range(0, 1000)
					.Select(i => new ClientCall { { HealthyResponse, 9200}, })
					.ToArray()
			);

		}

		[Fact] public async Task RulesAreIgnoredAfterBeingExecuted()
		{
			var audit = new Auditor(() => Virtual.Elasticsearch
				.Bootstrap(1)
				.ClientCalls(r => r.Succeeds(TimesHelper.Once).ReturnResponse(new { x = 1 }))
				.ClientCalls(r => r.Fails(TimesHelper.Once, 500).ReturnResponse(new { x = 2 }))
				.ClientCalls(r => r.Fails(TimesHelper.Twice, 400).ReturnResponse(new { x = 3 }))
				.ClientCalls(r => r.Succeeds(TimesHelper.Once).ReturnResponse(new { x = 4 }))
				.StaticConnectionPool()
				.Settings(s => s.DisablePing().EnableDebugMode())
			);
			_ = await audit.TraceCalls(
				new ClientCall {

					{ HealthyResponse, 9200, response =>
					{
						response.ApiCall.Success.Should().BeTrue();
						response.ApiCall.HttpStatusCode.Should().Be(200);
						response.ApiCall.DebugInformation.Should().Contain("x\":1");
					} },
				},
				new ClientCall {

					{ BadResponse, 9200, response =>
					{
						response.ApiCall.Success.Should().BeFalse();
						response.ApiCall.HttpStatusCode.Should().Be(500);
						response.ApiCall.DebugInformation.Should().Contain("x\":2");
					} },
				},
				new ClientCall {

					{ BadResponse, 9200, response =>
					{
						response.ApiCall.HttpStatusCode.Should().Be(400);
						response.ApiCall.DebugInformation.Should().Contain("x\":3");
					} },
				},
				new ClientCall {

					{ BadResponse, 9200, response =>
					{
						response.ApiCall.HttpStatusCode.Should().Be(400);
						response.ApiCall.DebugInformation.Should().Contain("x\":3");
					} },
				},
				new ClientCall {

					{ HealthyResponse, 9200, response =>
					{
						response.ApiCall.HttpStatusCode.Should().Be(200);
						response.ApiCall.DebugInformation.Should().Contain("x\":4");
					} },
				}
			);
		}
	}
}
