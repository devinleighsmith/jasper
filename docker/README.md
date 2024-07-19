# Running the Application on DevContainer for Local Development

## Pre-requisite

Ensure `Docker` and `Dev Containers` extenion (`ms-vscode-remote.remote-containers`) is installed in your machine.

## Steps

1. Launch code in VSCode.
2. Hit `Ctrl + Shift + P`, select `Dev Containers: Open Folder in Container...` and wait for it to completely load.
3. [Start](#starting-the-project) the project.

## Enabling Hot Reloading and Debugging

Hot reloading is a feature that lets you see the effects of your code changes almost instantly without having to completely restart the application. This significantly speeds up the development process.

### Frontend

1. Go to `./manage` file.
2. Find the `DEFAULT_CONTAINERS` and replace `web` with `web-dev`.

```
DEFAULT_CONTAINERS="db api web-dev"
```

3. Find the `build-all` function and replace with the code below.

```
build-all() {
 build-web-dev
 build-api
}
```

4. [Rebuild](#building-the-images) the images.
5. [Start](#starting-the-project) the project.

### Backend

- The backend is automatically utilizing hot reloading.
- Add the code below to the `launch.json` to enable remote debugging.

```
{
  "configurations": [
    {
      "name": ".NET Core Docker Attach",
      "type": "coreclr",
      "request": "attach",
      "processId": "${command:pickRemoteProcess}",
      "pipeTransport": {
        "pipeProgram": "docker",
        "pipeArgs": ["exec", "-i", "scv-api-1"],
        "debuggerPath": "/vsdbg/vsdbg",
        "pipeCwd": "${workspaceRoot}",
        "quoteArgs": false
      },
      "sourceFileMap": {
        "/opt/app-root/src": "${workspaceRoot}/"
      }
    }
  ]
}
```

## Notes

- When the error below occurs, go to `manage` file and change the _Select End Line of Sequence_ (lower right hand side of VSCode) from **CRLF** to **LF**.

```
bash: ./manage: /bin/bash^M: bad interpreter: No such file or directory
```

# Running the Application on Docker

## Management Script

The `manage` script wraps the Docker process in easy to use commands.

To get full usage information on the script, run:

```
./manage -h
```

## Building the Images

The first thing you'll need to do is build the Docker images.

To build the images, run:

```
./manage build
```

## Starting the Project

To start the project, run:

```
./manage start
```

This will start the project interactively; with all of the logs being written to the command line. Press `Ctrl-C` to shut down the services from the same shell window.

Any environment variables containing settings, configuration, or secrets can be placed in a `.env` file in the `docker` folder and they will automatically be picked up and loaded by the `./manage` script when you start the application.

## Stopping the Project

To stop the project, run:

```
./manage stop
```

This will shut down and clean up all of the containers in the project. This is a non-destructive process. The containers are not deleted so they will be reused the next time you run start.

Since the services are started interactively, you will have to issue this command from another shell window. This command can also be run after shutting down the services using the `Ctrl-C` method to clean up any services that may not have shutdown correctly.

## Using the Application

- By default, the main developer UI is exposed at; https://localhost:8080/scjscv/
- The Swagger API and documentation is available at; https://localhost:8080/scjscv/api/
- Which is also exposed directly at; http://localhost:5000/api/
