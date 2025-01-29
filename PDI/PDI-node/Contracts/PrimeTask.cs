namespace PDI.Contracts
{ 
    public class PrimeTask
    {        
        public string TaskId { get; set; }
        
        public string BundleId { get; set; }

        public int RangeEnd { get; set; }

        public int RangeStart { get; set; }
        public int MaxProcessingTime { get; set; }
        
    }
}