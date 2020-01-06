using System.IO;

namespace SimpleJpegDecoder
{
    public class JpegDecoder
    {
        #region Properties

        /// <summary>
        /// Width of loaded image in pixels
        /// </summary>
        public int Width => nanoJpeg == null?0:nanoJpeg.njGetWidth();
        /// <summary>
        /// Height of loaded image in pixels
        /// </summary>
        public int Height => nanoJpeg == null ? 0 : nanoJpeg.njGetHeight();
        /// <summary>
        /// Size of decoded image in bytes
        /// </summary>
        public int ImageSize => nanoJpeg == null ? 0 : nanoJpeg.njGetImageSize();
        /// <summary>
        /// Is image color (true) or greyscale (false)
        /// </summary>
        public bool IsColor => nanoJpeg == null ? false : nanoJpeg.njIsColor();

        #endregion

        #region Fields 

        KeyJ.NanoJPEG nanoJpeg;

        #endregion

        #region Contructor(s)

        public JpegDecoder()
        {
            nanoJpeg = new KeyJ.NanoJPEG();
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
            nanoJpeg.njDecode(jpegData);
            return nanoJpeg.njGetImage();
        }

        /// <summary>
        /// Get decoded uncompressed data in a byte array
        /// </summary>
        public byte[] GetImageData()
        {
            return nanoJpeg.njGetImage();
        }

        #endregion
    }
}