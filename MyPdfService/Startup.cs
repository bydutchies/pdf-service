using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PuppeteerSharp;
using System.IO;
using System.Runtime.InteropServices;

[assembly: FunctionsStartup(typeof(MyPdfService.Startup))]
namespace MyPdfService;

public class Startup : FunctionsStartup
{
  public override void Configure(IFunctionsHostBuilder builder)
  {
    var browserFetcherOptions = new BrowserFetcherOptions();

    // The default download location is readonly when hosted in Azure.
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      browserFetcherOptions.Path = Path.GetTempPath();

    // Make sure browser is downloaded
    var browserFetcher = new BrowserFetcher(browserFetcherOptions);
    browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision).Wait();
    
    // Store download path in settings. We need this when executing function.
    var puppeteerOptions = new PuppeteerOptions
    {
      BrowserExecutablePath = browserFetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision)
    };
    builder.Services.AddSingleton(puppeteerOptions);
  }
}
