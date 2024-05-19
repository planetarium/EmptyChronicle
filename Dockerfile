FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build-env
ARG TARGETPLATFORM
ARG BUILDPLATFORM
WORKDIR /app

COPY ./EmptyChronicle/EmptyChronicle.csproj ./EmptyChronicle/

COPY . ./
RUN <<EOF
#!/bin/bash
echo "TARGETPLATFROM=$TARGETPLATFORM"
echo "BUILDPLATFORM=$BUILDPLATFORM"
if [[ "$TARGETPLATFORM" = "linux/amd64" ]]
then
  dotnet publish EmptyChronicle/EmptyChronicle.csproj \
    -c Release \
    -r linux-x64 \
    -o out \
    --self-contained
elif [[ "$TARGETPLATFORM" = "linux/arm64" ]]
then
  dotnet publish EmptyChronicle/EmptyChronicle.csproj \
    -c Release \
    -r linux-arm64 \
    -o out \
    --self-contained
else
  echo "Not supported target platform: '$TARGETPLATFORM'."
  exit -1
fi
EOF

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

RUN apt-get update && \
    apt-get install -y --allow-unauthenticated libc6-dev libsnappy-dev jq curl && \
    rm -rf /var/lib/apt/lists/*

VOLUME /data

ENTRYPOINT ["dotnet", "EmptyChronicle.dll"]
