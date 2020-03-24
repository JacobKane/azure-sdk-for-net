﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.ChangeFeed.Models;

namespace Azure.Storage.Blobs.ChangeFeed
{
    internal class Shard
    {
        /// <summary>
        /// Container Client for listing Chunks.
        /// </summary>
        private readonly BlobContainerClient _containerClient;

        /// <summary>
        /// The path to this Shard.
        /// </summary>
        private readonly string _shardPath;

        /// <summary>
        /// Queue of the paths to Chunks we haven't processed.
        /// </summary>
        private readonly Queue<string> _chunks;

        /// <summary>
        /// The Chunk we are currently processing.
        /// </summary>
        private Chunk _currentChunk;

        /// <summary>
        /// The index of the Chunk we are processing.
        /// </summary>
        private long _chunkIndex;

        /// <summary>
        /// User provided Event Index.
        /// </summary>
        private long _eventIndex;

        /// <summary>
        /// If this Shard has been initalized.
        /// </summary>
        private bool _isInitialized;

        public Shard(
            BlobContainerClient containerClient,
            string shardPath,
            BlobChangeFeedShardCursor shardCursor = default)
        {
            _containerClient = containerClient;
            _shardPath = shardPath;
            _chunks = new Queue<string>();
            _isInitialized = false;
            _chunkIndex = shardCursor?.ChunkIndex ?? 0;
            _eventIndex = shardCursor?.EventIndex ?? 0;
        }

        private async Task Initalize(bool async)
        {
            // Get Chunks
            if (async)
            {
                await foreach (BlobHierarchyItem blobHierarchyItem in _containerClient.GetBlobsByHierarchyAsync(
                    prefix: _shardPath).ConfigureAwait(false))
                {
                    if (blobHierarchyItem.IsPrefix)
                        continue;

                    //Chunk chunk = new Chunk(_containerClient, blobHierarchyItem.Blob.Name);
                    _chunks.Enqueue(blobHierarchyItem.Blob.Name);
                }
            }
            else
            {
                foreach (BlobHierarchyItem blobHierarchyItem in _containerClient.GetBlobsByHierarchy(
                    prefix: _shardPath))
                {
                    if (blobHierarchyItem.IsPrefix)
                        continue;

                    //Chunk chunk = new Chunk(_containerClient, blobHierarchyItem.Blob.Name);
                    _chunks.Enqueue(blobHierarchyItem.Blob.Name);
                }
            }

            // Fast forward to current Chunk
            if (_chunkIndex > 0)
            {
                //TODO possible off by 1 error here.
                for (int i = 0; i < _chunkIndex; i++)
                {
                    _chunks.Dequeue();
                }
            }

            _currentChunk = new Chunk(_containerClient, _chunks.Dequeue(), _eventIndex);
            _isInitialized = true;
        }

        public BlobChangeFeedShardCursor GetCursor()
            => new BlobChangeFeedShardCursor(
                _chunkIndex,
                _currentChunk.EventIndex);

        public bool HasNext()
        {
            if (!_isInitialized)
            {
                return true;
            }

            return _chunks.Count > 0 || _currentChunk.HasNext();
        }

        public async Task<BlobChangeFeedEvent> Next(bool async)
        {
            if (!_isInitialized)
            {
                if (async)
                {
                    await Initalize(async: true).ConfigureAwait(false);
                }
                else
                {
                    Initalize(async: false).EnsureCompleted();
                }
            }

            if (!HasNext())
            {
                throw new InvalidOperationException("Shard doesn't have any more events");
            }

            BlobChangeFeedEvent changeFeedEvent;

            if (async)
            {
                changeFeedEvent = await _currentChunk.Next(async: true).ConfigureAwait(false);
            }
            else
            {
                changeFeedEvent = _currentChunk.Next(async: false).EnsureCompleted();
            }

            // Remove currentChunk if it doesn't have another event.
            if (!_currentChunk.HasNext() && _chunks.Count > 0)
            {
                _currentChunk = new Chunk(_containerClient, _chunks.Dequeue());
                _chunkIndex++;
            }
            return changeFeedEvent;
        }
    }
}