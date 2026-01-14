namespace CampusEats.Api.Features.Menu.UploadMenuImage;

using MediatR;
using Microsoft.AspNetCore.Hosting;

public class UploadMenuImageHandler(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<UploadMenuImageCommand, UploadMenuImageResult>
{
    public async Task<UploadMenuImageResult> Handle(UploadMenuImageCommand request, CancellationToken cancellationToken)
    {
        if (request.Length <= 0 || request.FileStream is null)
            throw new InvalidOperationException("File is empty.");
        
        var webRoot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsRoot = Path.Combine(webRoot, "menu-images");
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(request.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".jpg";
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using (var fileStream = File.Create(filePath))
        {
            await request.FileStream.CopyToAsync(fileStream, cancellationToken);
        }

        var httpContext = httpContextAccessor.HttpContext!;
        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        var url = $"{baseUrl}/menu-images/{fileName}";

        return new UploadMenuImageResult(url);
    }
}