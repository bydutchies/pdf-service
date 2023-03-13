using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using PuppeteerSharp;
using System.IO;
using System.Threading.Tasks;

namespace MyPdfService;

internal class UrlToPdf
{
  private readonly PuppeteerOptions _options;

  public UrlToPdf(PuppeteerOptions options)
  {
    _options = options;
  }

  [FunctionName("ConvertUrlToPdf")]
  public async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "convert/url-to-pdf")] HttpRequest request)
  {
    string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
    dynamic postData = JsonConvert.DeserializeObject(requestBody);

    string url = postData?.url;
    if (string.IsNullOrEmpty(url))
      return new BadRequestResult();

    // Launch browser
    var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
      Headless = true,
      ExecutablePath = _options.BrowserExecutablePath
    });
    var page = await browser.NewPageAsync();

    // Load html from url
    await page.GoToAsync(url);

    // Generate pdf
    var options = new PdfOptions
    {
      PreferCSSPageSize = _options.PreferCSSPageSize,
      PrintBackground = _options.PrintBackground
    };
    var stream = await page.PdfStreamAsync(options);

    await browser.CloseAsync();

    return new FileStreamResult(stream, "application/pdf");
  }
}
