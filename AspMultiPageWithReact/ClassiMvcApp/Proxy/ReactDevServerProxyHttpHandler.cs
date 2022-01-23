using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace ClassiMvcApp
{
    public class ReactDevServerProxyHttpHandler : IHttpHandler, IRouteHandler
    {
        private readonly string _targetUrl;

        public ReactDevServerProxyHttpHandler(string targetUrl)
        {
            _targetUrl = targetUrl;
        }

        public static void MapReactDevServer(RouteCollection routes,
            string targetUrl = "http://localhost:3000/",
            string pathPrefix = "static/{*rest}")
        {
            routes.Add(new Route(pathPrefix, new ReactDevServerProxyHttpHandler(targetUrl.TrimEnd('/')))
            {
                Constraints = new RouteValueDictionary(new
                {
                    Action = "^$",
                    Controller = "^$",
                    Area = "^$",
                    rest = "^.+$"
                })
            });
        }

        /// <summary>
        ///     Enables processing of HTTP Web requests by a custom HttpHandler that implements the
        ///     <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">
        ///     An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic
        ///     server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.
        /// </param>
        public void ProcessRequest(HttpContext context)
        {
            var url = context.Request.RawUrl;
            try
            {
                var request = WebRequest.CreateHttp(_targetUrl + url);
                context.Request.CopyHeadersTo(request);
                request.Method = context.Request.HttpMethod;
                request.ContentType = context.Request.ContentType;
                request.UserAgent = context.Request.UserAgent;

                if (context.Request.AcceptTypes != null)
                    request.Accept = string.Join(";", context.Request.AcceptTypes);

                if (context.Request.UrlReferrer != null)
                    request.Referer = context.Request.UrlReferrer.ToString();

                if (!context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                    context.Request.InputStream.CopyTo(request.GetRequestStream());

                var response = (HttpWebResponse)request.GetResponse();
                response.CopyHeadersTo(context.Response);
                context.Response.ContentType = response.ContentType;
                context.Response.StatusCode = (int)response.StatusCode;
                context.Response.StatusDescription = response.StatusDescription;
                context.Response.Flush();
                var stream = response.GetResponseStream();
                stream.CopyTo(context.Response.OutputStream);
                stream.Flush();
            }
            catch (WebException exception)
            {
                if (exception.Response is HttpWebResponse response)
                {
                    context.Response.StatusCode = (int)response.StatusCode;
                    context.Response.StatusDescription = response.StatusDescription;
                    response.CopyHeadersTo(context.Response);
                    var stream = response.GetResponseStream();
                    if (stream != null)
                    {
                        stream.CopyTo(context.Response.OutputStream);
                        context.Response.OutputStream.Flush();
                    }
                }
                else
                {

                    context.Response.StatusCode = 501;
                    context.Response.StatusDescription = exception.Status.ToString();
                    var msg = Encoding.ASCII.GetBytes(exception.Message);
                    context.Response.OutputStream.Write(msg, 0, msg.Length);
                    context.Response.OutputStream.Flush();

                }
            }
            catch (Exception exception)
            {
                context.Response.StatusCode = 501;
                context.Response.StatusDescription = "Failed to call proxied URL.";
                var msg = Encoding.ASCII.GetBytes(exception.Message);
                context.Response.OutputStream.Write(msg, 0, msg.Length);
            }
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return this;
        }

        /// <summary>
        ///     Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>
        ///     true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.
        /// </returns>
        public bool IsReusable
        {
            get { return true; }
        }
    }
}
