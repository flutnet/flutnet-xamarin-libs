#!/usr/bin/env bash

# Define paths
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
SRC_DIR=$SCRIPT_DIR/../src
VSTOOL_PATH="/Applications/Visual Studio.app/Contents/MacOS/vstool"
SOLUTION_PATH=$SCRIPT_DIR/../Flutnet.sln

# Clean and build
"$VSTOOL_PATH" build --configuration:Debug --target:Clean "$SOLUTION_PATH"
"$VSTOOL_PATH" build --configuration:Release --target:Clean "$SOLUTION_PATH"

"$VSTOOL_PATH" build --configuration:Debug "$SOLUTION_PATH"
"$VSTOOL_PATH" build --configuration:Release "$SOLUTION_PATH"