FROM mcr.microsoft.com/dotnet/core/sdk:2.2
EXPOSE 5000
ENV APP_ENV="api"
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "run", "--urls", "http://0.0.0.0:5000",  "--network", "host"]