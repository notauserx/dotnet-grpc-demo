using Grpc.Net.Compression;
using System.IO;
using System.IO.Compression;

namespace CountryService.Web.Providers;

public class BrotliCompressionProvider : ICompressionProvider
{
    private readonly CompressionLevel? _compressionLevel;

    public BrotliCompressionProvider(CompressionLevel compressionLevel)
    {
        _compressionLevel = compressionLevel;
    }

    public BrotliCompressionProvider()  {}


    // Must match grpc-accept-encoding
    public string EncodingName => "br";

    public Stream CreateCompressionStream(Stream stream, CompressionLevel? compressionLevel)
    {
        if (compressionLevel.HasValue)
        {
            return new BrotliStream(stream, compressionLevel.Value, true);
        }
        else if (!_compressionLevel.HasValue && compressionLevel.HasValue)
            return new BrotliStream(stream, compressionLevel.Value, true);

        return new BrotliStream(stream, CompressionLevel.Fastest, true);
    }

    public Stream CreateDecompressionStream(Stream stream)
    {
        return new BrotliStream(stream, CompressionMode.Decompress);
    }
}
