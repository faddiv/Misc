using BenchmarkDotNet.Attributes;

namespace SlowlyChangingDimensions
{
    [MemoryDiagnoser]
    public class Benchy
    {
        private DateTime middleDate;
        private ScdDbContext1 _db;
        public Benchy()
        {
        }

        [GlobalSetup]
        public void Setup()
        {
            middleDate = DateTime.Parse("2005.04.21");
            _db = new ScdDbContext1();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _db.Dispose();
        }

        [Benchmark(Baseline =true)]
        public List<ScdExampleData> ScdWithLeftJoinPast()
        {
            return _db.ScdExampleTable1
                .GroupJoin(_db.ScdExampleTable1, e => e.Id, e => e.PreviousVersionId, (e, n) => new { e, n })
                .SelectMany(s => s.n.DefaultIfEmpty(), (s, n) => new { s.e, NextVersion = n != null ? n.CreatedTimestamp : DateTime.MaxValue })
                .Where(e => e.e.CreatedTimestamp <= middleDate && middleDate < e.NextVersion)
                .Select(e => new ScdExampleData
                {
                    Id = e.e.Id,
                    CorrelationId = e.e.CorrelationId,
                    Data1 = e.e.Data1,
                    Data2 = e.e.Data2,
                    Data3 = e.e.Data3
                })
                .ToList();
        }

        [Benchmark]
        public List<ScdExampleData> ScdWithLeftJoinLast()
        {
            return _db.ScdExampleTable1
                .GroupJoin(_db.ScdExampleTable1, e => e.Id, e => e.PreviousVersionId, (e, n) => new { e, n })
                .SelectMany(s => s.n.DefaultIfEmpty(), (s, n) => new { s.e, NextVersion = n != null ? n.CreatedTimestamp : DateTime.MaxValue })
                .Where(e => e.e.CreatedTimestamp <= DateTime.Now && DateTime.Now < e.NextVersion)
                .Select(e => new ScdExampleData
                {
                    Id = e.e.Id,
                    CorrelationId = e.e.CorrelationId,
                    Data1 = e.e.Data1,
                    Data2 = e.e.Data2,
                    Data3 = e.e.Data3
                })
                .ToList();
        }

        [Benchmark]
        public List<ScdExampleData> ScdWithFromToPast()
        {
            return _db.ScdExampleTable2
                .Where(e => e.CreatedTimestamp <= middleDate && middleDate < e.EndTimestamp)
                .Select(e => new ScdExampleData
                {
                    Id = e.Id,
                    CorrelationId = e.CorrelationId,
                    Data1 = e.Data1,
                    Data2 = e.Data2,
                    Data3 = e.Data3
                })
                .ToList();
        }

        [Benchmark]
        public List<ScdExampleData> ScdWithFromToLast()
        {
            return _db.ScdExampleTable2
                .Where(e => e.CreatedTimestamp <= DateTime.Now && DateTime.Now < e.EndTimestamp)
                .Select(e => new ScdExampleData
                {
                    Id = e.Id,
                    CorrelationId = e.CorrelationId,
                    Data1 = e.Data1,
                    Data2 = e.Data2,
                    Data3 = e.Data3
                })
                .ToList();
        }

        [Benchmark]
        public List<ScdExampleData> ScdWithCurrentVersion()
        {
            return _db.ScdExampleTable2
                .Where(e => e.IsCurrentVersion)
                .Select(e => new ScdExampleData
                {
                    Id = e.Id,
                    CorrelationId = e.CorrelationId,
                    Data1 = e.Data1,
                    Data2 = e.Data2,
                    Data3 = e.Data3
                })
                .ToList();
        }
    }
}
