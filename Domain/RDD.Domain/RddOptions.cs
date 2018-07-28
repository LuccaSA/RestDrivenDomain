namespace RDD.Domain
{
    public class RddOptions
    {
        /// <summary>
        /// Defines the maximum allowed items per page
        /// </summary>
        public int MaximumItemsPerPage { get; set; } = 1000;

        /// <summary>
        /// Defines the default number of items per page 
        /// </summary>
        public int DefaultItemsPerPage { get; set; } = 100;
    }
}
