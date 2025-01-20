# Brief Description
This project is made with .NET 8 using the webapi template. It also uses Cloudflare R2 for file uploads and MySQL as its database. If you would like to run this project, make sure to have all the necessary software and .env files needed.

# Docker
My best recommendation is to run this via Docker. If you have Docker installed, first, make sure to check if you have all the .env files that is required. Once you do, go to the same directory as the rest of the .env files and enter the following to your terminal:
```
curl -O https://raw.githubusercontent.com/cedoromal/sample-dotnet-webapi/refs/heads/master/docker-compose.yml
docker compose up -d
```

# No Docker
If you don't have Docker, make sure you have .NET 8 SDK and MySQL correctly configured (this project automatically creates the table and database for you). After which, apply the necessary configuration to the secrets - particularly for the DB connection string. Once you have everything ready, clone this repo and run the following commands:
```
dotnet restore
dotnet run --profile http
```

If you wish to run the frontend, simply clone the [other repo](https://github.com/cedoromal/sample-nextjs-frontend) and run the following (make sure you have pnpm installed):
```
pnpm i
pnpm run dev
```

# Temporary site
If you are still having troubles trying to run the project, or if you simply want to see it without any hassle, check out [sample.cedoromal.com](https://sample.cedoromal.com). I will only be hosting this temporarily (perhaps until February) since it takes resources in my home server.
