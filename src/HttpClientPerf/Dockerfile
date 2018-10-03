#--------------Backend Build--------------
FROM microsoft/dotnet:2.1.302-sdk-stretch AS build

WORKDIR /src

COPY ./HttpClientPerf.csproj .
RUN dotnet restore

COPY ./ ./
RUN dotnet publish -c Release -o /app  -r debian.9-x64
#--------------Runtime Image--------------
FROM microsoft/dotnet:2.1.2-runtime-deps-stretch-slim AS base

WORKDIR /app
# Copy C# back-end 
COPY --from=build /app .

ENTRYPOINT ["./HttpClientPerf"]