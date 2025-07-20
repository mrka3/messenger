using Google.Protobuf.WellKnownTypes;
using Grpc;
using Grpc.Core;
using Library.Storage.Abstractions;

namespace Messenger.Grpc;

using Storage = Library.Storage.Models;

/// <summary>
/// Grpc-сервис месседжера.
/// </summary>
public sealed class MessengerService : global::Grpc.Messenger.MessengerBase
{
    /// <summary>
    /// Создает экземпляр типа <see cref="MessengerService"/>.
    /// </summary>
    /// <param name="messageRepository">
    /// Репозиторий сообщений.
    /// </param>
    /// <param name="userRepository">
    /// Репозиторий пользователей.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Если один из параметров <see langword="null"/>.
    /// </exception>
    public MessengerService(
        IBaseRepository<Storage.Message> messageRepository,
        IBaseRepository<Storage.User> userRepository)
    {
        ArgumentNullException.ThrowIfNull(messageRepository);
        ArgumentNullException.ThrowIfNull(userRepository);
        
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Авторизует пользователя.
    /// </summary>
    /// <param name="request">
    /// Тело запроса.
    /// </param>
    /// <param name="context">
    /// Контекст запроса.
    /// </param>
    public override Task<Empty> AuthorizeUser(User request, ServerCallContext context)
    {
        _userRepository.Save(new Storage.User
        {
            Username = request.Name,
            Email = request.Email
        });

        return Task.FromResult(new Empty());
    }

    /// <summary>
    /// Добавляет сообщение в базу.
    /// </summary>
    /// <param name="request">
    /// Тело запроса.
    /// </param>
    /// <param name="context">
    /// Контекст запроса.
    /// </param>
    public override Task<MessageId> AddMessage(Message request, ServerCallContext context)
    {
        var messageId = _messageRepository.Save(new Storage.Message
        {
            Sender = request.Sender,
            Target = request.Target,
            Text = request.Text,
            Timestamp = DateTimeOffset.Now,
            IsPersonal = request.IsPersonal,
            IsRead = false
        });

        return Task.FromResult(new MessageId
        {
            Value = messageId
        });
    }

    /// <summary>
    /// Прочитывает сообщение.
    /// </summary>
    /// <param name="request">
    /// Тело запроса.
    /// </param>
    /// <param name="context">
    /// Контекст запроса.
    /// </param>
    public override Task<Empty> ReadMessage(MessageId request, ServerCallContext context)
    {
        var message = _messageRepository.Get(request.Value);

        message.IsRead = true;

        _messageRepository.Save(message);

        return Task.FromResult(new Empty());
    }

    /// <summary>
    /// Меняет текст сообщения.
    /// </summary>
    /// <param name="request">
    /// Тело запроса.
    /// </param>
    /// <param name="context">
    /// Контекст запроса.
    /// </param>
    public override Task<Empty> ChangeTextMessage(ChangeMessageBody request, ServerCallContext context)
    {
        var message = _messageRepository.Get(request.MessageId);

        message.Text = request.Text;

        _messageRepository.Save(message);

        return Task.FromResult(new Empty());
    }

    /// <summary>
    /// Удаляет сообщение.
    /// </summary>
    /// <param name="request">
    /// Тело запроса.
    /// </param>
    /// <param name="context">
    /// Контекст запроса.
    /// </param>
    public override Task<Empty> DeleteMessage(MessageId request, ServerCallContext context)
    {
        var message = _messageRepository.Get(request.Value);

        message.IsDeleted = true;

        _messageRepository.Save(message);

        return Task.FromResult(new Empty());
    }

    /// <summary>
    /// Получает историю сообщения.
    /// </summary>
    /// <param name="request">
    /// Тело запроса.
    /// </param>
    /// <param name="context">
    /// Контекст запроса.
    /// </param>
    public override Task<HistoryCollection> History(GroupName request, ServerCallContext context)
    {
        var messages = _messageRepository.GetAll(x => x.Target == request.Name && x.IsDeleted == false);

        var result = new HistoryCollection()
        {
            History =
            {
                messages.Select(x =>
                    new MessageHistory
                    {
                        Id = x.Id!.Value,
                        Text = x.Text
                    })
            }
        };

        return Task.FromResult(result);
    }

    private readonly IBaseRepository<Storage.Message> _messageRepository;
    private readonly IBaseRepository<Storage.User> _userRepository;
}