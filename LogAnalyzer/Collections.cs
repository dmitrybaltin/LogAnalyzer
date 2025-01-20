// Collections.cs

using System.Runtime.CompilerServices;

namespace LogAnalyzer;

/// <summary>
/// A dynamic array that automatically resizes itself when the array is accessed beyond its current capacity.
/// All the added new items are filled by default value of the type T.
/// The array grows by a factor defined by the expansion reserve.
/// </summary>
/// <typeparam name="T">The type of elements in the array.</typeparam>
public class ExpandableArray<T>
{
    // Internal array to hold the elements
    private T[] _array;

    // Factor by which the array size will expand when resized 
    private readonly float _expansionReserve;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="initialSize">The initial size of the array.</param>
    /// <param name="expansionReserve">The factor by which the array size will expand when resized. Default is 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the initial size is negative.</exception>
    public ExpandableArray(int initialSize, float expansionReserve = 1)
    {
        _expansionReserve = Math.Max(1, expansionReserve);
        
        if (initialSize < 0)
            throw new ArgumentOutOfRangeException(nameof(initialSize), "Size cannot be negative");

        _array = new T[initialSize];
    }

    /// <summary>
    /// Gets the current length of the array.
    /// </summary>
    public int Length => _array.Length;
    
    /// <summary>
    /// Resizes the array to the specified size, expanding it if needed
    /// </summary>
    /// <param name="size">The new size</param>
    public void Resize(int size)
    {
        if (size > _array.Length)
            Array.Resize(ref _array, (int)(_expansionReserve * size));
    }

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// Automatically resizes the array if the index is out of bounds.
    /// All the added new items are filled by default value of the type T.
    /// </summary>
    /// <param name="index">The index of the element to retrieve or set.</param>
    public T this[int index]
    {
        get
        {
            if (index >= _array.Length)
                Array.Resize(ref _array, (int)(_expansionReserve * index));
            
            return _array[index];
        }
        set
        {
            if (index >= _array.Length)
                Array.Resize(ref _array, (int)(_expansionReserve * index));
            
            _array[index] = value;
        }
    }
}

/// <summary>
/// A collection of integer identifiers for strings.
/// When a new unique string is added to the collection, an identifier is automatically created using an auto-increment method starting from 0.
/// Integer identifiers can be used as indices in the array.
/// </summary>
class IndexedHashSet
{
    // Dictionary for mapping strings to unique integer identifiers
    private readonly Dictionary<string, int> _valueToId = new();
    
    // List for storing strings by their identifiers
    private readonly List<string> _idToValue = [];

    // The number of elements in the collection
    public int Count { get; private set; }

    /// <summary>
    /// Retrieves the identifier for the specified string value.
    /// If the string is not yet in the collection, it will be added.
    /// </summary>
    /// <param name="value">The string to get or add to the collection.</param>
    /// <returns>The identifier for the specified string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int AddAndGetId(string value)
    {
        if (!_valueToId.TryGetValue(value, out var id))
        {
            // If the string is not found, assign it a new identifier
            id = Count++;
            _valueToId[value] = id;
            _idToValue.Add(value);
        }
        return id;
    }

    /// <summary>
    /// Retrieves the string corresponding to the specified integer identifier.
    /// </summary>
    /// <param name="id">The identifier to look up the string.</param>
    /// <returns>The string associated with the specified identifier, or "unknown" if the identifier is invalid.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetValue(int id)
    {
        return id >= 0 && id < _idToValue.Count 
            ? _idToValue[id] 
            : "unknown";
    }
}