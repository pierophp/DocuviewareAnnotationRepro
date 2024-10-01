namespace DocuviewareAnnotationRepro.Models;

public class FileDataAndInfo
{
    public FileDataAndInfo()
    {
        FileStream = null;
        FileType = string.Empty;
        FileName = string.Empty;
    }
    public Stream FileStream { get; set; }
    public string FileType { get; set; }
    public string FileName { get; set; }
}

