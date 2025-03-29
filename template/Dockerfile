 FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG TARGETARCH
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore -a $TARGETARCH

# copy and publish app and libraries
COPY . .
RUN dotnet publish -a $TARGETARCH --no-restore -o /app


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["./k8soperator", "operator"]
