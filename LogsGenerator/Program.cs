namespace LogsGenerator;

public class LogFileGenerator
{
    // List of possible HTTP status codes
    private static readonly List<int> StatusCodes = new List<int> { 200, 400, 403, 404, 500 };

    // Log file generation
    public static void GenerateLogFile(string fileName, int userCount, int endpointCount, long totalLines, int batchSize = 10000)
    {
        Random random = new Random();

        // Generating unique users
        var users = new List<string>();
        for (int i = 0; i < userCount; i++)
        {
            users.Add($"user_{i + 1}");
        }

        // Generating unique endpoints
        var endpoints = new List<string>();
        for (int i = 0; i < endpointCount; i++)
        {
            endpoints.Add($"/endpoint_{i + 1}");
        }

        // Batch size
        double lastLoggedProgress = 0;

        // Opening a stream to write to the file
        using (StreamWriter writer = new StreamWriter(fileName))
        {
            var batch = new List<string>(batchSize);
            for (long i = 0; i < totalLines; i++)
            {
                // Selecting a random user and endpoint
                var user = users[random.Next(userCount)];
                var endpoint = endpoints[random.Next(endpointCount)];

                // Randomly generating status and execution time
                var status = StatusCodes[random.Next(StatusCodes.Count)];
                var executionTime = random.Next(1, 1000); // Execution time in milliseconds (from 1 to 1000)

                // Forming the log line
                string logLine = $"{GenerateRandomIp()} {user} {endpoint} {status} {executionTime}";

                // Adding the line to the batch
                batch.Add(logLine);

                // If the batch is full, write it to the file
                if (batch.Count == batchSize)
                {
                    writer.Write(string.Join(Environment.NewLine, batch) + Environment.NewLine);
                    batch.Clear();
                }

                // Logging the progress
                var progress = Math.Round(100 * (float)i / totalLines, 2);
                if (progress >= lastLoggedProgress + 1)
                {
                    Console.WriteLine($"Progress: {progress}%, time: {DateTime.Now}");
                    lastLoggedProgress = progress;
                }
            }
            // Write remaining lines if any
            if (batch.Count > 0)
            {
                writer.Write(string.Join(Environment.NewLine, batch) + Environment.NewLine);
                batch.Clear();
            }
        }

        Console.WriteLine($"Log file with {totalLines} lines successfully created: {fileName}");
    }


    // Generating a random IP address
    private static string GenerateRandomIp()
    {
        Random random = new Random();
        return $"{random.Next(1, 255)}.{random.Next(0, 255)}.{random.Next(0, 255)}.{random.Next(1, 255)}";
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Generation parameters
        string fileName = "generated_log.txt";
        int userCount = 1000000;  // Number of users
        int endpointCount = 3000;  // Number of endpoints
        long totalLines = 1000000000;  // Total number of lines in the file
        int batchSize = 10000;

        // Generating the log file
        LogFileGenerator.GenerateLogFile(fileName, userCount, endpointCount, totalLines, batchSize);
    }
}
