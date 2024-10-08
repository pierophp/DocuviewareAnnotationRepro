using GdPicture14;

namespace DocuviewareAnnotationRepro.Business;

public class ImageProcessingManager
{
    public static GdPictureStatus ManageImageOrientationWithGDPicture(int imageId, GdPictureImaging oGdPictureImaging)
    {
        GdPictureRotateFlipType orientation = oGdPictureImaging.TagGetExifRotation(imageId);
        var status = oGdPictureImaging.Rotate(imageId, orientation);
        var tagId = GetExifOrientationTagId(imageId, oGdPictureImaging);
        if (tagId > 0)
        {
            oGdPictureImaging.TagDelete(imageId, tagId);
        }
        return status;
    }

    public static int GetExifOrientationTagId(int imageId, GdPictureImaging oGdPictureImaging)
    {
        for (int i = 1; i <= oGdPictureImaging.TagCount(imageId); i++)
        {
            if (oGdPictureImaging.TagGetName(imageId, i) == "Orientation")
            {
                return i;
            }
        }

        return 0;
    }

}

