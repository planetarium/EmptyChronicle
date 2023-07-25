FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy AS build-env
WORKDIR /app
ARG COMMIT

COPY ./lib9c/Lib9c/Lib9c.csproj ./Lib9c/
COPY ./EmptyChronicle/EmptyChronicle.csproj ./EmptyChronicle/
RUN dotnet restore Lib9c
RUN dotnet restore EmptyChronicle

COPY . ./
RUN dotnet publish EmptyChronicle/EmptyChronicle.csproj \
    -c Release \
    -r linux-arm64 \
    -o out \
    --self-contained \
    --version-suffix $COMMIT


FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
RUN apt-get update && apt-get install -y libc6-dev
COPY --from=build-env /app/out .

RUN apt-get update && \
    apt-get install -y --allow-unauthenticated libc6-dev jq curl && \
    rm -rf /var/lib/apt/lists/* 

VOLUME /data

#ENTRYPOINT ["dotnet", "EmptyChronicle.dll"]
