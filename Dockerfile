ARG VERSION=0.0.0

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSION
WORKDIR /src

COPY ["RaccoltaASM.csproj", "./"]
RUN dotnet restore "RaccoltaASM.csproj"

COPY . .
RUN dotnet publish "RaccoltaASM.csproj" -c Release -o /app/publish /p:Version=$VERSION /p:InformationalVersion=$VERSION

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
ENV TZ=Europe/Rome
RUN apt-get update && apt-get install -y --no-install-recommends tzdata \
	&& rm -rf /var/lib/apt/lists/*
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RaccoltaASM.dll"]
