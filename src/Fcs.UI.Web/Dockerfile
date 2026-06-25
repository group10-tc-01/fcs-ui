FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Packages.props", "."]
COPY ["Fcs.UI.slnx", "."]
COPY ["src/Fcs.UI.Domain/Fcs.UI.Domain.csproj", "src/Fcs.UI.Domain/"]
COPY ["src/Fcs.UI.Cache/Fcs.UI.Cache.csproj", "src/Fcs.UI.Cache/"]
COPY ["src/Fcs.UI.Application/Fcs.UI.Application.csproj", "src/Fcs.UI.Application/"]
COPY ["src/Fcs.UI.Infrastructure/Fcs.UI.Infrastructure.csproj", "src/Fcs.UI.Infrastructure/"]
COPY ["src/Fcs.UI.Shared/Fcs.UI.Shared.csproj", "src/Fcs.UI.Shared/"]
COPY ["src/Fcs.UI.Web/Fcs.UI.Web.csproj", "src/Fcs.UI.Web/"]
RUN dotnet restore "src/Fcs.UI.Web/Fcs.UI.Web.csproj"

COPY . .
RUN dotnet publish "src/Fcs.UI.Web/Fcs.UI.Web.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY ["src/Fcs.UI.Web/nginx.conf", "/etc/nginx/nginx.conf"]
