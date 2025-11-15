namespace Schmeconomics.Api.Time;

public interface IDateTimeProvider
{
    DateTime UtcNow => DateTime.UtcNow;
}
