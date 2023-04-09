#!/bin/bash

echo "Starting drop database."

solution="ReactActivity"

if [ -d "$solution" ]; then
    echo "Directory $solution exists."
else
    echo "Directory $solution does not exist."
    exit 1
fi

cd $solution

dotnet ef database drop -p Persistence -c DataContext -s API --verbose

if [ $? -ne 0 ]; then
    echo "Error dropping database"
    exit 1
fi

cd ..

echo "Finish dropping database."