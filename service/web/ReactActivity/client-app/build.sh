#!/bin/sh

SolutionPath="/app/ReactActivity/client-app/build"

if [ -d $SolutionPath ]; then
    echo "remove has been build directory. . ."
    rm -r $SolutionPath
fi

echo "Starting build client-app."

npm run build

if [ $? -eq 0 ]; then
    echo "Build client-app successfully."
else
    echo "Build client-app failed."
    exit 1
fi

echo "Finish build client-app."

OutputPath="/app/production/frontend/dist"

if [ -d $OutputPath ]; then
    echo "Starting remove has been build dist directory. . ."
    rm -rf $OutputPath
    echo "Finish remove has been build dist directory. . ."
fi

if [ -d $OutputPath ]; then
    echo "Remove has been build dist directory failed."
    exit 1
fi

echo "Starting build dist directory. . ."
mkdir -p $OutputPath
echo "Finish build dist directory. . ."

if [ -d $OutputPath ]; then
    echo "Build dist directory successfully."
else
    echo "Build dist directory failed."
    exit 1
fi

echo "Starting copy build files to $OutputPath."

cp -r build/* $OutputPath

if [ $? -eq 0 ]; then
    echo "Copy build files to $OutputPath successfully."
else
    echo "Copy build files to $OutputPath failed."
    exit 1
fi

echo "Finish copy build files to $OutputPath."

echo "Starting remove old build files."

rm -rf $SolutionPath

if [ $? -eq 0 ]; then
    echo "Remove old build files successfully."
else
    echo "Remove old build files failed."
    exit 1
fi

echo "Finish remove old build files."
