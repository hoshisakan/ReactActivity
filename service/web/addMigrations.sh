#!/bin/bash

if [ -z "$1" ]; then
    echo "No argument supplied fpr migration number"
    echo "Usage: ./addMigrations.sh <migration_name_number> -d for allow create database"
    echo "Usage: ./addMigrations.sh <migration_name_number> -n for not allow create database"
    exit 1
fi

if [ -z "$2" ]; then
    echo "No argument supplied for allow create database"
    echo "Usage: ./addMigrations.sh <migration_name_number> -d for allow create database"
    echo "Usage: ./addMigrations.sh <migration_name_number> -n for not allow create database"
    exit 1
fi

solution="ReactActivity"

if [ -d "$solution" ]; then
    echo "Directory $solution exists."
else
    echo "Directory $solution does not exist."
    exit 1
fi

cd $solution

echo "Starting add migrations."

if [ "$1" = "1" ]; then
    echo "Adding migration 1 'InitialCreate' For 'Persistence' Add 'Activities' Table."
    dotnet ef migrations add InitialCreate -s API -p Persistence -c DataContext --verbose
elif [ "$1" = "2" ]; then
    echo "Adding migration 2."
    dotnet ef migrations add IdentityAdded -s API -p Persistence -c DataContext --verbose
elif [ "$1" = "3" ]; then
    echo "Adding migration 3."
    dotnet ef migrations add IdentityChangeBioFieldToNullable -s API -p Persistence -c DataContext --verbose
elif [ "$1" = "4" ]; then
    echo "Adding migration 4."
    dotnet ef migrations add ActivityAttendeeAdded -s API -p Persistence -c DataContext --verbose
elif [ "$1" = "5" ]; then
    echo "Adding migration 5."
    dotnet ef migrations add AddCancelledProperty -s API -p Persistence -c DataContext --verbose
elif [ "$1" = "6" ]; then
    echo "Adding migration 6."
elif [ "$1" = "7" ]; then
    echo "Adding migration 7."
elif [ "$1" = "8" ]; then
    echo "Adding migration 8."
elif [ "$1" = "9" ]; then
    echo "Adding migration 9."
elif [ "$1" = "10" ]; then
    echo "Adding migration 10."
elif [ "$1" = "all" ]; then
    echo "Adding all migrations."
else
    echo "Migration $1 does not exist."
    exit 1
fi

if [ "$2" = "-d" ]; then
    echo "Allow create database."
    dotnet ef database update -s API -p Persistence -c DataContext
elif [ "$2" = "-n" ]; then
    echo "Not allow create database."
else
    echo "Argument $2 does not exist."
    exit 1
fi

echo "Finish add migrations."