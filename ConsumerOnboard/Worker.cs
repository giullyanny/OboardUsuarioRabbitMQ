using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ConsumerOnboard.Data;
using ConsumerOnboard.Entities;
using Newtonsoft.Json;

namespace ConsumerOnboard;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly string _queueName;
    private readonly string _exchange;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _exchange = "amq.topic";
        _queueName = "qusuarios";

        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq", // Host do RabbitMQ
            UserName = "guest",     // Usuário do RabbitMQ
            Password = "guest"      // Senha do RabbitMQ
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Topic, durable: true); // Declara o exchange do tipo tópico, durável

        var queueArgs = new Dictionary<string, object> { { "x-max-length", 20 } }; // Argumento para limitar o tamanho da fila

        _channel.QueueDeclare(queue: _queueName,
                             durable: true, // A fila é durável
                             exclusive: false, // Não é exclusiva
                             autoDelete: false, // Não será deletada automaticamente
                             arguments: queueArgs); // Argumentos adicionais
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Registra a ação a ser realizada quando o serviço for cancelado
        stoppingToken.Register(() =>
        {
            _channel.Close();
            _connection.Close();
        });

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation($"Received message: {message}"); // Log da mensagem recebida

            // Processa a mensagem (por exemplo, salva no banco de dados)
            await ProcessMessage(message);
        };

        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer); // Começa a consumir mensagens da fila

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken); // Espera por 1 segundo antes de continuar, respeitando o token de cancelamento
        }
    }

    private async Task ProcessMessage(string message)
    {
        // Desserializa a mensagem para o objeto específico
        var user = JsonConvert.DeserializeObject<User>(message);

        user.DataCriacao = DateTime.Now;

        // Salva o usuário no banco de dados
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }
    }

    public override void Dispose()
    {
        _channel.Dispose(); // Libera o canal
        _connection.Dispose(); // Libera a conexão
        base.Dispose();
    }
}
