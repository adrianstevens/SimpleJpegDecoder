using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SimpleJpegDecoder
{
    public class JpegDecoder : IDisposable
    {
        /// <summary>
        /// Width of the decoded image in pixels. Returns 0 if no image has been decoded.
        /// </summary>
        public int Width => nanoJpeg == null?0:nanoJpeg.Width;

        /// <summary>
        /// Height of the decoded image in pixels. Returns 0 if no image has been decoded.
        /// </summary>
        public int Height => nanoJpeg == null ? 0 : nanoJpeg.Height;

        /// <summary>
        /// Size of the decoded image data in bytes (Width × Height × channels).
        /// Returns 0 if no image has been decoded.
        /// </summary>
        public int ImageSize => nanoJpeg == null ? 0 : nanoJpeg.ImageSize;

        /// <summary>
        /// Returns true if the decoded image is color (RGB), false if grayscale.
        /// Note: returns true before any image has been decoded.
        /// </summary>
        public bool IsColor => nanoJpeg == null ? false : nanoJpeg.IsColor;

        NanoJpeg.NJImage nanoJpeg;

        byte[] decodedData;

        /// <summary>
        /// Initializes a new instance of the <see cref="JpegDecoder"/> class.
        /// </summary>
        public JpegDecoder()
        {
            nanoJpeg = new NanoJpeg.NJImage();
        }

        /// <summary>
        /// Decodes a compressed JPEG from a stream and returns the uncompressed pixel data.
        /// </summary>
        /// <param name="jpegStream">Stream containing compressed JPEG data.</param>
        /// <returns>Byte array of uncompressed pixel data (RGB or grayscale).</returns>
        /// <exception cref="NanoJpeg.NJException">Thrown if the data is not a valid baseline JPEG.</exception>
        public byte[] DecodeJpeg(Stream jpegStream)
        {
            using (var buffer = new MemoryStream())
            {
                jpegStream.CopyTo(buffer);
                nanoJpeg.Decode(buffer); // uses GetBuffer() internally — no ToArray() copy
            }

            decodedData = new byte[nanoJpeg.ImageSize];
            unsafe
            {
                Marshal.Copy((IntPtr)nanoJpeg.Image, decodedData, 0, nanoJpeg.ImageSize);
            }
            return decodedData;
        }

        /// <summary>
        /// Decodes a compressed JPEG from a byte array and returns the uncompressed pixel data.
        /// </summary>
        /// <param name="jpegData">Compressed JPEG data.</param>
        /// <returns>Byte array of uncompressed pixel data (RGB or grayscale).</returns>
        /// <exception cref="NanoJpeg.NJException">Thrown if the data is not a valid baseline JPEG.</exception>
        public byte[] DecodeJpeg(byte[] jpegData)
        {
            nanoJpeg.Decode(jpegData);

            decodedData = new byte[nanoJpeg.ImageSize];
            unsafe
            {
                Marshal.Copy((IntPtr)nanoJpeg.Image, decodedData, 0, nanoJpeg.ImageSize);
            }
            return decodedData;
        }

        /// <summary>
        /// Decodes a compressed JPEG from a byte array into a caller-provided output buffer.
        /// Use this overload to avoid a heap allocation on every decode —
        /// allocate the buffer once and reuse it across frames.
        /// </summary>
        /// <param name="jpegData">Compressed JPEG data.</param>
        /// <param name="outputBuffer">Buffer to write decoded pixels into. Must be at least Width × Height × channels bytes.</param>
        /// <exception cref="ArgumentException">Thrown if outputBuffer is too small for the decoded image.</exception>
        /// <exception cref="NanoJpeg.NJException">Thrown if the data is not a valid baseline JPEG.</exception>
        public void DecodeJpeg(byte[] jpegData, byte[] outputBuffer)
        {
            nanoJpeg.Decode(jpegData);

            if (outputBuffer.Length < nanoJpeg.ImageSize)
                throw new ArgumentException($"Output buffer too small. Required: {nanoJpeg.ImageSize}, provided: {outputBuffer.Length}", nameof(outputBuffer));

            unsafe
            {
                Marshal.Copy((IntPtr)nanoJpeg.Image, outputBuffer, 0, nanoJpeg.ImageSize);
            }
            decodedData = outputBuffer;
        }

        /// <summary>
        /// Decodes a compressed JPEG from a stream into a caller-provided output buffer.
        /// Use this overload to avoid a heap allocation on every decode —
        /// allocate the buffer once and reuse it across frames.
        /// </summary>
        /// <param name="jpegStream">Stream containing compressed JPEG data.</param>
        /// <param name="outputBuffer">Buffer to write decoded pixels into. Must be at least Width × Height × channels bytes.</param>
        /// <exception cref="ArgumentException">Thrown if outputBuffer is too small for the decoded image.</exception>
        /// <exception cref="NanoJpeg.NJException">Thrown if the data is not a valid baseline JPEG.</exception>
        public void DecodeJpeg(Stream jpegStream, byte[] outputBuffer)
        {
            using (var buffer = new MemoryStream())
            {
                jpegStream.CopyTo(buffer);
                nanoJpeg.Decode(buffer);
            }

            if (outputBuffer.Length < nanoJpeg.ImageSize)
                throw new ArgumentException($"Output buffer too small. Required: {nanoJpeg.ImageSize}, provided: {outputBuffer.Length}", nameof(outputBuffer));

            unsafe
            {
                Marshal.Copy((IntPtr)nanoJpeg.Image, outputBuffer, 0, nanoJpeg.ImageSize);
            }
            decodedData = outputBuffer;
        }

        /// <summary>
        /// Returns the uncompressed pixel data from the last successful decode.
        /// </summary>
        /// <returns>Byte array of uncompressed pixel data, or null if no image has been decoded.</returns>
        public byte[] GetImageData()
        {
            return decodedData;
        }

        /// <summary>
        /// Resets the decoder, disposing the current internal state and clearing the data buffer.
        /// Call this before reusing the decoder if you need to release unmanaged memory immediately.
        /// </summary>
        public void Reset()
        {
            nanoJpeg.Dispose();
            nanoJpeg = new NanoJpeg.NJImage();
            decodedData = null;
        }

        /// <summary>
        /// Releases all resources used by the JpegDecoder
        /// </summary>
        public void Dispose()
        {
            nanoJpeg?.Dispose();
            nanoJpeg = null;
            decodedData = null;
        }

    }
}
