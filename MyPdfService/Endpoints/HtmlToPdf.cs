using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using PuppeteerSharp;
using System.IO;
using System.Threading.Tasks;

namespace MyPdfService;

internal class HtmlToPdf
{
  private readonly PuppeteerOptions _options;

  public HtmlToPdf(PuppeteerOptions options)
  {
    _options = options;
  }

  [FunctionName("ConvertHtmlToPdf")]
  public async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "html-to-pdf")] HttpRequest request)
  {
    string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
    dynamic postData = JsonConvert.DeserializeObject<dynamic>(requestBody);

    string html = postData?.html;
    if (string.IsNullOrEmpty(html))
      return new BadRequestResult();

    // Launch browser
    var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
      Headless = true,
      ExecutablePath = _options.BrowserExecutablePath
    });
    var page = await browser.NewPageAsync();

    // Load html and wait until the page is completely loaded
    await page.SetContentAsync(html, new() { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });

    // Generate pdf
    var options = new PdfOptions
    {
      PreferCSSPageSize = _options.PreferCSSPageSize,
      PrintBackground = _options.PrintBackground
    };
    var stream = await page.PdfStreamAsync(options);
    
    // Close browser
    await browser.CloseAsync();

    return new FileStreamResult(stream, "application/pdf");
  }
}
