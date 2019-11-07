using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Vertica.Integration.WebApi;

namespace Vertica.Integration.Portal
{
    public static class PortalExtensions
    {
		public static WebApiConfiguration WithPortal(this WebApiConfiguration webApi)
        {
	        if (webApi == null) throw new ArgumentNullException(nameof(webApi));

	        webApi.Application.Extensibility(extensibility =>
            {
	            extensibility.Register(() =>
	            {
					if (!Directory.Exists(PortalConfiguration.Folder))
					{
						var zipFile = new FileInfo(PortalConfiguration.ZipFile);

						if (!zipFile.Exists)
						{
							throw new InvalidOperationException(
								$@"Expected the following zip-file '{zipFile.Name}' to be present in the following folder '{zipFile
									.DirectoryName}'. 
This zip is automatically added when installing the Vertica.Integration.Portal NuGet package. 
Try re-installing the package and/or make sure that the zip-file is included part of your deployment of this platform and placed in the root-folder.");
						}

						ZipFile.ExtractToDirectory(zipFile.FullName, PortalConfiguration.Folder);
					}

	                webApi
	                    .AddFromAssemblyOfThis<PortalConfiguration>()
	                    .HttpServer(httpServer => httpServer.Configure(owin =>
	                    {
	                        owin.App.UseFileServer(new FileServerOptions
	                        {
	                            RequestPath = new PathString(""),
	                            FileSystem = new PhysicalFileSystem(PortalConfiguration.Folder)
	                        });

                            // https://github.com/aspnet/AspNetKatana/wiki/Static-Files-on-IIS
                            owin.App.UseStageMarker(PipelineStage.MapHandler);
	                    }));

		            return new PortalConfiguration();
	            });
            });

			return webApi;
        }
    }
}