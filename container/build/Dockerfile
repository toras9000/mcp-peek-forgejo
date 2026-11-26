FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:11.0 AS builder
ARG TARGETARCH

WORKDIR /work

COPY ./*.csproj ./src/
RUN dotnet restore \
    --arch $TARGETARCH \
    ./src

COPY ./ ./src/
RUN dotnet publish \
    --configuration Release \
    --output ./publish \
    --arch $TARGETARCH \
    --no-restore \
    ./src


FROM mcr.microsoft.com/dotnet/aspnet:11.0

WORKDIR /app
COPY --from=builder  /work/publish  /app

CMD ["dotnet", "/app/mcp-peek-forgejo.dll"]
