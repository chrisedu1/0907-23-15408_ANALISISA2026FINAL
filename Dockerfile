FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app


COPY ApiRest/*.csproj ./ApiRest/
RUN dotnet restore ApiRest/*.csproj

COPY . ./
RUN dotnet publish ApiRest/*.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "ApiRest.dll"]