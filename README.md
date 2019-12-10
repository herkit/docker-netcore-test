# docker-netcore-test
This repo demonstrates creating, starting and retrieving files from a docker container using dotnetcore and the Docker.DotNet Nuget package.

## Getting started
First build the Docker image in /cowsaid:

```
cd cowsaid
docker build -t cowsaid .
```

This image is based on the grycap/cowsay image, the only difference is that it will write it's output to the container filesystem layer 
instead of writing to TTY.

Next step is to run the solution, and voila! The cowsaid.txt file created will be written to your temp folder.
