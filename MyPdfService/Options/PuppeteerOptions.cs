namespace MyPdfService;

internal class PuppeteerOptions
{
  public string BrowserExecutablePath { get; set; }
  public bool PreferCSSPageSize { get; set; } = true;
  public bool PrintBackground { get; set; } = true;
}

