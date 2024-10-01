namespace DocuviewareAnnotationRepro.Models;

public class DocuViewareConfiguration
{
    public string SessionId { get; set; }
    public string ControlId { get; set; }
    public Guid FileId { get; set; }
    public bool ShowAnnotationsSnapIn { get; set; }
    public Guid AnnotationsFileId { get; set; }

}

