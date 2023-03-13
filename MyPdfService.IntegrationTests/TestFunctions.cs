using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PuppeteerSharp;
using System.Runtime.InteropServices;

namespace MyPdfService.IntegrationTests;

[TestClass]
public class TestFunctions
{
  private readonly string _path;
  
  public TestFunctions() 
  {
    var browserFetcherOptions = new BrowserFetcherOptions();

    // The default download location is readonly when hosted in Azure.
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      browserFetcherOptions.Path = Path.GetTempPath();

    // Make sure browser is downloaded
    var browserFetcher = new BrowserFetcher(browserFetcherOptions);
    browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision).Wait();

    _path = browserFetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision);
  }

  [TestMethod]
  public async Task TestHtmlToPdf()
  {
    // Arrange
    var function = new HtmlToPdf(new PuppeteerOptions { BrowserExecutablePath = _path });
    var body = new { html = "<p>Hello World</p>" };

    // Act
    var response = await function.Run(GetRequest(body));

    // Assert
    Assert.AreEqual(typeof(FileStreamResult), response.GetType());
  }

  [TestMethod]
  public async Task TestUrlToPdf()
  {
    // Arrange
    var function = new UrlToPdf(new PuppeteerOptions { BrowserExecutablePath = _path });
    var body = new { url = "https://www.bydutchies.nl" };

    // Act
    var response = await function.Run(GetRequest(body));

    // Assert
    Assert.AreEqual(typeof(FileStreamResult), response.GetType());
  }

  #region Private
  private HttpRequest GetRequest(object body)
  {
    var json = JsonConvert.SerializeObject(body);

    var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

    var context = new DefaultHttpContext();
    var request = context.Request;
    request.Body = memoryStream;
    request.ContentType = "application/json";

    return request;
  }
  #endregion Private
}