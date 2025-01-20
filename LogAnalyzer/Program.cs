// Usage:
// 2. Run it from the command line providing the log file path as the first argument and the result file path as the second argument.
//    Example: LogAnalyzer.exe path_from/logfile.txt path_to/resultfile.txt
// 3. The program will output the result file with the calculated error-to-success ratio for each UID-endpoint pair.
//
//	Task:
//	You have a log file several hundred gigabytes in size
//	The lines in the file have the following format: IP - UID - endpoint - status - execution time in ms
//
//		File fragment: 
//		153.12.279.13 hGsd8sdk /admin 200 121
//		153.12.279.13 hGsd8sdk /user/contacts 400 10
//		153.12.279.31 hGsd8sdk /user/contacts 400 10
//		13.0.163.102 lxY7nKxl /user/contacts 200 452
//		13.0.163.102 lxY7nKxl /messages 403 10
//
//	For the following file you need for each pair of UID - endpoint display the ratio of error log entries to successful log entries
//
//  Additional conditions:
//  1. <=1000000 unique users
//  2. <=5000 unique endpoints
//  2. <=1000 unique endpoints per each user

namespace LogAnalyzer;

public static class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length > 0)
        {
            //await LogAnalyser.AnalyzeAsync(args[0], args[1]);
            await LogAnalyser.AnalyzeAsync("generated_log_big.txt", "result_big.txt");
        }
        else
        {
            Console.WriteLine("Please provide valid arguments.");
            Console.WriteLine("Example: LogAnalyzer.exe path_from/logfile.txt path_to/resultfile.txt");
        }
    }
}
