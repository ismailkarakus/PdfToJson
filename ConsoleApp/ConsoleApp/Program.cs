using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        string pdfPath = "C:\\Users\\SampleUser\\Documents\\Sample.pdf"; // PDF dosyasının yolu
        string jsonOutputPath = "output.json"; // Çıktı JSON dosyası

        try
        {
            var rawText = ExtractTextFromPdf(pdfPath);
            var processedData = ProcessText(rawText);
            SaveToJson(processedData, jsonOutputPath);
            Console.WriteLine("İşlem başarılı, JSON dosyası oluşturuldu.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
        }
    }

    static string ExtractTextFromPdf(string pdfPath)
    {
        using (PdfReader reader = new PdfReader(pdfPath))
        using (PdfDocument pdfDoc = new PdfDocument(reader))
        {
            string text = string.Empty;
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                text += PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i));
            }
            return text;
        }
    }

    static string ProcessText(string text)
    {
        // Gürültü, reklam, filigran ve zehirli içeriklerin temizlenmesi
        text = RemoveNoise(text);
        text = RemoveAdvertisements(text);
        text = RemoveWatermarks(text);
        text = RemoveToxicContent(text);

        // Markdown'a dönüşüm ve LaTeX formüllerinin işlenmesi
        text = ConvertToMarkdown(text);
        text = ConvertFormulasToLatex(text);

        // Cümle uzunluğunu kontrol etme
        if (text.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length < 100)
        {
            throw new Exception("Metin en az 100 kelime içermelidir.");
        }

        return text;
    }

    static string RemoveNoise(string text)
    {
        // Gürültü tanımlayıcı garip semboller ve anlamsız içerikleri kaldırma
        return Regex.Replace(text, @"[^a-zA-Z0-9\s,.!?]+", string.Empty);
    }

    static string RemoveAdvertisements(string text)
    {
        // Reklam içeriklerini kaldırma
        return Regex.Replace(text, @"\b(advertisement|promo|ad)\b.*?(\n|\r|$)", string.Empty, RegexOptions.IgnoreCase);
    }

    static string RemoveWatermarks(string text)
    {
        // Filigranları kaldırma
        return Regex.Replace(text, @"\b(phone|email|address)\b:?\s*.*?(\n|\r|$)", string.Empty, RegexOptions.IgnoreCase);
    }

    static string RemoveToxicContent(string text)
    {
        // Zehirli içerikleri kaldırma
        string[] toxicKeywords = { "pornography", "gambling", "drugs", "fraud", "anti-party", "anti-government" };
        foreach (var keyword in toxicKeywords)
        {
            text = Regex.Replace(text, $@"\b{keyword}\b.*?(\n|\r|$)", string.Empty, RegexOptions.IgnoreCase);
        }
        return text;
    }

    static string ConvertToMarkdown(string text)
    {
        // HTML'den Markdown'a dönüştürme işlemi
        text = text.Replace("<b>", "**").Replace("</b>", "**");
        text = text.Replace("<i>", "*").Replace("</i>", "*");
        // Diğer HTML etiketlerini burada işleyin
        return text;
    }

    static string ConvertFormulasToLatex(string text)
    {
        // Formülleri LaTeX formatına dönüştürme
        return Regex.Replace(text, @"\[(.*?)\]", @"$$$1$$$"); // Örneğin, [formül] → $$formül$$
    }

    static void SaveToJson(string data, string outputPath)
    {
        var jsonData = new { Content = data };
        string jsonString = JsonConvert.SerializeObject(jsonData, Formatting.Indented);

        File.WriteAllText(outputPath, jsonString);
    }
}
