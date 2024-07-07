using Microsoft.EntityFrameworkCore;
using ConsumerOnboard;
using ConsumerOnboard.Data;

var builder = Host.CreateApplicationBuilder(args);

// Configura o DbContext para usar o SQL Server com a string de conexão especificada nas configurações
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adiciona o serviço em segundo plano (Worker) ao contêiner de serviços
builder.Services.AddHostedService<Worker>();

var host = builder.Build(); // Constrói o host
host.Run(); // Executa o host
