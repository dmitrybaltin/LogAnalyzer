// LogAnalyser.cs

using System.Text;
using Cysharp.IO;

namespace LogAnalyzer;

public class LogAnalyser
{
    /// <summary>
    /// Analyzed log file
    /// </summary>
    private readonly string _logFileName;
    
    /// <summary>
    /// Collection of unique endpoints
    /// Used as custom collection IndexedHashSet that assign an integer ID to every endpoint 
    /// These Id is used inside _users collection as indices to store endpoints stats
    /// </summary>
    private readonly IndexedHashSet _uniqueEndpoints;

    /// <summary>
    /// Collection of all the found users
    /// </summary>
    private readonly Dictionary<string, User> _users;
    
    private LogAnalyser(string logFileName)
    {
        _logFileName = logFileName;
        _users = new Dictionary<string, User>(1000000);
        _uniqueEndpoints = new IndexedHashSet();
    }
    
    public static async Task AnalyzeAsync(string logFileName, string resultFileName)
    {
        var analyzer = new LogAnalyser(logFileName);

        Console.WriteLine($"Analyzing is started for the file '{logFileName}' at {DateTime.Now}");

        //analyzer.CountLines(new ProgressPrinter("Lines counting test", 1));        
        
        //analyzer.CountAndDeserializeLines(new ProgressPrinter("Lines counting and deserializing test", 1));        
        
        //await analyzer.FastCountLinesAsync(new ProgressPrinter("Fast lines counting test using Utf8StreamReader", 1));   //Not used because the result is the same as using common reader
        
        analyzer.PreAnalyzeLog(new ProgressPrinter("Preprocessing", 1));

        analyzer.MainAnalyzeLog(new ProgressPrinter("Main processing", 1));
            
        analyzer.SaveDetailedStats(resultFileName, 1000, new ProgressPrinter("Saving detailed stats", 1));
            
        Console.WriteLine("Everything is complete");
    }

    /// <summary>
    /// Helping method used to profile the speed of reading the source file  
    /// </summary>
    /// <param name="progressPrinter"></param>
    private void CountLines(ProgressPrinter progressPrinter)
    {
        var totalBytes = new FileInfo(_logFileName).Length;
        
        using var fileStream = new FileStream(
            _logFileName, 
            FileMode.Open, 
            FileAccess.Read, 
            FileShare.Read, 
            16 * 1024 * 1024);
        
        using var reader = new StreamReader(fileStream);

        long lineCounter = 0;
        long bytesCounter = 0;
        
        while (reader.ReadLine() is { } line)
        {
            lineCounter++;
            bytesCounter+= Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length;
            var progress = MathF.Round(100 * (float)bytesCounter / totalBytes, 2);
            progressPrinter.PrintProgress(progress, $"{lineCounter} lines");
        }
        
        progressPrinter.Finish($"{lineCounter} lines processed");
    }

    /// <summary>
    /// One more helping method used to profile the speed of reading the source file  
    /// </summary>
    /// <param name="progressPrinter"></param>
    private void CountAndDeserializeLines(ProgressPrinter progressPrinter)
    {
        var totalBytes = new FileInfo(_logFileName).Length;
        
        using var fileStream = new FileStream(
            _logFileName, 
            FileMode.Open, 
            FileAccess.Read, 
            FileShare.Read, 
            16 * 1024 * 1024);
        
        using var reader = new StreamReader(fileStream);

        long lineCounter = 0;
        long bytesCounter = 0;
        
        while (reader.ReadLine() is { } line)
        {
            var fields = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            lineCounter++;
            bytesCounter+= Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length;
            var progress = MathF.Round(100 * (float)bytesCounter / totalBytes, 2);
            progressPrinter.PrintProgress(progress, $"{lineCounter} lines");
        }
        
        progressPrinter.Finish($"{lineCounter} lines processed");
    }

    /// <summary>
    /// Obe more helping method used to profile the speed of reading the source file
    /// Utf8StreamReader is used here but the result is the same as common Reader  
    /// </summary>
    /// <param name="progressPrinter"></param>
    private async Task FastCountLinesAsync(ProgressPrinter progressPrinter)
    {
        var totalBytes = new FileInfo(_logFileName).Length;
        
        var reader = new Utf8StreamReader(_logFileName);

        long lineCounter = 0;
        long bytesCounter = 0;
        
        // Most performant style, similar as System.Threading.Channels
        while (await reader.LoadIntoBufferAsync())
        {
            while (reader.TryReadLine(out ReadOnlyMemory<byte> line))
            {
                lineCounter++;
                bytesCounter += line.Length + Environment.NewLine.Length;
                var progress = MathF.Round(100 * (float)bytesCounter / totalBytes, 2);
                progressPrinter.PrintProgress(progress, $"{lineCounter} lines");
            }
        }
        progressPrinter.Finish($"");
    }
    
    /// <summary>
    /// The first stage of analyzing log file. 
    /// Creates a collection of users and allocate memory for all the endpoints inside them.
    /// </summary>
    /// <param name="progressPrinter"></param>
    private void PreAnalyzeLog(ProgressPrinter progressPrinter)
    {
        var totalBytes = new FileInfo(_logFileName).Length;
        
        using var fileStream = new FileStream(
            _logFileName, 
            FileMode.Open, 
            FileAccess.Read, 
            FileShare.Read, 
            160 * 1024 * 1024);
        
        using var reader = new StreamReader(fileStream);

        long lineCounter = 0;
        long bytesCounter = 0;

        while (reader.ReadLine() is { } line)
        {
            var fields = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            _users.GetOrAdd(fields[1], 1);
            _uniqueEndpoints.AddAndGetId(fields[2]);
            
            lineCounter++;
            bytesCounter+= Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length;
            var progress = MathF.Round(100 * (float)bytesCounter/totalBytes, 2);
            progressPrinter.PrintProgress(progress, $"{lineCounter} lines");
        }

        Console.WriteLine("Resizing Endpoints collection");

        foreach (var user in _users)
            user.Value.Endpoints.Resize(_uniqueEndpoints.Count);
        
        progressPrinter.Finish();
    }
    
    /// <summary>
    /// Read the log  an calculate required
    /// It's recommended to call it after PreAnalyzeLog to avoid extra allocation and GC launching     
    /// </summary>
    /// <param name="progressPrinter"></param>
    private void MainAnalyzeLog(ProgressPrinter progressPrinter)
    {
        var totalBytes = new FileInfo(_logFileName).Length;
        
        using var fileStream = new FileStream(
            _logFileName, 
            FileMode.Open, 
            FileAccess.Read, 
            FileShare.Read, 
            160 * 1024 * 1024);
        
        using var reader = new StreamReader(fileStream);

        long lineCounter = 0;
        long bytesCounter = 0;
        
        while (reader.ReadLine() is { } line)
        {
            var fields = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            var user = _users.GetOrAdd(fields[1], 100);
            var endpointId = _uniqueEndpoints.AddAndGetId(fields[2]);
            var statusCode = fields[3];

            user.RecalculateEndpointStats(endpointId, statusCode);
            
            lineCounter++;
            bytesCounter+= Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length;
            var progress = MathF.Round(100 * (float)bytesCounter/totalBytes, 2);
            progressPrinter.PrintProgress(progress, $"{lineCounter} lines");
        }
        
        progressPrinter.Finish($"{lineCounter} lines processed");
    }

    private void SaveShortStats(TextWriter writer)
    {
        double sumEndpoints = 0;
        foreach (var user in _users)
            sumEndpoints += user.Value.Endpoints.Length;

        var avgEndpoints = _users.Count > 0 ? sumEndpoints / _users.Count : 0;

        writer.WriteLine($"Found unique users: {_users.Count}, unique endpoints: {_uniqueEndpoints.Count}, Avg endpoints per user: {avgEndpoints}");
    }
    
    /// <summary>
    /// Save the results as tab separated file
    /// </summary>
    /// <param name="resultFileName"></param>
    /// <param name="batchSize"></param>
    /// <param name="progressPrinter"></param>
    private  void SaveDetailedStats(string resultFileName, int batchSize, ProgressPrinter progressPrinter)
    {
        using var writer = new StreamWriter(resultFileName, append: false, encoding: Encoding.UTF8, bufferSize: 8192);
        
        var batch = new List<string>(batchSize);

        batch.Add($"UID\tendpoint\terror_ratio");

        var userIndex = 0;
        foreach (var user in _users)
        {
            for (int ednpointId = 0; ednpointId < user.Value.Endpoints.Length; ednpointId++)
            {
                var endpoint = user.Value.Endpoints[ednpointId];
                var successful = endpoint.Entries - endpoint.Errors;

                var ratio = successful != 0
                    ? ((float)endpoint.Errors / successful).ToString()
                    : "inf";

                // Ad the line to the batch
                var line = $"{user.Key}\t{_uniqueEndpoints.GetValue(ednpointId)}\t{ratio}"; 
                batch.Add(line);

                // If the batch is full, then save it to the disk
                if (batch.Count == batchSize)
                {
                    writer.Write(string.Join(Environment.NewLine, batch) + Environment.NewLine);
                    batch.Clear();
                }

                progressPrinter.PrintProgress(MathF.Round(100 * (float)userIndex / _users.Count, 2));
            }
            userIndex++;
        }

        //Save unprocessed lines from the batch
        if (batch.Count > 0)
            writer.Write(string.Join(Environment.NewLine, batch) + Environment.NewLine);
        
        progressPrinter.Finish();
    }
}
