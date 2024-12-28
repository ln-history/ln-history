namespace LN_history.Core.Helper;

public static class HelperFunctions
{
    public static readonly TimeSpan DefaultTimespan = new(14, 0, 0, 0);
    
    public static double CalculateCost(long feeBaseMSat, long feeProportionalMillionths, int paymentSizeSat)
    {
        if (paymentSizeSat == 0) return 0.0;
        
        var feeBase = feeBaseMSat / 1000.0;
        var feeProportional = feeProportionalMillionths / 1_000_000.0;
            
        return feeBase + paymentSizeSat * feeProportional;
    }
        
    public static double CalculateMedian(List<double> numbers)
    {
        if (numbers.Count == 0) return 0.0;
    
        numbers.Sort();
        var middle = numbers.Count / 2;

        return numbers.Count % 2 == 0 
            ? (numbers[middle - 1] + numbers[middle]) / 2.0 
            : numbers[middle];
    }

    public static double CalculateAverage(List<double> numbers)
    {
        if (numbers.Count == 0) return 0.0;
        
        // Calculate the sum of all numbers in the list
        var sum = numbers.Sum();
        var average = sum / numbers.Count;
    
        return average;
    }

    public static string GetFileNameByTimestamp(DateTime timestamp, string extension)
    {
        var formattedTimestamp = timestamp.ToString("o")
            .Replace(":", "-")
            .Replace(".", "_");
        return $"ln-{formattedTimestamp}.{extension}";
    }
}