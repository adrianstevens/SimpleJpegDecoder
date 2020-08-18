using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SimpleJpegDecoder
{
    public class JpegDecoder
    {
        #region Properties

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

        #endregion

        #region Fields 

        NanoJpeg.NJImage nanoJpeg;

        byte[] decodedData;

        #endregion

        #region Contructor(s)

        public JpegDecoder()
        {
            nanoJpeg = new NanoJpeg.NJImage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Decode compressed jpeg from a Stream and 
        /// return uncompressed data in a byte array
        /// </summary>
        /// <param name="jpegStream"></param>
        /// <returns></returns>
        public byte[] DecodeJpeg(Stream jpegStream)
        {
            using (var ms = new MemoryStream())
            {
                jpegStream.CopyTo(ms);
                return DecodeJpeg(ms.ToArray());
            }
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
            nanoJpeg = new NanoJpeg.NJImage();
            decodedData = null; 
        }

        #endregion
    }
}