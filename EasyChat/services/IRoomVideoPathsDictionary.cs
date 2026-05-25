using System.Collections.Concurrent;
namespace EasyChat.Services;
public interface IRoomVideoPathsDictionary<T, U> : IDictionary<T, U> where T : notnull {}

public class RoomVideoPathsDictionary<T, U> : ConcurrentDictionary<T, U>, IRoomVideoPathsDictionary<T, U> where T : notnull
{
    public RoomVideoPathsDictionary() : base() {}
}