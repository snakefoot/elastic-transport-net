// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

#if !NETSTANDARD2_0 && !NETFRAMEWORK
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Elastic.Transport
{
	public abstract partial class PostData
	{
		/// <summary>
		/// Create a <see cref="PostData"/> instance that will write the <paramref name="bytes"/> to the output <see cref="Stream"/>
		/// </summary>
		public static PostData ReadOnlyMemory(ReadOnlyMemory<byte> bytes) => new PostDataReadOnlyMemory(bytes);

		private class PostDataReadOnlyMemory : PostData
		{
			private readonly ReadOnlyMemory<byte> _memoryOfBytes;

			protected internal PostDataReadOnlyMemory(ReadOnlyMemory<byte> item)
			{
				_memoryOfBytes = item;
				Type = PostType.ReadOnlyMemory;
			}

			public override void Write(Stream writableStream, ITransportConfiguration settings, bool disableDirectStreaming)
			{
				if (_memoryOfBytes.IsEmpty) return;

				MemoryStream? buffer = null;

				if (!disableDirectStreaming)
					writableStream.Write(_memoryOfBytes.Span);
				else
				{
					WrittenBytes ??= _memoryOfBytes.Span.ToArray();
					buffer = settings.MemoryStreamFactory.Create(WrittenBytes);
				}
				FinishStream(writableStream, buffer, disableDirectStreaming);
			}

			public override async Task WriteAsync(Stream writableStream, ITransportConfiguration settings, bool disableDirectStreaming, CancellationToken cancellationToken)
			{
				if (_memoryOfBytes.IsEmpty) return;

				MemoryStream? buffer = null;

				if (!disableDirectStreaming)
					writableStream.Write(_memoryOfBytes.Span);
				else
				{
					WrittenBytes ??= _memoryOfBytes.Span.ToArray();
					buffer = settings.MemoryStreamFactory.Create(WrittenBytes);
				}

				await FinishStreamAsync(writableStream, buffer, disableDirectStreaming, cancellationToken).ConfigureAwait(false);
			}
		}
	}
}
#endif
