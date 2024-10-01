using DocuviewareAnnotationRepro.Business;
using GdPicture14.WEB;
using DotNetEnv;

namespace DocuviewareAnnotationRepro;

public class Program
{
    public static void Main(string[] args)
    {
        Env.Load();
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        SetDocuvieware();

        app.Run();
    }

    public static string GetCacheDirectory()
    {
        return Directory.GetCurrentDirectory() + "\\cache";
    }

    public static string GetDocumentsDirectory()
    {
        return Directory.GetCurrentDirectory() + "\\documents";
    }
    public static void SetDocuvieware()
    {
        var appUri = "http://localhost:1824";

        DocuViewareLicensing.RegisterKEY(Environment.GetEnvironmentVariable("DocuviewareLicenseKey"));
        var cacheFolder = GetCacheDirectory();            
        var stickSession = true;
        var stateMode = DocuViewareSessionStateMode.InProc;
        DocuViewareManager.SetupConfiguration(stickSession, stateMode, cacheFolder, appUri, "DocuVieware");
        DocuViewareEventsHandler.CustomAction += CustomActionDispatcher;
    }

    public static void CustomActionDispatcher(object sender, CustomActionEventArgs e)
    {
        if (e.actionName.Equals("saveAnnotations"))
        {
            StaticCustomAnnotationManager.SaveAnnotationsFile(e);
        }
        else if (e.actionName.Equals("DownloadPdfWithAnnotationsFromImageFile"))
        {
            StaticCustomAnnotationManager.DownloadPdfWithAnnotationsFromImageFile(e);
        }
    }
}
