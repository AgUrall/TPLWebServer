using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Kontur.ImageTransformer.Logger;

namespace Kontur.ImageTransformer
{
    internal class ImageHttpServer : AsyncHttpServer
    {
        private readonly Func<Image, string, Rectangle, Image> _imageHandler;

        private readonly uint _maxDataSize;
        private readonly Size _maxImageSize;

        public ImageHttpServer(uint maxTasksNumber, Func<Image, string, Rectangle, Image> imageHandler,
            Size maxImageSize, uint maxDataSize = 100 * 1024) : base(maxTasksNumber)
        {
            _imageHandler = imageHandler;
            _maxDataSize = maxDataSize;
            _maxImageSize = maxImageSize;
        }

        protected override async Task HandleContextAsync(HttpListenerContext context)
        {
            try
            {
                if (context.Request.ContentLength64 <= 0)
                {
                    SendEmptyResponse(context, (int) HttpStatusCode.NoContent);
                    return;
                }

                if (context.Request.HttpMethod.ToUpper() != "POST" ||
                    context.Request.ContentLength64 > _maxDataSize)
                {
                    SendEmptyResponse(context, (int) HttpStatusCode.BadRequest);
                    return;
                }

                Tuple<string, Rectangle> data;
                try
                {
                    data = GetReqestData(context);
                }
                catch (Exception)
                {
                    SendEmptyResponse(context, (int) HttpStatusCode.BadRequest);
                    return;
                }

                using (var img = await GetSourceImageAsync(context, ImageFormat.Png).ConfigureAwait(false))
                {
                    if (img.Size.Height > _maxImageSize.Height || img.Size.Width > _maxImageSize.Width)
                        SendEmptyResponse(context, (int) HttpStatusCode.BadRequest);

                    using (var resultImg = _imageHandler(img, data.Item1, data.Item2))
                    {
                        if (resultImg == null)
                            SendEmptyResponse(context, (int) HttpStatusCode.NoContent);
                        else
                            await SendResponce(HttpStatusCode.OK, resultImg, ImageFormat.Png, context)
                                .ConfigureAwait(false);
                    }
                }
            }
            catch (ArgumentException)
            {
                SendEmptyResponse(context, (int) HttpStatusCode.BadRequest);
            }
            catch (ObjectDisposedException error)
            {
                LoggerWrapper.LoggWarning("ImageHttpServer:", error);
            }
            catch (Exception error)
            {
                LoggerWrapper.LoggWarning("ImageHttpServer:", error);
                SendEmptyResponse(context, (int) HttpStatusCode.InternalServerError);
            }
        }

        public Tuple<string, Rectangle> GetReqestData(HttpListenerContext context)
        {
            var parts = context.Request.Url.AbsolutePath.Split('/');
            if (parts.Length < 4) throw new UriFormatException("Wrong uri format");
            var transform = parts[2];
            var area = ParseArea(parts[3]);
            var result = new Tuple<string, Rectangle>(transform, area);
            return result;
        }

        private static Rectangle ParseArea(string reqest)
        {
            var parts = reqest.Split(',');
            if (parts.Length < 4) throw new UriFormatException("Wrong uri format: too few coords");
            var result = new Rectangle
            {
                X = int.Parse(parts[0]),
                Y = int.Parse(parts[1]),
                Width = int.Parse(parts[2]),
                Height = int.Parse(parts[3])
            };
            return result;
        }

        public async Task<Image> GetSourceImageAsync(HttpListenerContext context, ImageFormat format)
        {
            using (var imgStream = new MemoryStream())
            {
                await context.Request.InputStream.CopyToAsync(imgStream).ConfigureAwait(false);
                imgStream.Seek(0, SeekOrigin.Begin);
                var img = Image.FromStream(imgStream);
                if (!img.RawFormat.Equals(format))
                    throw new ArgumentException();
                return img;
            }
        }

        public async Task SendResponce(HttpStatusCode statusCode, Image source, ImageFormat format,
            HttpListenerContext context)
        {
            var response = context.Response;
            response.StatusCode = (int) statusCode;
            response.ContentType = "application/octet-stream";
            using (var imgStream = new MemoryStream())
            {
                source.Save(imgStream, format);
                imgStream.Position = 0;
                await imgStream.CopyToAsync(response.OutputStream).ConfigureAwait(false);
            }

            response.OutputStream.Close();
            response.Close();
        }
    }
}