FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY EnterpriseHub.slnx ./
COPY src/EnterpriseHub.Domain/EnterpriseHub.Domain.csproj src/EnterpriseHub.Domain/
COPY src/EnterpriseHub.Application/EnterpriseHub.Application.csproj src/EnterpriseHub.Application/
COPY src/EnterpriseHub.Infrastructure/EnterpriseHub.Infrastructure.csproj src/EnterpriseHub.Infrastructure/
COPY src/EnterpriseHub.API/EnterpriseHub.API.csproj src/EnterpriseHub.API/
RUN dotnet restore src/EnterpriseHub.API/EnterpriseHub.API.csproj

# Copy remaining source files
COPY src/EnterpriseHub.Domain/ src/EnterpriseHub.Domain/
COPY src/EnterpriseHub.Application/ src/EnterpriseHub.Application/
COPY src/EnterpriseHub.Infrastructure/ src/EnterpriseHub.Infrastructure/
COPY src/EnterpriseHub.API/ src/EnterpriseHub.API/

RUN dotnet publish src/EnterpriseHub.API/EnterpriseHub.API.csproj -c Release -o /app/publish --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EnterpriseHub.API.dll"]
