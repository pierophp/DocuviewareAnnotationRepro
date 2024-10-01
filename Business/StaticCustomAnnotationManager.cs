using GdPicture14;
using GdPicture14.WEB;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace DocuviewareAnnotationRepro.Business;

public class StaticCustomAnnotationManager
{
    public static async void SaveAnnotationsFile(CustomActionEventArgs e)
    {
        string bearerToken = string.Empty;
        string fileId = string.Empty;
        string filename = string.Empty;
        string workspaceId = string.Empty;
        var dataJson = e.args.ToString();

        try
        {
            if (!string.IsNullOrEmpty(dataJson))
            {
                dynamic data = JObject.Parse(dataJson);
                bearerToken = data["Authorization"];
                fileId = data["FileId"];
                filename = data["Filename"];
                workspaceId = data["WorkspaceId"];
            }
            else
            {
                e.message = new DocuViewareMessage($"Save document annotations failed. No parameters found.");
                return;
            }

            MemoryStream stream = new MemoryStream();

            var status = e.docuVieware.SaveAnnotations(stream, true);
            if (status == GdPictureStatus.OK)
            {
                var fileSize = stream.Length;
                var xmpFile = stream.ToArray();
                stream.Close();
                stream.Dispose();
                await FileManager.InsertOrUpdateXMPAnnotationsForFile(Guid.Parse(fileId), Guid.Parse(workspaceId), xmpFile, bearerToken, e);

                return;
            }
        }
        catch (Exception ex)
        {
            e.message = new DocuViewareMessage($"Saving annotations for file {filename} failed. Error = {ex.Message}");
        }
    }

    public static void DownloadPdfWithAnnotationsFromImageFile(CustomActionEventArgs e)
    {
        var dataJson = e.args.ToString();
        var filename = string.Empty;
        var environment = string.Empty;

        if (!string.IsNullOrEmpty(dataJson))
        {
            dynamic data = JObject.Parse(dataJson);
            filename = data["Filename"];
        }
        var baseName = Path.GetFileNameWithoutExtension(filename);

        var pdfFileName = $"{baseName}.pdf";
        byte[] fileWithAnnotations = null;
        var burnStatus = e.docuVieware.BurnAnnotations(true);
        if (burnStatus == GdPictureStatus.OK)
        {
            int originalImageId;
            GdPictureStatus statusGD = e.docuVieware.GetNativeImage(out originalImageId);
            if (statusGD == GdPictureStatus.OK)
            {

                using (MemoryStream stream = new MemoryStream())
                {
                    using (GdPictureImaging gdPictureImaging = new GdPictureImaging())
                    {
                        int clonedImageId = gdPictureImaging.CreateClonedGdPictureImage(originalImageId);
                        var orientationStatus = ImageProcessingManager.ManageImageOrientationWithGDPicture(clonedImageId, gdPictureImaging);
                        if (orientationStatus == GdPictureStatus.OK)
                        {
                            var status = gdPictureImaging.SaveAsPDF(clonedImageId, stream, false, baseName, "DocuviewareAnnotationRepro", "", "", "");
                            if (GdPictureStatus.OK != status)
                            {
                                e.message = SetErrorMessage($"Error converting {filename} with the annotations to PDF: {status}");
                            }
                            else
                            {
                                if (stream != null && stream.Length > 0)
                                {
                                    fileWithAnnotations = ReadToEnd(stream);
                                }
                            }
                        }
                        else
                        {
                            e.message = SetErrorMessage($"Error managing image orientation after burning annotations. Status={orientationStatus}");
                        }
                        gdPictureImaging.ReleaseGdPictureImage(clonedImageId);
                    }
                }
            }
            else
            {
                e.message = SetErrorMessage($"Error getting native image from document. File = {filename}. Status = {statusGD}");
            }
        }
        else
        {
            e.message = SetErrorMessage($"Error burning annotations to the document. File = {filename}. Status = {burnStatus}");
        }

        if (fileWithAnnotations != null)
        {
            var resultStr = Convert.ToBase64String(fileWithAnnotations);
            var returnedObj = new object();
            returnedObj = new { filename = pdfFileName, file = resultStr };
            e.result = returnedObj;
        }
    }


    private static byte[] ReadToEnd(System.IO.Stream stream)
    {
        long originalPosition = 0;

        if (stream.CanSeek)
        {
            originalPosition = stream.Position;
            stream.Position = 0;
        }

        try
        {
            byte[] readBuffer = new byte[4096];

            int totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;

                if (totalBytesRead == readBuffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    if (nextByte != -1)
                    {
                        byte[] temp = new byte[readBuffer.Length * 2];
                        Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                        Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                        readBuffer = temp;
                        totalBytesRead++;
                    }
                }
            }

            byte[] buffer = readBuffer;
            if (readBuffer.Length != totalBytesRead)
            {
                buffer = new byte[totalBytesRead];
                Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
            }
            return buffer;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }

    private static DocuViewareMessage SetErrorMessage(string message)
    {
        return new DocuViewareMessage(message, null, 2500, 300, 300, false, "130%", "normal", "#FFFFFF", "none", "none", "48px", DocuViewareMessageIcon.Error);
    }
}

