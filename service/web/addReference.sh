#!/bin/bash

if [ -z "$1" ] || [ -z "$2" ]; then
    echo "No argument supplied for target name and class library name."
    echo "Usage: ./addReference.sh <target_name> <class_library_name>"
    exit 1
fi

solution="ReactActivity"

cd $solution

echo "Starting change directory to $1."

cd $1

if [ $? -ne 0 ]; then
    echo "Error changing directory to $1"
    exit 1
fi

echo "Finish changing directory to $1."

echo "Starting add reference to class library $2."

dotnet add reference ../$2/$2.csproj

if [ $? -ne 0 ]; then
    echo "Error adding reference to class library $2"
    exit 1
fi

cd ..

echo "Finish adding reference to class library $2."