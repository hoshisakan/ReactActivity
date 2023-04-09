#!/bin/bash

solution="ReactActivity"

if [ -d "$solution" ]; then
    echo "Directory $solution exists."
else
    echo "Directory $solution does not exist."
    exit 1
fi

cd $solution

echo "Starting remove migrations."

#TODO: The command will remove last migration.
dotnet ef migrations remove -p Persistence -s API -c DataContext --verbose

if [ $? -eq 0 ]; then
    echo "Finish removing migrations."
else
    echo "Error removing migrations."
    exit 1
fi