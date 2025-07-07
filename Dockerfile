# Usar la imagen oficial de .NET 8.0 SDK para la construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar archivos de proyecto y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del código fuente
COPY . ./

# Construir la aplicación
RUN dotnet publish -c Release -o out

# Usar la imagen runtime para la ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copiar los archivos publicados desde la etapa de construcción
COPY --from=build /app/out .

# Crear directorio para archivos estáticos
RUN mkdir -p wwwroot/images/profiles wwwroot/images/vehicles

# Exponer el puerto que usa la aplicación
EXPOSE 10000

# Establecer variables de entorno para producción
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:10000

# Comando para ejecutar la aplicación
ENTRYPOINT ["dotnet", "BackendRent2Go.dll"]
