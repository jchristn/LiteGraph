namespace LiteGraph.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Vector helper.
    /// </summary>
    public static class VectorHelper
    {
        /// <summary>
        /// Calculates the cosine similarity between two vectors
        /// </summary>
        /// <returns>Value between -1 and 1, where 1 means vectors are identical, 0 means orthogonal, -1 means opposite</returns>
        public static float CalculateCosineSimilarity(List<float> vectors1, List<float> vectors2)
        {
            if (vectors1 == null || vectors2 == null)
                throw new ArgumentNullException("Vectors cannot be null");

            if (vectors1.Count != vectors2.Count)
                throw new ArgumentException("Vectors must be of equal length");

            float dotProduct = CalculateInnerProduct(vectors1, vectors2);
            float magnitude1 = (float)Math.Sqrt(vectors1.Sum(x => x * x));
            float magnitude2 = (float)Math.Sqrt(vectors2.Sum(x => x * x));

            if (magnitude1 == 0 || magnitude2 == 0)
                throw new ArgumentException("Vector magnitudes cannot be zero");

            return dotProduct / (magnitude1 * magnitude2);
        }

        /// <summary>
        /// Calculates the cosine distance between two vectors
        /// </summary>
        /// <returns>Value between 0 and 2, where 0 means vectors are identical, 2 means opposite</returns>
        public static float CalculateCosineDistance(List<float> vectors1, List<float> vectors2)
        {
            return 1 - CalculateCosineSimilarity(vectors1, vectors2);
        }

        /// <summary>
        /// Calculates the Euclidean similarity between two vectors
        /// </summary>
        /// <returns>Value between 0 and 1, where 1 means vectors are identical</returns>
        public static float CalculateEuclidianSimilarity(List<float> vectors1, List<float> vectors2)
        {
            float distance = CalculateEuclidianDistance(vectors1, vectors2);
            return 1 / (1 + distance);  // Convert distance to similarity using inverse relationship
        }

        /// <summary>
        /// Calculates the Euclidean distance between two vectors
        /// </summary>
        /// <returns>Value >= 0, where 0 means vectors are identical</returns>
        public static float CalculateEuclidianDistance(List<float> vectors1, List<float> vectors2)
        {
            if (vectors1 == null || vectors2 == null)
                throw new ArgumentNullException("Vectors cannot be null");

            if (vectors1.Count != vectors2.Count)
                throw new ArgumentException("Vectors must be of equal length");

            float sumSquaredDifferences = 0;
            for (int i = 0; i < vectors1.Count; i++)
            {
                float diff = vectors1[i] - vectors2[i];
                sumSquaredDifferences += diff * diff;
            }

            return (float)Math.Sqrt(sumSquaredDifferences);
        }

        /// <summary>
        /// Calculates the inner (dot) product of two vectors
        /// </summary>
        /// <returns>The sum of the element-wise products</returns>
        public static float CalculateInnerProduct(List<float> vectors1, List<float> vectors2)
        {
            if (vectors1 == null || vectors2 == null)
                throw new ArgumentNullException("Vectors cannot be null");

            if (vectors1.Count != vectors2.Count)
                throw new ArgumentException("Vectors must be of equal length");

            return vectors1.Zip(vectors2, (a, b) => a * b).Sum();
        }
    }
}