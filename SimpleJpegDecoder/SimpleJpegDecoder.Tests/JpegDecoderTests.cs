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
    }
}
