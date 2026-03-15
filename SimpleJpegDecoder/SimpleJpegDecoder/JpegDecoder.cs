using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SimpleJpegDecoder
{
    public class JpegDecoder : IDisposable
    {
        /// <summary>
        /// Width of loaded image in pixels
        /// </summary>
        public int Width => nanoJpeg == null?0:nanoJpeg.Width;
        /// <summary>
        /// Height of loaded image in pixels
        /// </summary>
        public int Height => nanoJpeg == null ? 0 : nanoJpeg.Height;
        /// <summary>
        /// Size of decoded image in bytes
        /// </summary>
        public int ImageSize => nanoJpeg == null ? 0 : nanoJpeg.ImageSize;
        /// <summary>
        /// Is image color (true) or greyscale (false)
        /// </summary>
        public bool IsColor => nanoJpeg == null ? false : nanoJpeg.IsColor;

        NanoJpeg.NJImage nanoJpeg;

        byte[] decodedData;

        public JpegDecoder()
        {
            nanoJpeg = new NanoJpeg.NJImage();
        }

        /// <summary>
        /// Decode compressed jpeg from a Stream and
        /// return uncompressed data in a byte array
        /// </summary>
        /// <param name="jpegStream"></param>
        /// <returns></returns>
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
        /// Decode compressed jpeg from a byte array and
        /// return uncompressed data in a byte array
        /// </summary>
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
        /// Decode compressed jpeg from a byte array into a caller-provided output buffer.
        /// Use this overload to avoid a heap allocation on every decode —
        /// allocate the buffer once and reuse it across frames.
        /// </summary>
        /// <param name="jpegData">Compressed JPEG data</param>
        /// <param name="outputBuffer">Buffer to write decoded pixels into. Must be at least Width * Height * channels bytes.</param>
        /// <exception cref="ArgumentException">outputBuffer is too small for the decoded image</exception>
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
        /// Decode compressed jpeg from a Stream into a caller-provided output buffer.
        /// Use this overload to avoid a heap allocation on every decode —
        /// allocate the buffer once and reuse it across frames.
        /// </summary>
        /// <param name="jpegStream">Stream containing compressed JPEG data</param>
        /// <param name="outputBuffer">Buffer to write decoded pixels into. Must be at least Width * Height * channels bytes.</param>
        /// <exception cref="ArgumentException">outputBuffer is too small for the decoded image</exception>
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
        /// Get decoded uncompressed data in a byte array
        /// </summary>
        public byte[] GetImageData()
        {
            return decodedData;
        }

        /// <summary>
        /// Reset decoder and clear data buffer
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
