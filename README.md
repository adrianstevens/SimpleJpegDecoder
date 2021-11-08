# SimpleJpegDecoder
A light-weight C# jpeg decoder based on NanoJpeg - targeting Meadow but should work on most modern C# platforms - https://www.wildernesslabs.co/meadow

Nuget package is hosted on nuget.org: https://www.nuget.org/packages/SimpleJpegDecoder/ 


This code uses a submodule of NanoJpeg.NET (by Jbildstein) C# port of Martin Fiedler's NanoJpeg originally written in C. 

https://github.com/JBildstein/NanoJpeg.Net

- decodes baseline JPEG only, no progressive or lossless JPEG
- supports 8-bit grayscale and YCbCr images, no 16 bit, CMYK or other color spaces
- supports any power-of-two chroma subsampling ratio
- supports restart markers

For more info, visit http://keyj.emphy.de/nanojpeg/
