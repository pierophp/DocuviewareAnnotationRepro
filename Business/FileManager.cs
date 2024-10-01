using DocuviewareAnnotationRepro.Models;
using GdPicture14.WEB;
using System.Net;

namespace DocuviewareAnnotationRepro.Business;

public class FileManager
{
    public FileManager()
    {}

    public FileDataAndInfo GetFileGivenGuid(Guid fileId)
    {   
        string filePath = $@"wwwroot\documents\{fileId}.webp";

        FileDataAndInfo file = new FileDataAndInfo();
        file.FileName = fileId.ToString() + ".webp";
        file.FileType = "image/webp";
        file.FileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);;

        return file;

    }
   
    public static async Task InsertOrUpdateXMPAnnotationsForFile(Guid assetFileId, Guid workspaceId, byte[] xmpFile, string bearerToken, CustomActionEventArgs e)
    {
        try
        {
            //var uri = AppSettingsStaticProviderURI;
            //string url = $@"{uri}/api/Annotations/InsertOrUpdateXMPAnnotationsForFile?fileId={assetFileId}&workspaceId={workspaceId}";
            //WebRequest request = WebRequest.Create(url);
            //request.Timeout = 300000;
            //request.Method = "POST";
            //request.Headers.Add("Authorization", bearerToken);
            //request.ContentType = "application/octet-stream";
            //request.ContentLength = xmpFile.Length;

            //((HttpWebRequest)request).AllowWriteStreamBuffering = true;
            //using (Stream dataStream = request.GetRequestStream())
            //{
            //    dataStream.Write(xmpFile, 0, xmpFile.Length);
            //}

            //var result = await request.GetResponseAsync();
            //e.message = new DocuViewareMessage($"Annotations file saved with success!");

        }
        catch (Exception ex)
        {
            e.message = new DocuViewareMessage($"Fail to save the annotations file in the database. Message ={ex.Message}");
        }
    }

}


