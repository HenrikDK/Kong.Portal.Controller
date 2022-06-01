namespace Kong.Portal.Ui.Infrastructure;

public static class CompressionExtensions
{
    public static string ToGzipBase64(this string value, CompressionLevel level = CompressionLevel.Fastest)
    {
        var bytes = Encoding.Unicode.GetBytes(value);
        using var input = new MemoryStream(bytes);
        using var output = new MemoryStream();
        using var stream = new GZipStream(output, level);

        input.CopyToAsync(stream);

        var result = output.ToArray();

        return Convert.ToBase64String(result);
    }
    
    public static string ToBrotliBase64(this string value, CompressionLevel level = CompressionLevel.Fastest)
    {
        var bytes = Encoding.Unicode.GetBytes(value);
        using var input = new MemoryStream(bytes);
        using var output = new MemoryStream();
        using var stream = new BrotliStream(output, level);

        input.CopyToAsync(stream);
        stream.FlushAsync();

        var result = output.ToArray();

        return Convert.ToBase64String(result);
    }

    public static string FromGzipBase64(this string value)
    {
        var bytes = Convert.FromBase64String(value);
        using var input = new MemoryStream(bytes);
        using var output = new MemoryStream();
        using var stream = new GZipStream(input, CompressionMode.Decompress);

        stream.CopyToAsync(output);
        stream.FlushAsync();
        
        return Encoding.Unicode.GetString(output.ToArray());
    }

    public static string FromBrotliBase64(this string value)
    {
        var bytes = Convert.FromBase64String(value);
        using var input = new MemoryStream(bytes);
        using var output = new MemoryStream();
        using var stream = new BrotliStream(input, CompressionMode.Decompress);

        stream.CopyToAsync(output);
        
        return Encoding.Unicode.GetString(output.ToArray());
    }
}