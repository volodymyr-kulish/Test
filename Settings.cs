namespace ConsoleApp1
{
    public static class Settings
    {
        public static class Source
        {
            public const string BasePath = @"D:\Test\";
            public const string InputFileName = "input_big.txt";
            public const string OutputFileName = "output.txt";
            public const string SplitedFileNameFormat = @"D:\Test\split{0:d5}.dat";
        }

        public static class General
        {
            public const int MaxSplitedFileSize = 100_000_000;

            public const int RecordSize = 100; // estimated record size
            public const int Records = 10_000_000; // estimated total # records
            public const int MaxUsage = 500_000_000; // max memory usage
        }
    }
}
