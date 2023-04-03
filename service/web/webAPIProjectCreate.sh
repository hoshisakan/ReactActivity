#!/bin/bash

solution="ReactActivity"

echo "Creating directory $solution"

mkdir $solution

if [ $? -ne 0 ]; then
    echo "Error creating directory $solution"
    exit 1
fi

echo "Finish creating directory $solution"

cd $solution

echo "Starting create solution and projects."

dotnet new sln
dotnet new webapi -n API -f net7.0
dotnet new classlib -n Application -f net7.0
dotnet new classlib -n Domain -f net7.0
dotnet new classlib -n Persistence -f net7.0

echo "Finish create solution and projects."

echo "Adding projects to the solution."

dotnet sln add API/API.csproj
dotnet sln add Application/Application.csproj
dotnet sln add Domain/Domain.csproj
dotnet sln add Persistence/Persistence.csproj

echo "Finish adding projects to the solution."

echo "Adding project references."

cd API
dotnet add reference ../Application/Application.csproj

cd ../Application
dotnet add reference ../Domain/Domain.csproj
dotnet add reference ../Persistence/Persistence.csproj

cd ../Persistence
dotnet add reference ../Domain/Domain.csproj

cd ..

echo "Finish adding project references."

echo "Executing dotnet restore."

dotnet restore

echo "Finish executing dotnet restore."