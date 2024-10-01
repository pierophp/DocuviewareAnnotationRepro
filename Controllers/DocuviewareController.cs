using Microsoft.AspNetCore.Mvc;
using DocuviewareAnnotationRepro.Models;
using GdPicture14.WEB;
using System.Text;
using DocuviewareAnnotationRepro.Business;

namespace DocuviewareAnnotationRepro.Controllers;

[ApiController]
[Route("DocuVieware")]
public class DocuviewareController : Controller
{
    private readonly ILogger<DocuviewareController> _logger;

    public DocuviewareController(ILogger<DocuviewareController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [Route("GetDocuViewareControlForFile")]
    public async Task<DocuViewareRESTOutputResponse> GetDocuViewareControlForFile(DocuViewareConfiguration controlConfigurationForFile)
    {
        FileDataAndInfo fileDataAndInfo = null;
        try
        {
            if (!DocuViewareManager.IsSessionAlive(controlConfigurationForFile.SessionId))
            {
                if (!string.IsNullOrEmpty(controlConfigurationForFile.SessionId) && !string.IsNullOrEmpty(controlConfigurationForFile.ControlId))
                {
                    int docuviewareSessionTimeOutInMinutes = 20;
                    DocuViewareManager.CreateDocuViewareSession(controlConfigurationForFile.SessionId, controlConfigurationForFile.ControlId, docuviewareSessionTimeOutInMinutes);
                    _logger.LogInformation($"DocuVieware session created. Id= {controlConfigurationForFile.SessionId} controlId={controlConfigurationForFile.ControlId}");
                }
                else
                {
                    _logger.LogError("Invalid session identifier {sessionId} and/or invalid control identifier {controlId}.", controlConfigurationForFile.SessionId,
                        controlConfigurationForFile.ControlId);
                    return FileRequestFailed(string.Empty, "Invalid session identifier and/or invalid control identifier.");
                }
            }

            _logger.LogInformation($"DocuVieware: get control file for file ID ={controlConfigurationForFile.FileId}");
            var enableAnnotations = controlConfigurationForFile.ShowAnnotationsSnapIn;
            var fileManager = new FileManager();
        
            using (DocuViewareControl docuVieware = new DocuViewareControl(controlConfigurationForFile.SessionId))
            {
                docuVieware.AllowPrint = false;
                docuVieware.EnablePrintButton = false;
                docuVieware.AllowUpload = false;
                docuVieware.EnableFileUploadButton = false;
                docuVieware.CollapsedSnapIn = false;
                docuVieware.EnableRotateButtons = false;

                docuVieware.EnableZoomButtons = true;
                docuVieware.EnablePageViewButtons = true;
                docuVieware.EnableMouseModeButtons = false;
                docuVieware.EnableFormFieldsEdition = false;
                docuVieware.EnableTwainAcquisitionButton = false;

                docuVieware.EnableLoadFromUriButton = false;
                docuVieware.EnableSaveButton = false;
                docuVieware.ShowDigitalSignatureSnapIn = false;

                docuVieware.MaxUploadSize = int.MaxValue;
                docuVieware.MaxDownloadSize = int.MaxValue;
                docuVieware.OpenZoomMode = GdPicture14.ViewerZoomMode.ZoomModeFitToViewer;

                fileDataAndInfo = fileManager.GetFileGivenGuid(controlConfigurationForFile.FileId);
                if (fileDataAndInfo != null && fileDataAndInfo.FileStream != null)
                {
                    _logger.LogInformation($"GetDocuViewareControlForFile: file returned by Server: file name = {fileDataAndInfo.FileName} length = { fileDataAndInfo.FileStream.Length}");
                }
                else
                {
                    _logger.LogInformation($"GetDocuViewareControlForFile: file returned by Server is null.");
                }
                docuVieware.EnableAnnotationActionButtons = enableAnnotations;
                docuVieware.EnableFreehandHighlighterAnnotationButton = enableAnnotations;
                docuVieware.EnableSelectedTextAnnotationEdition = enableAnnotations;
                docuVieware.EnableTextSelectionAnnotation = enableAnnotations;
                docuVieware.ShowBookmarksSnapIn = enableAnnotations;
                docuVieware.ShowSnapInButtonStrip = enableAnnotations;
                docuVieware.ShowSnapInCollapseButton = enableAnnotations;
                docuVieware.ShowTextSearchSnapIn = enableAnnotations;
                docuVieware.ShowThumbnailsSnapIn = enableAnnotations;
                docuVieware.EnableMultipleThumbnailSelection = enableAnnotations;
                docuVieware.ShowAnnotationsCommentsSnapIn = enableAnnotations;
                docuVieware.ShowAnnotationsSnapIn = enableAnnotations;
                docuVieware.ShowRedactionSnapIn = false;
                docuVieware.EnablePrintButton = enableAnnotations;
                docuVieware.DisableAnnotationPrinting = false;
                docuVieware.AnnotationDropShadow = enableAnnotations;
                docuVieware.AllowPrint = enableAnnotations;
                docuVieware.EnablePrintToPDF = false;

                if (fileDataAndInfo.FileStream != null)
                {

                    if (fileDataAndInfo.FileStream.Length > docuVieware.MaxUploadSize)
                    {
                        return FileRequestFailed(fileDataAndInfo.FileName, "File size exceeds the maximum limit!");
                    }
                    var trimmedFilename = string.Concat(fileDataAndInfo.FileName.Split('"'));
                    var status = docuVieware.LoadFromStream(fileDataAndInfo.FileStream, false, trimmedFilename);
                    if (status != GdPicture14.GdPictureStatus.OK)
                    {
                        _logger.LogError($"Unable to load file from stream. Docuvieware status = {status}");
                        return FileRequestFailed(fileDataAndInfo.FileName);
                    }

                    if (!controlConfigurationForFile.AnnotationsFileId.Equals(Guid.Empty))
                    {
                       
                        FileDataAndInfo annotationsFileDataAndInfo = fileManager.GetFileGivenGuid(controlConfigurationForFile.AnnotationsFileId);
                        if (annotationsFileDataAndInfo.FileStream != null)
                        {
                            var statusLoad = docuVieware.LoadAnnotations(annotationsFileDataAndInfo.FileStream);
                            if (statusLoad != GdPicture14.GdPictureStatus.OK)
                            {
                                _logger.LogError($"Unable to load annotation file from stream. Docuvieware status = {status}");
                            }
                        }
                    }
                }
                else
                {
                    return FileRequestFailed();
                }

                using (StringWriter controlOutput = new StringWriter())
                {
                    docuVieware.RenderControl(controlOutput);
                    DocuViewareRESTOutputResponse output = new DocuViewareRESTOutputResponse
                    {
                        HtmlContent = controlOutput.ToString()
                    };
                    return output;
                }
            }
        }
        catch (Exception e)
        {
            var name = fileDataAndInfo != null ? fileDataAndInfo.FileName : string.Empty;

            _logger.LogError("Error in the generation of {fileName} image. Exception ={msg}", name, e.Message);
            return FileRequestFailed(name, e.Message);
        }
    }

    private DocuViewareRESTOutputResponse FileRequestFailed(string fileName = "", string errorMsg = "")
    {

        var sb = new StringBuilder();
        sb.Append("<html>");
        sb.Append("<div style:\"display:flex; flex-direction:column;padding:5em;");
        sb.Append("<div style=\"margin-left:40%;margin-top:5%;font-size: 4rem;font-weight: bold;\">Oh no!</div>");
        sb.Append("<div style=\"width:400px; display:flex; margin-top: 5%; margin-left: 40%; border: none; flex-direction: column; align-content: center;\">");
        if (!string.IsNullOrEmpty(fileName))
        {
            sb.Append("  <span style=\"font-size: 1.4rem;\"> We couldn't load the file ");
            sb.Append(fileName);
            sb.Append(" </span>");
        }
        else
        {
            sb.Append("  <span style=\"font-size: 1.4rem;\"> We couldn't load the file you were looking for.</span>");

        }
        if (!string.IsNullOrEmpty(errorMsg))
        {
            sb.Append("  <span style=\"font-size: 1.4rem;\"> Error message: ");
            sb.Append(errorMsg);
            sb.Append(" </span>");
        }
        sb.Append("</div>");
        sb.Append("</div>");
        sb.Append("</html>");

        using (StringWriter controlOutput = new StringWriter())
        {
            DocuViewareRESTOutputResponse output = new DocuViewareRESTOutputResponse
            {
                HtmlContent = sb.ToString()
            };
            return output;
        }
    }
}
