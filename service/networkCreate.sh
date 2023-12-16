#!/bin/bash
# Create the network, but if dcoker-compose.yml file has been created, this script will not be used.
# Such as: networks:
# react-activity-network:
#     ipam:
#       config:
#         - subnet: ${NETWORK_SUBNET}
#           gateway: ${NETWORK_GATEWAY}

echo "Creating the network."

docker network create react-activity-network --driver=bridge --subnet=172.28.1.0/24 --gateway=172.28.1.1

if [ $? -eq 0 ]; then
    echo "Create the network successfully."
else
    echo "Create the network failed."
    exit 1
fi