using System;

namespace Lazybook.Api.DTOs;

public class AccountPictureRequest
{
    public required IFormFile Image { get; init; }
}