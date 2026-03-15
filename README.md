# SimpleJpegDecoder

A lightweight C# JPEG decoder targeting [Meadow](https://www.wildernesslabs.co/meadow) and other memory-constrained platforms. Works on any modern C# platform supporting .NET Standard 2.0 or .NET Framework 4.7.2.

NuGet: https://www.nuget.org/packages/SimpleJpegDecoder/

## Features

- Decodes baseline JPEG (no progressive or lossless)
- Supports 8-bit grayscale and YCbCr (RGB) images
- Supports any power-of-two chroma subsampling ratio
- Supports restart markers
- Implements `IDisposable` for deterministic release of unmanaged memory
- Zero-allocation decode path for repeated use (e.g. display refresh on Meadow)

## Usage

### Basic decode from byte array

```csharp
var decoder = new JpegDecoder();
byte[] pixels = decoder.DecodeJpeg(jpegBytes);
// decoder.Width, decoder.Height, decoder.IsColor are now populated
```

### Decode from a Stream

```csharp
using var stream = File.OpenRead("image.jpg");
byte[] pixels = decoder.DecodeJpeg(stream);
```

### Zero-allocation decode (recommended for Meadow)

Pre-allocate the output buffer once and reuse it across frames to avoid GC pressure:

```csharp
var decoder = new JpegDecoder();

// decode once to find dimensions
decoder.DecodeJpeg(firstFrame);
var outputBuffer = new byte[decoder.Width * decoder.Height * 3];

// reuse buffer for every subsequent frame — no heap allocation
while (true)
{
    decoder.DecodeJpeg(GetNextFrame(), outputBuffer);
    display.Draw(outputBuffer);
}
```

### Cleanup

`JpegDecoder` holds unmanaged memory internally. Use it in a `using` block or call `Dispose()` when done:

```csharp
using (var decoder = new JpegDecoder())
{
    var pixels = decoder.DecodeJpeg(jpegBytes);
}
```

## Limitations

- Baseline JPEG only — no progressive, lossless, CMYK, or 16-bit
- Minimum image size of 8×8 pixels (NanoJpeg constraint with 4:2:0 chroma subsampling)

## Credits

Built on [NanoJpeg.NET](https://github.com/JBildstein/NanoJpeg.Net) by Johannes Bildstein — a C# port of [NanoJPEG](http://keyj.emphy.de/nanojpeg/) by Martin J. Fiedler.
