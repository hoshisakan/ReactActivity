# Reactivities
## About
Neil Cummings's Udemy Course : The complete guide to building an app use ASP.NET Core 7.0, React 18.2 (with Typescript) and Mobx, and deploy it to the cloud.

## Use Docker to build the following services
* Nginx 1.23.3
* PostgreSQL 15.2
* pgAdmin 7.1
* Dotnet Core 7.0 through Ubuntu 22.10
* Redis 7.0.9
* Node.js 20.1-alpine3.16

## Live Demo
https://reactivities.servepics.com/

* Test user account and password or your can through Google account to login
```
    Email: yll10229@zbock.com
    Password: Pa$$w0rd
```

# Project Description

## Website structure
[Frontend Package Relative Link](./service/web/reactInstalledPackages.txt)
* Frontend: React + Mobx + Semantic UI React + Axios + React Router + Formik + Yup + React Toastify + React Calendar + React Cropper + React Dropzone + React Google Login etc.

[Backend Package Relative Link](./service/web/addPackages.sh)
* Backend: .Net 7 + CQRS + MediatR + Entity Framework Core + AutoMapper + Fluent Validation + SignalR + Identity + Redis + Cloudinary + SendGrid + JWT + Swagger etc.

* Database: PostgreSQL

* Proxy: Nginx + Websocket + SSL + Let's Encrypt

* Container: Docker

## Features
### Home page
* Apply normal login and external login with Google

![HomePage](https://imgur.com/p2ybYvQ.png)

* Register new user

![RegisterForm](https://imgur.com/5FOQwv3.png)

* Register successfully and send email to user for email confirmation

![RegisterSuccessAndSendEmailConfirmation](https://imgur.com/yayOlrc.png)

* ReSend email confirmation

![ReSendEmailConfirmation](https://imgur.com/Qia8CJS.png)


* Email confirmation verify successfully

![EmailConfirmation](https://imgur.com/yJ6La6b.png)

* User Login

![LoginForm](https://imgur.com/ZwcswlG.png)

### Activities page

* Activities page

![ActivitiesPage](https://imgur.com/TbVzbvI.png)

* Create new activity

![CreateNewActivity](https://imgur.com/ybZT2Hc.png)

* View activity detail

![ViewActivityDetail](https://imgur.com/ALfkam4.png)

* Edit activity

![EditActivity](https://imgur.com/CgRcF1l.png)

![EditActivity](https://imgur.com/OamLunI.png)

* Cancel activity

![CancelActivity](https://imgur.com/xC3wgZV.png)

* Chat room

![ChatRoom](https://imgur.com/oGF280O.png)

* Chat room with other user

![ChatRoomWithOtherUser](https://imgur.com/kMJHXNv.png)

### User profile page

* About page

![EditUserProfileAbout](https://imgur.com/rzO0YCa.png)

![SuccessEditUserProfileAbout](https://imgur.com/w2OTiac.png)

* Photos page

![ReSizeAndReviewPhoto](https://imgur.com/xQrBP81.png)

![UploadUserProfilePhoto](https://imgur.com/Vcpky6g.png)

* Events page

![FutureEventsPage](https://imgur.com/Vmj7b8u.png)

![PastEventsPage](https://imgur.com/sTThZ0I.png)

![HostingEventsPage](https://imgur.com/4LDrIPV.png)

* Followers page

![FollowersPage](https://imgur.com/AS5bFh6.png)

* Following page

![FollowersPage](https://imgur.com/HUmmnq1.png)

### Filter activities

* All activities

![AllActivities](https://imgur.com/2k100yM.png)

* Activities by date

![ActivitiesByDate](https://imgur.com/3UEFfOv.png)

* Activities by self host

![ActivitiesBySelfHost](https://imgur.com/QsfC6x3.png)

* Activities by attending

![ActivitiesByAttending](https://imgur.com/cravB6k.png)

## Data Backup Schedule
[Backend Data Backup Schedule Extension](service/web/ReactActivity/API/Extensions/DataBackupServiceExtensions.cs)

[Backend Data Backup Schedule Service](./service/web/ReactActivity/API/Services/DataBackupService.cs)

* Backup PostgreSQL database every one hour and keep the last 24 hours backup files, Setting in appsettings.json
```
    "Production": {
        "Identity": "ProductionBackupJob",
        "Target": "reactivities",
        "CronSchedule": {
            "WeekdaysCronExpression": "0 0 17-0 * * ?",
            "WeekendsCronExpression": "0 0 8-0 * * ?"
        },
        "Allowed": true
    }
```
## Swagger API Documentation

* Account

![SwaggerAccount](https://imgur.com/AzhbGSI.png)

* Activities

![SwaggerActivities](https://imgur.com/RSphTWH.png)

* Photos

![SwaggerPhotos](https://imgur.com/Vu5LTey.png)

* Profiles

![SwaggerProfiles](https://imgur.com/TMM0tK6.png)

* Follow

![SwaggerFollow](https://imgur.com/9Mvtnuf.png)

* Photos

![SwaggerPhotos](https://imgur.com/TMM0tK6.png)

## Environment Variables Setting
* Frontend with .env.production
```
    REACT_APP_API_URL=http://localhost:5000/api/
    REACT_APP_CHAT_URL=http://localhost:5000/chat
    REACT_APP_GOOGLE_CLIENT_ID=xxxxxxxxxxxxxxxxxxxxxxx
```

* Backend with appsettings.json
```
    ConnectionStrings: {
        DefaultConnection: "Host=example-postgres;Port=5432;Database=example;Username=postgres;Password=postgres",
        RedisConnection: "example-redis:6379,password=redispassword",
    },
    PostgreSQLConfigure: {
        "Schema": "example",
    },
    RedisSettings: {
        "InstanceName": "example",
    },
    StaticFiles: {
        "StoragePath": "/app/production/staticfiles"
    },
    "API": {
        "Google": {
            "BaseUrl": "https://www.googleapis.com/oauth2/v3/",
        }
    },
    "ClientAppSettings": {
        "Origin": "http://localhost:3000",
    },
    "Serilog": {
        "MinimumLevel": {
            "Default": "Information",
            "Override": {
                "Microsoft": "Warning",
                "System": "Warning"
            }
        },
        "WriteTo": [
            {
                "Name": "File",
                "Args": {
                    "path": "/app/production/logs/log.txt",
                    "rollingInterval": "Hour",
                    "retainedFileCountLimit": 720
                }
            }
        ]
    },
    "CorsSettings": {
        "PolicyName": "ExamplePolicy",
    },
    "Origins": {
        "AllowedOrigins": [
            "http://localhost:3000",
        ]
    },
    "Role": {
        "NormalUser": {
            "Password": "ExamplePassword",
        }
    },
    "JWTSettings": {
        "TokenKey": "ExampleKey",
        "Issuer": "ExampleIssuer",
        "Audience": "ExampleAudience",
        "AccessTokenExpiration": 15,
        "RefreshTokenExpiration": 30,
    },
    "SMTPGoogle": {
        "Host": "smtp.gmail.com",
        "Port": 587,
        "Username": "example@gmail.com",
        "Password": "ExamplePassword",
        "Sender": "example@gmail.com"
    },
    "Cloudinary": {
        "CloudName": "example",
        "ApiKey": "xxxxxxxxxxxxxxxxxxxxxxx",
        "ApiSecret": "xxxxxxxxxxxxxxxxxxxxxxx"
    },
    "KestrelSettings": {
        "Endpoints": {
            "Http": {
                "Port": 5000
            },
            "Https": {
                "Port": 5001
            }
        },
        "Certificates": {
            "Default": {
                "Path": "/app/production/certificates/example.pfx",
                "Password": "ExamplePassword"
            }
        }
    },
    "DataBackupSetting": {
        "Production": {
            "Identity": "ProductionBackupJob",
            "Target": "example",
            "CronSchedule": {
                "WeekdaysCronExpression": "0 0 17-0 * * ?",
                "WeekendsCronExpression": "0 0 8-0 * * ?"
            },
            "Allowed": true
        }
    }
```

# Development Environment Setup

## Management Docker containers with Docker Compose
* Start: `docker-compose up -d`
* Stop: `docker-compose down`
* Restart: `docker-compose restart`
* Rebuild: `docker-compose build`

## Build Docker Containers with docker-compose.yml
* docker-compose.yml
```
version: '3.9'
services:
    redis:
        build: ./conf/redis
        image: ${CONTAINER_AUTHOR}/${PROJECT_NAME}/redis:${REDIS_IMAGE_VERSION}
        container_name: ${PROJECT_NAME}_redis
        volumes:
            - ./data/redis:/data
            # - ./conf/redis/setting/redis.conf:/usr/local/etc/redis/redis.conf
        ports:
            - ${REDIS_EXTERNAL_PORT}:${REDIS_INNER_PORT}
        networks:
            react-activity-network:
              ipv4_address: ${REDIS_HOST_IP}
        command: redis-server --appendonly ${REDIS_AOF_ENABLED} --requirepass ${REDIS_PWD}
        restart: on-failure:3

    reverse_proxy:
        build:
            context: ./conf/nginx
            dockerfile: Dockerfile
            args:
                - NGINX_TIME_ZONE=${NGINX_TIME_ZONE}
                - NGINX_LANG_NAME=${NGINX_LANG_NAME}
                - NGINX_LANG_INPUTFILE=${NGINX_LANG_INPUTFILE}
                - NGINX_LANG_CHARMAP=${NGINX_LANG_CHARMAP}
                - DEBIAN_FRONTEND=${NGINX_DEBIAN_FRONTEND}
        image: ${CONTAINER_AUTHOR}/${PROJECT_NAME}/nginx:${NGINX_IMAGE_VERSION}
        container_name: ${PROJECT_NAME}_nginx
        env_file:
          - ./.env
        environment:
          - LANG=${NGINX_LANG_NAME}
        volumes:
            - ./web/production/frontend/dist:/usr/share/nginx/dist
            - ./web/production/certs/ssl/nginx:/etc/nginx/ssl
            - ./web/production/certs/data/.well-known/pki-validation:/usr/share/nginx/html/letsencrypt/
            - ./web/test/html:/usr/share/nginx/html
            - ./conf/nginx/nginx.conf:/etc/nginx/nginx.conf
            - ./conf/nginx/conf.d:/etc/nginx/conf.d
            - ./logs/nginx:/var/log/nginx
        ports:
            - ${NGINX_HTTP_EXTERNAL_PORT}:${NGINX_HTTP_INNER_PORT}
            - ${NGINX_HTTPS_EXTERNAL_PORT}:${NGINX_HTTPS_INNER_PORT}
        depends_on:
            - db
            - backend
        networks:
          react-activity-network:
              ipv4_address: ${NGINX_HOST_IP}
        user: root
        tty: true
        restart: on-failure:3

    frontend:
        build:
            context: ./conf/nodejs
            dockerfile: Dockerfile
            args:
                - NODEJS_ENV=${NODEJS_ENV}
        image: ${CONTAINER_AUTHOR}/${PROJECT_NAME}/nodejs:${NODEJS_IMAGE_VERSION}
        container_name: ${PROJECT_NAME}_frontend
        volumes:
            - ./web:/app
        ports:
            - ${NODEJS_EXTERNAL_PORT}:${NODEJS_INNER_PORT}
        depends_on:
            - db
        networks:
            react-activity-network:
                ipv4_address: ${NODEJS_HOST_IP}
        tty: true
        restart: on-failure:3

    backend:
        build:
            context: ./conf/dotnet/ubuntu
            dockerfile: Dockerfile
            args:
                - DOTNET_INSTALLED_VERSION=${DOTNET_INSTALLED_VERSION}
                - DOTNET_TIME_ZONE=${DOTNET_TIME_ZONE}
                - DOTNET_LANG_NAME=${DOTNET_LANG_NAME}
                - DOTNET_LANG_INPUTFILE=${DOTNET_LANG_INPUTFILE}
                - DOTNET_LANG_CHARMAP=${DOTNET_LANG_CHARMAP}
                - DEBIAN_FRONTEND=${DOTNET_DEBIAN_FRONTEND}
                - DOTNET_POSTGRESQL_CLIENT_HOME=${DOTNET_POSTGRESQL_CLIENT_HOME}
                - DOTNET_POSTGRESQL_CLIENT_VERSION=${DOTNET_POSTGRESQL_CLIENT_VERSION}
                - POSTGRES_DATA_BACKUP_PATH=:${POSTGRES_DATA_BACKUP_PATH}
                - DOTNET_PACKAGES_PATH=${DOTNET_PACKAGES_PATH}
        image: ${CONTAINER_AUTHOR}/${PROJECT_NAME}/dotnet:${DOTNET_IMAGE_VERSION}
        container_name: ${PROJECT_NAME}_backend
        env_file:
            - ./.env
        environment:
            - DOTNET_LANG_NAME=${DOTNET_LANG_NAME}
            - PGPASSWORD=${POSTGRES_PASSWORD}
            - DOTNET_POSTGRES_USER=${POSTGRES_USER}
            - DOTNET_POSTGRES_HOST_IP=${POSTGRES_HOST_IP}
            - DOTNET_POSTGRES_PORT=${POSTGRES_INNER_PORT}
        volumes:
            - ./web:/app
            - ./data/postgresql/pgdata_backup:${POSTGRES_DATA_BACKUP_PATH}
            - ./logs/backend:/var/log/backend
        ports:
            - ${DOTNET_EXTERNAL_HTTP_PORT}:${DOTNET_INNER_HTTP_PORT}
            - ${DOTNET_EXTERNAL_HTTPS_PORT}:${DOTNET_INNER_HTTPS_PORT}
            - ${DOTNET_TEST_EXTERNAL_HTTP_PORT}:${DOTNET_TEST_INNER_HTTP_PORT}
        depends_on:
          - db
        networks:
            react-activity-network:
                ipv4_address: ${DOTNET_HOST_IP}
        tty: true
        restart: on-failure:3

    db:
        build:
            context: ./conf/postgresql
            dockerfile: Dockerfile
            args:
                - POSTGRES_TIME_ZONE=${POSTGRES_TIME_ZONE}
                - POSTGRES_LANG_NAME=${POSTGRES_LANG_NAME}
                - POSTGRES_LANG_INPUTFILE=${POSTGRES_LANG_INPUTFILE}
                - POSTGRES_LANG_CHARMAP=${POSTGRES_LANG_CHARMAP}
        image: ${CONTAINER_AUTHOR}/${PROJECT_NAME}/postgresql:${POSTGRES_IMAGE_VERSION}
        container_name: ${PROJECT_NAME}_postgresql
        env_file:
          - ./.env
        environment:
            - DATABASE_HOST=${DATABASE_HOST}
            - POSTGRES_USER=${POSTGRES_USER}
            - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
            - POSTGRES_DB=${POSTGRES_DB}
            - PGDATA=/var/lib/postgresql/data
            - TZ=${POSTGRES_TIME_ZONE}
            - POSTGRES_LANG_NAME=${POSTGRES_LANG_NAME}
        volumes:
            - ./data/postgresql/pgdata:/var/lib/postgresql/data
            - ./data/postgresql/pgdata_backup:${POSTGRES_DATA_BACKUP_PATH}
        ports:
            - ${POSTGRES_EXTERNAL_PORT}:${POSTGRES_INNER_PORT}
        networks:
            react-activity-network:
                ipv4_address: ${POSTGRES_HOST_IP}
        restart: on-failure:3

    db_adminer:
        build: ./conf/postgresql_admin
        image: ${CONTAINER_AUTHOR}/${PROJECT_NAME}/pgadmin:${PGADMIN_IMAGE_VERSION}
        container_name: ${PROJECT_NAME}_pgadmin
        env_file:
          - ./.env
        environment:
            - PGADMIN_DEFAULT_EMAIL=${PGADMIN_DEFAULT_EMAIL:-test@test.com}
            - PGADMIN_DEFAULT_PASSWORD=${PGADMIN_DEFAULT_PASSWORD:-test123!}
        volumes:
            - db_adminer:/var/lib/pgadmin
            - ./data/pgadmin/pgadmin_data:/var/lib/pgadmin
        ports:
            - ${PGADMIN_EXTERNAL_PORT}:${PGADMIN_INNER_PORT}
        depends_on:
            - db
        networks:
          react-activity-network:
              ipv4_address: ${PGADMIN_HOST_IP}
        user: root
        restart: on-failure:2



networks:
    react-activity-network:
        ipam:
          config:
            - subnet: ${NETWORK_SUBNET}
              gateway: ${NETWORK_GATEWAY}

volumes:
    db_adminer:
```

# Website runtime environment
* Ubuntu 20.04
* Nginx 1.23.3
* Node.js 20.1-alpine3.16
* .NET 7.0 SDK and Runtime with ASP.NET Core 7.0
* PostgreSQL 15.2
* PgAdmin 7.1
* Redis 7.0.9