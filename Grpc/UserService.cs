using Google.Protobuf.WellKnownTypes;
using Grpc;
using Grpc.Core;
using Library.Storage.Abstractions;

namespace Messenger.Grpc;

using Storage = Library.Storage.Models;

/// <summary>
/// Grpc-сервис месседжера.
/// </summary>
public sealed class UserService : Users.UsersBase
{
    /// <summary>
    /// Создает экземпляр типа <see cref="UserService"/>.
    /// </summary>
    /// <param name="userRepository">
    /// Репозиторий пользователей.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Если один из параметров <see langword="null"/>.
    /// </exception>
    public UserService(IBaseRepository<Storage.User> userRepository)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        
        _userRepository = userRepository;
    }

    /// <summary>
    /// Получает пользователей системы.
    /// </summary>
    /// <param name="request">
    /// Тело запроса.
    /// </param>
    /// <param name="context">
    /// Контекст запроса.
    /// </param>
    public override Task<UserCollection> GetUsers(Empty request, ServerCallContext context)
    {
        var users = _userRepository.GetAll()
            .Select(x => new User
            {
                Name = x.Username
            }).ToList();

        var collection = new UserCollection()
        {
            Users = { users }
        };
        
        return Task.FromResult(collection);
    }

    private readonly IBaseRepository<Storage.User> _userRepository;
}