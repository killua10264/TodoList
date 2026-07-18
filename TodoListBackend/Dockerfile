# 1. Dùng .NET SDK để build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 2. Copy file .csproj và tải các thư viện
COPY *.csproj ./
RUN dotnet restore

# 3. Copy toàn bộ code còn lại và build ra bản chính thức (Release)
COPY . ./
RUN dotnet publish -c Release -o out

# 4. Tạo môi trường chạy (Runtime) siêu nhẹ để đưa lên web
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# 5. Mở cổng 8080 và chạy file DLL
EXPOSE 8080
ENTRYPOINT ["dotnet", "TodoListBackend.dll"]