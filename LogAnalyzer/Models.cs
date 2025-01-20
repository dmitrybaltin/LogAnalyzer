// Models.cs

using System.Runtime.InteropServices;

namespace LogAnalyzer;

/// <summary>
/// User stats
/// </summary>
public class User
{
    /// <summary>
    /// Optimized array of the endpoints stats
    /// Integer identifiers are used as indices instead of original strings values
    /// Also used a custom collection ExpandableArray that is auto-resized if used index is more then the len of the array    
    /// </summary>
    public readonly ExpandableArray<Endpoint> Endpoints;

    public User(int endpointsCount)
    {
        Endpoints = new(endpointsCount);
    }
    
    /// <summary>
    /// Recalculate Add the line of log file to the user stats.
    /// It instantly used to calculate endpoint stats.  
    /// </summary>
    /// <param name="endpointId"></param>
    /// <param name="statusCode"></param>
    public void RecalculateEndpointStats(int endpointId, string statusCode)
    {
        var endpoint = Endpoints[endpointId];

        endpoint.Entries++;
        if (statusCode[0] != '2')
            endpoint.Errors++;

        Endpoints[endpointId] = endpoint;
    }
}

/// <summary>
/// Optimized endpoint data
/// I can suppose that 2 bytes per each value is enough for given source conditions
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 4)] 
public struct Endpoint
{
    [FieldOffset(0)] public ushort Entries;
    [FieldOffset(2)] public ushort Errors;
}

public static class UsersCollectionExtensions
{
    /// <summary>
    /// Tries to get an existing user from the dictionary, or adds a new user if not found.
    /// </summary>
    /// <param name="users">The dictionary of users, where the key is the user's name and the value is the User object.</param>
    /// <param name="userName">The name of the user to retrieve or add.</param>
    /// <param name="minEndpoints">The minimum number of endpoints to initialize a new user with, if they need to be added.</param>
    /// <returns>The existing user if found; otherwise, a new User object with the specified minimum endpoints.</returns>
    public static User GetOrAdd(this Dictionary<string, User> users, string userName, int minEndpoints)
    {
        if (!users.TryGetValue(userName, out var user))
        {
            user = new User(minEndpoints);
            users.Add(userName, user);
        }
        return user;
    }
}
