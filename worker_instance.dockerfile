FROM mcr.microsoft.com/dotnet/core/sdk:2.2
ENV APP_ENV="worker"
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "run", "--network", "host"]