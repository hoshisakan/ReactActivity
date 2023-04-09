#!/bin/bash

if [ -z "$1" ] || [ -z "$2" ]; then
    echo "No argument supplied for class library name and add target reference name."
    echo "Usage: ./addClassLibrary.sh <class_library_name> <add_target_reference_name>"
    exit 1
fi

solution="ReactActivity"

cd $solution

echo "Starting create class library $1."

dotnet new classlib -n $1 -f net7.0

if [ $? -ne 0 ]; then
    echo "Error creating class library $1"
    exit 1
fi

echo "Finish create class library $1."

echo "Adding class library $1 to solution."

dotnet sln add $1/$1.csproj

if [ $? -ne 0 ]; then
    echo "Error adding class library $1 to solution"
    exit 1
fi

echo "Finish adding class library $1 to solution."

echo "Adding project reference to class library $1."

cd $1

dotnet add reference ../$2/$2.csproj

if [ $? -ne 0 ]; then
    echo "Error adding project reference to class library $1"
    exit 1
fi

cd ..

echo "Finish adding project reference to class library $1."