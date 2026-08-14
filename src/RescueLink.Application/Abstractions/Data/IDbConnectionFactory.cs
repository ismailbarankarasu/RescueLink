using System.Data.Common;

namespace RescueLink.Application.Abstractions.Data;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}