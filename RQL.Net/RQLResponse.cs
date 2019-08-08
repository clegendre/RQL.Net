using System.Collections.Generic;

namespace RQL.Net
{
    public class RQLResponse
    {
        internal RQLResponse( RQLRequestBatch batch, string jsonResult, bool success )
        {
            Success = success;
            Batch = batch;
            JsonResponse = jsonResult;
            if( success )
            {
                Results = Newtonsoft.Json.JsonConvert.DeserializeObject<IReadOnlyList<object[][]>>( jsonResult );
            }
        }

        /// <summary>
        /// Gets whether a result has been obtained or not
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the raw json Response
        /// </summary>
        public string JsonResponse { get; }

        /// <summary>
        /// Gets the original <see cref="RQLRequestBatch"/> request sent and build
        /// </summary>
        public RQLRequestBatch Batch { get; }

        /// <summary>
        /// Gets the result of the RQL requests, in the order of the batch.
        /// </summary>
        public IReadOnlyList<object[][]> Results { get; }
    }
}
