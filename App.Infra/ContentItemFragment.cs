using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace AppInfra
{
    public class ContentItemFragment
    {
        [VectorStoreRecordKey(StoragePropertyName = "id")]        
        public string Id { get; set; }

        [VectorStoreRecordData(StoragePropertyName = "content_item_id")]
        public Guid ContentItemId { get; set; }

        [VectorStoreRecordVector(Dimensions: 1536, DistanceFunction.CosineDistance, StoragePropertyName = "embedding")]
        public ReadOnlyMemory<float>? Embedding { get; set; }

        [VectorStoreRecordData(StoragePropertyName = "content")]
        [TextSearchResultValue]
        public string Content { get; set; } = string.Empty;

        [VectorStoreRecordData(StoragePropertyName = "source")]
        [TextSearchResultName]
        public string Source { get; set; } = string.Empty;
    }
}
