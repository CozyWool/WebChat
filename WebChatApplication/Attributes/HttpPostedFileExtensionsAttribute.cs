using System.ComponentModel.DataAnnotations;

namespace WebChatApplication.Attributes;

public class HttpPostedFileExtensionsAttribute : DataTypeAttribute
{
    private readonly FileExtensionsAttribute _innerAttribute = new();

    public HttpPostedFileExtensionsAttribute() : base(DataType.Upload)
    {
        ErrorMessage = _innerAttribute.ErrorMessage;
    }

    public string Extensions
    {
        get => _innerAttribute.Extensions;
        set => _innerAttribute.Extensions = value;
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return _innerAttribute.IsValid(null);
        }
        
        if (value is not IFormFile formFile)
        {
            return false;
        }

        var fileName = formFile.FileName;
        return _innerAttribute.IsValid(fileName);
    }
}