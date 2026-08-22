using RescueLink.Application.Common.Results;

namespace RescueLink.API.Common;

public interface IErrorLocalizer
{
    Error Localize(Error error);
}