# Use a imagem do .NET 8 SDK como base
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Instalar Git
RUN apt-get update && apt-get install -y git

# Expor portas necessárias
EXPOSE 80
EXPOSE 443
EXPOSE 8080
EXPOSE 5269

# Definir o comando padrão
CMD ["tail", "-f", "/dev/null"]
