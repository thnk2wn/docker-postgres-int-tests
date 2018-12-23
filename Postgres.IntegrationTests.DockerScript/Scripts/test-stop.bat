@echo off

echo Killing postgres container
docker kill postgres-server
docker rm postgres-server