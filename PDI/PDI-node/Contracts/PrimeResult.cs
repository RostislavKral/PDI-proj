namespace PDI.Contracts
{
    public class PrimeResult
    {
        public string TaskId { get; set; }
        public List<int> Primes { get; set; }
        public int? UnprocessedStart { get; set; }
        public int RangeEnd { get; set; }
        public string BundleId { get; set; }
    }
}