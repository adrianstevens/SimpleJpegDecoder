using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NanoJpeg;
using Xunit;

namespace SimpleJpegDecoder.Tests
{
    public class JpegDecoderTests
    {
        /// <summary>
        /// Generates a valid baseline JPEG using System.Drawing.
        /// System.Drawing always produces 3-channel YCbCr JPEGs, so IsColor will be true.
        /// </summary>
        private static byte[] CreateColorJpeg(int width, int height)
        {
            using (var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        // -------------------------------------------------------------------------
        // Default state (before any decode)
        // -------------------------------------------------------------------------

        [Fact]
        public void Width_BeforeDecode_ReturnsZero()
        {
            var decoder = new JpegDecoder();
            Assert.Equal(0, decoder.Width);
        }

        [Fact]
        public void Height_BeforeDecode_ReturnsZero()
        {
            var decoder = new JpegDecoder();
            Assert.Equal(0, decoder.Height);
        }

        [Fact]
        public void ImageSize_BeforeDecode_ReturnsZero()
        {
            var decoder = new JpegDecoder();
            Assert.Equal(0, decoder.ImageSize);
        }

        [Fact]
        public void IsColor_BeforeDecode_ReturnsTrue()
        {
            // NJImage initializes ncomp=0; IsColor returns (ncomp != 1) = true.
            // The null guard in JpegDecoder.IsColor is dead code — nanoJpeg is never null after construction.
            var decoder = new JpegDecoder();
            Assert.True(decoder.IsColor);
        }

        [Fact]
        public void GetImageData_BeforeDecode_ReturnsNull()
        {
            var decoder = new JpegDecoder();
            Assert.Null(decoder.GetImageData());
        }

        // -------------------------------------------------------------------------
        // DecodeJpeg(byte[])
        // -------------------------------------------------------------------------

        [Fact]
        public void DecodeJpeg_ByteArray_SetsCorrectWidth()
        {
            var decoder = new JpegDecoder();
            decoder.DecodeJpeg(CreateColorJpeg(16, 8));
            Assert.Equal(16, decoder.Width);
        }

        [Fact]
        public void DecodeJpeg_ByteArray_SetsCorrectHeight()
        {
            var decoder = new JpegDecoder();
            decoder.DecodeJpeg(CreateColorJpeg(16, 8));
            Assert.Equal(8, decoder.Height);
        }

        [Fact]
        public void DecodeJpeg_ByteArray_ColorJpeg_IsColorTrue()
        {
            var decoder = new JpegDecoder();
            decoder.DecodeJpeg(CreateColorJpeg(8, 8));
            Assert.True(decoder.IsColor);
        }

        [Fact]
        public void DecodeJpeg_ByteArray_ReturnsNonNullArray()
        {
            var decoder = new JpegDecoder();
            var result = decoder.DecodeJpeg(CreateColorJpeg(8, 8));
            Assert.NotNull(result);
        }

        [Fact]
        public void DecodeJpeg_ByteArray_ColorJpeg_DataLengthIsWidthTimesHeightTimesThree()
        {
            var decoder = new JpegDecoder();
            var result = decoder.DecodeJpeg(CreateColorJpeg(16, 8));
            Assert.Equal(16 * 8 * 3, result.Length);
        }

        [Fact]
        public void ImageSize_AfterColorDecode_EqualsWidthTimesHeightTimesThree()
        {
            var decoder = new JpegDecoder();
            decoder.DecodeJpeg(CreateColorJpeg(12, 8));
            Assert.Equal(decoder.Width * decoder.Height * 3, decoder.ImageSize);
        }

        // -------------------------------------------------------------------------
        // DecodeJpeg(Stream)
        // -------------------------------------------------------------------------

        [Fact]
        public void DecodeJpeg_Stream_SetsCorrectDimensions()
        {
            var decoder = new JpegDecoder();
            decoder.DecodeJpeg(new MemoryStream(CreateColorJpeg(10, 6)));
            Assert.Equal(10, decoder.Width);
            Assert.Equal(6, decoder.Height);
        }

        [Fact]
        public void DecodeJpeg_Stream_ProducesIdenticalOutputToByteArray()
        {
            var jpeg = CreateColorJpeg(8, 8);
            var fromBytes = new JpegDecoder().DecodeJpeg(jpeg);
            var fromStream = new JpegDecoder().DecodeJpeg(new MemoryStream(jpeg));
            Assert.Equal(fromBytes, fromStream);
        }

        // -------------------------------------------------------------------------
        // GetImageData
        // -------------------------------------------------------------------------

        [Fact]
        public void GetImageData_AfterDecode_ReturnsSameReferenceAsDecodeJpeg()
        {
            var decoder = new JpegDecoder();
            var result = decoder.DecodeJpeg(CreateColorJpeg(8, 8));
            Assert.Same(result, decoder.GetImageData());
        }

        // -------------------------------------------------------------------------
        // Reset
        // -------------------------------------------------------------------------

        [Fact]
        public void Reset_AfterDecode_ClearsWidth()
        {
            var decoder = new JpegDecoder();
            decoder.DecodeJpeg(CreateColorJpeg(8, 8));
            decoder.Reset();
            Assert.Equal(0, decoder.Width);
        }

        [Fact]
        public void Reset_AfterDecode_ClearsHeight()
        {
            var decoder = new JpegDecoder();
            decoder.DecodeJpeg(CreateColorJpeg(8, 8));
            decoder.Reset();
            Assert.Equal(0, decoder.Height);
        }

        [Fact]
        public void Reset_AfterDecode_ClearsImageSize()
        {
            var decoder = new JpegDecoder();
            decoder.DecodeJpeg(CreateColorJpeg(8, 8));
            decoder.Reset();
            Assert.Equal(0, decoder.ImageSize);
        }

        [Fact]
        public void Reset_AfterDecode_ClearsGetImageData()
        {
            var decoder = new JpegDecoder();
            decoder.DecodeJpeg(CreateColorJpeg(8, 8));
            decoder.Reset();
            Assert.Null(decoder.GetImageData());
        }

        [Fact]
        public void DecodeJpeg_AfterReset_DecodesSuccessfully()
        {
            var decoder = new JpegDecoder();
            decoder.DecodeJpeg(CreateColorJpeg(16, 16));
            decoder.Reset();

            // NanoJpeg requires chroma component dimensions >= 3px.
            // With 4:2:0 subsampling this means source images must be >= 8x8.
            var result = decoder.DecodeJpeg(CreateColorJpeg(8, 8));
            Assert.Equal(8, decoder.Width);
            Assert.Equal(8, decoder.Height);
            Assert.NotNull(result);
        }

        // -------------------------------------------------------------------------
        // Re-use without Reset (NJImage.Decode calls Init() internally)
        // -------------------------------------------------------------------------

        [Fact]
        public void DecodeJpeg_CalledTwiceWithoutReset_SecondDecodeReturnsCorrectDimensions()
        {
            var decoder = new JpegDecoder();
            decoder.DecodeJpeg(CreateColorJpeg(16, 16));
            decoder.DecodeJpeg(CreateColorJpeg(8, 12));

            Assert.Equal(8, decoder.Width);
            Assert.Equal(12, decoder.Height);
        }

        // -------------------------------------------------------------------------
        // Error cases
        // -------------------------------------------------------------------------

        [Fact]
        public void DecodeJpeg_EmptyArray_ThrowsNJException()
        {
            var decoder = new JpegDecoder();
            Assert.Throws<NJException>(() => decoder.DecodeJpeg(new byte[0]));
        }

        [Fact]
        public void DecodeJpeg_SingleByte_ThrowsNJException()
        {
            var decoder = new JpegDecoder();
            Assert.Throws<NJException>(() => decoder.DecodeJpeg(new byte[] { 0xFF }));
        }

        [Fact]
        public void DecodeJpeg_WrongMagicBytes_ThrowsNJException()
        {
            var decoder = new JpegDecoder();
            // PNG header — starts with 0x89 0x50, not 0xFF 0xD8
            var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            Assert.Throws<NJException>(() => decoder.DecodeJpeg(pngHeader));
        }

        [Fact]
        public void DecodeJpeg_RandomData_ThrowsNJException()
        {
            var decoder = new JpegDecoder();
            var random = new byte[256];
            new System.Random(42).NextBytes(random);
            Assert.Throws<NJException>(() => decoder.DecodeJpeg(random));
        }

        [Fact]
        public void DecodeJpeg_Stream_InvalidData_ThrowsNJException()
        {
            var decoder = new JpegDecoder();
            var notJpeg = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            Assert.Throws<NJException>(() => decoder.DecodeJpeg(new MemoryStream(notJpeg)));
        }

        // -------------------------------------------------------------------------
        // DecodeJpeg(Stream) — non-seekable stream (exercises the internal buffer path)
        // -------------------------------------------------------------------------

        [Fact]
        public void DecodeJpeg_NonSeekableStream_DecodesCorrectly()
        {
            var jpeg = CreateColorJpeg(8, 8);
            var decoder = new JpegDecoder();
            using (var wrapped = new NonSeekableStream(jpeg))
            {
                var result = decoder.DecodeJpeg(wrapped);
                Assert.Equal(8, decoder.Width);
                Assert.Equal(8, decoder.Height);
                Assert.NotNull(result);
            }
        }

        [Fact]
        public void DecodeJpeg_NonSeekableStream_ProducesSameOutputAsByteArray()
        {
            var jpeg = CreateColorJpeg(8, 8);
            var fromBytes = new JpegDecoder().DecodeJpeg(jpeg);
            using (var wrapped = new NonSeekableStream(jpeg))
            {
                var fromStream = new JpegDecoder().DecodeJpeg(wrapped);
                Assert.Equal(fromBytes, fromStream);
            }
        }

        // -------------------------------------------------------------------------
        // DecodeJpeg(byte[], byte[]) — pre-allocated output buffer
        // -------------------------------------------------------------------------

        [Fact]
        public void DecodeJpeg_WithOutputBuffer_ProducesSamePixelsAsAllocatingOverload()
        {
            var jpeg = CreateColorJpeg(16, 8);
            var expected = new JpegDecoder().DecodeJpeg(jpeg);

            var decoder = new JpegDecoder();
            var outputBuffer = new byte[16 * 8 * 3];
            decoder.DecodeJpeg(jpeg, outputBuffer);

            Assert.Equal(expected, outputBuffer);
        }

        [Fact]
        public void DecodeJpeg_WithOutputBuffer_SetsWidthAndHeight()
        {
            var decoder = new JpegDecoder();
            var jpeg = CreateColorJpeg(16, 8);
            decoder.DecodeJpeg(jpeg, new byte[16 * 8 * 3]);

            Assert.Equal(16, decoder.Width);
            Assert.Equal(8, decoder.Height);
        }

        [Fact]
        public void DecodeJpeg_WithOutputBuffer_GetImageDataReturnsProvidedBuffer()
        {
            var decoder = new JpegDecoder();
            var jpeg = CreateColorJpeg(8, 8);
            var outputBuffer = new byte[8 * 8 * 3];
            decoder.DecodeJpeg(jpeg, outputBuffer);

            Assert.Same(outputBuffer, decoder.GetImageData());
        }

        [Fact]
        public void DecodeJpeg_WithOutputBuffer_LargerThanNeeded_Succeeds()
        {
            var decoder = new JpegDecoder();
            var jpeg = CreateColorJpeg(8, 8);
            var outputBuffer = new byte[8 * 8 * 3 + 256]; // extra space is fine
            decoder.DecodeJpeg(jpeg, outputBuffer);

            Assert.Equal(8, decoder.Width);
        }

        [Fact]
        public void DecodeJpeg_WithOutputBuffer_TooSmall_ThrowsArgumentException()
        {
            var decoder = new JpegDecoder();
            var jpeg = CreateColorJpeg(8, 8);
            var tooSmall = new byte[10];

            Assert.Throws<ArgumentException>(() => decoder.DecodeJpeg(jpeg, tooSmall));
        }

        [Fact]
        public void DecodeJpeg_Stream_WithOutputBuffer_ProducesSamePixelsAsAllocatingOverload()
        {
            var jpeg = CreateColorJpeg(16, 8);
            var expected = new JpegDecoder().DecodeJpeg(jpeg);

            var decoder = new JpegDecoder();
            var outputBuffer = new byte[16 * 8 * 3];
            decoder.DecodeJpeg(new MemoryStream(jpeg), outputBuffer);

            Assert.Equal(expected, outputBuffer);
        }

        [Fact]
        public void DecodeJpeg_Stream_WithOutputBuffer_TooSmall_ThrowsArgumentException()
        {
            var decoder = new JpegDecoder();
            var jpeg = CreateColorJpeg(8, 8);

            Assert.Throws<ArgumentException>(() => decoder.DecodeJpeg(new MemoryStream(jpeg), new byte[10]));
        }

        // -------------------------------------------------------------------------
        // IDisposable / Reset dispose
        // -------------------------------------------------------------------------

        [Fact]
        public void JpegDecoder_ImplementsIDisposable()
        {
            Assert.IsAssignableFrom<IDisposable>(new JpegDecoder());
        }

        [Fact]
        public void Dispose_CanBeUsedInUsingBlock()
        {
            // Should not throw
            using (var decoder = new JpegDecoder())
            {
                decoder.DecodeJpeg(CreateColorJpeg(8, 8));
            }
        }

        [Fact]
        public void Dispose_ClearsProperties()
        {
            var decoder = new JpegDecoder();
            decoder.DecodeJpeg(CreateColorJpeg(8, 8));
            decoder.Dispose();

            Assert.Equal(0, decoder.Width);
            Assert.Equal(0, decoder.Height);
            Assert.Equal(0, decoder.ImageSize);
            Assert.Null(decoder.GetImageData());
        }

        [Fact]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var decoder = new JpegDecoder();
            decoder.DecodeJpeg(CreateColorJpeg(8, 8));
            decoder.Dispose();
            decoder.Dispose(); // should not throw
        }

        [Fact]
        public void Reset_CalledMultipleTimes_DoesNotThrow()
        {
            var decoder = new JpegDecoder();
            decoder.DecodeJpeg(CreateColorJpeg(8, 8));
            decoder.Reset();
            decoder.Reset();
            decoder.Reset();
        }

        [Fact]
        public void Reset_DecodesSuccessfullyAfterMultipleResets()
        {
            var decoder = new JpegDecoder();
            decoder.Reset();
            decoder.Reset();

            var result = decoder.DecodeJpeg(CreateColorJpeg(8, 8));
            Assert.Equal(8, decoder.Width);
            Assert.NotNull(result);
        }
    }

    /// <summary>
    /// Wraps a byte array in a Stream that is not a MemoryStream,
    /// so the non-fast-path in DecodeJpeg(Stream) is exercised.
    /// </summary>
    internal class NonSeekableStream : Stream
    {
        private readonly MemoryStream _inner;
        public NonSeekableStream(byte[] data) => _inner = new MemoryStream(data);
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new System.NotSupportedException();
        public override long Position { get => throw new System.NotSupportedException(); set => throw new System.NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new System.NotSupportedException();
        public override void SetLength(long value) => throw new System.NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new System.NotSupportedException();
        protected override void Dispose(bool disposing) { if (disposing) _inner.Dispose(); base.Dispose(disposing); }
    }
}
