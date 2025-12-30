namespace ComicWeb.Infrastructure.Storage;

public sealed class StorageSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ImagePath { get; set; } = "wwwroot/uploads/images";
    public int MaxFileSizeMb { get; set; } = 10;
}
