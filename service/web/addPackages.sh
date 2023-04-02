#!/bin/bash

if [ -z "$1" ]; then
    echo "No argument supplied"
    echo "Usage: ./addPackages.sh <project> or <classlib_name>"
    exit 1
fi

solution="ReactActivity"

echo "Starting add packages to projects and classlib."

cd $solution

if [ -d "$1" ]; then
    echo "Directory $1 exists."
    cd $1
else
    echo "Directory $1 does not exist."
    exit 1
fi

if [ "$1" = "API" ]; then
    dotnet add package Microsoft.EntityFrameworkCore.Design --version 7.0.4
    dotnet add package Serilog.AspNetCore --version 6.0.1
elif [ "$1" = "Persistence" ]; then
    dotnet add package Microsoft.EntityFrameworkCore --version 7.0.4
    dotnet add package Microsoft.EntityFrameworkCore.Design --version 7.0.4
    dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 7.0.3
elif [ "$1" = "Domain" ]; then
    echo "Nothing to do."
elif [ "$1" = "Application" ]; then
    dotnet add package Microsoft.EntityFrameworkCore.Relational --version 7.0.4
    dotnet add package MediatR.Extensions.Microsoft.DependencyInjection --version 11.1.0
    dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.0
else
    echo "Project or classlib $1 does not exist."
    exit 1
fi

dotnet list package
cd ..

echo "Finish packages to projects and classlib."
