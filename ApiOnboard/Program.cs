using System;
using System.Text;
using ApiOnboard;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner.
// Saiba mais sobre como configurar o Swagger/OpenAPI em https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura o pipeline de requisição HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Configuração da conexão com o RabbitMQ
var connectionFactory = new ConnectionFactory
{
    HostName = "rabbitmq", // Host do RabbitMQ
    UserName = "guest",    // Usuário do RabbitMQ
    Password = "guest"     // Senha do RabbitMQ
};

// Mapeia o endpoint POST para "/weatherforecast"
app.MapPost("/weatherforecast", async (User user) =>
{
    try
    {
        string exchange = "amq.topic"; // Nome da exchange

        // Cria uma conexão e um canal com o RabbitMQ
        using (var connection = connectionFactory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Declara a exchange de tópico
            channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true);

            string queueName = "qusuarios"; // Nome da fila

            // Parâmetros adicionais para a fila
            var queueArgs = new Dictionary<string, object>
            {
                { "x-max-length", 20 } // Define o número máximo de mensagens na fila
            };

            // Declara a fila
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);

            // Serializa o objeto user para JSON
            string message = Newtonsoft.Json.JsonConvert.SerializeObject(user);
            var body = Encoding.UTF8.GetBytes(message);

            // Publica a mensagem na exchange de tópico
            channel.BasicPublish(exchange: exchange, routingKey: "eduardo.evento", basicProperties: null, body: body);
        }

        // Cria uma resposta de sucesso com os dados do usuário e uma mensagem
        var response = new { dados = new { user.Name, user.Email, user.Phone }, mensagem = "Dados enviados com sucesso!"};
        return Results.Ok(response); // Retorna a resposta com status 200 (OK)
    }
    catch (Exception ex)
    {
        // Loga o erro e retorna uma resposta de erro
        Console.WriteLine($"Erro ao enviar mensagem para RabbitMQ: {ex.Message}");
        return Results.BadRequest(new { error = "Erro ao enviar mensagem para RabbitMQ" }); // Retorna a resposta com status 400 (Bad Request)
    }
})
.WithOpenApi();

app.Run(); // Executa a aplicação
