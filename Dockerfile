FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY HobomSpace.slnx ./
COPY src/HobomSpace.Domain/HobomSpace.Domain.csproj src/HobomSpace.Domain/
COPY src/HobomSpace.Application/HobomSpace.Application.csproj src/HobomSpace.Application/
COPY src/HobomSpace.Infrastructure/HobomSpace.Infrastructure.csproj src/HobomSpace.Infrastructure/
COPY src/HobomSpace.Api/HobomSpace.Api.csproj src/HobomSpace.Api/
RUN dotnet restore src/HobomSpace.Api/HobomSpace.Api.csproj

COPY src/ src/
COPY hobom-buf-proto/ hobom-buf-proto/
RUN dotnet publish src/HobomSpace.Api/HobomSpace.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

RUN groupadd --system --gid 1001 appgroup && \
    useradd --system --uid 1001 --gid appgroup --no-create-home appuser

COPY --from=build /app .

USER appuser
EXPOSE 8080 50052
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "HobomSpace.Api.dll"]
