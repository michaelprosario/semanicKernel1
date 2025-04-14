﻿using Microsoft.Extensions.VectorData;

namespace AppInfra
{
    public class ContentItemFragment
    {
        [VectorStoreRecordKey]
        public Guid Id { get; set; }

        [VectorStoreRecordData(StoragePropertyName = "content_item_id")]
        public Guid ContentItemId { get; set; }

        [VectorStoreRecordVector(Dimensions: 4, DistanceFunction.CosineDistance)]
        public ReadOnlyMemory<float>? Embedding { get; set; }

        [VectorStoreRecordData(StoragePropertyName = "content")]
        public string Content { get; set; } = string.Empty;

        [VectorStoreRecordData(StoragePropertyName = "source")]
        public string Source { get; set; } = string.Empty;
    }
}
