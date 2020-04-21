FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /gauss

# copy csproj and restore as distinct layers
COPY ./ ./
RUN dotnet restore

# copy everything else and build
COPY GaussBell/ ./
RUN dotnet publish -c Release -o out

# build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /opt/cavitos/gauss
COPY --from=build /gauss/out ./

RUN useradd -m gauss \
    && chown -R gauss:gauss /opt/cavitos/gauss

USER gauss

EXPOSE 5000
ENTRYPOINT ["dotnet", "GaussBell.dll"]
