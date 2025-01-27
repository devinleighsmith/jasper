[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)
![Lifecycle:Stable](https://img.shields.io/badge/Lifecycle-Stable-97ca00)

# JASPER

This repository contains the code for the **JASPER** application.

## Running in Docker

Refer to [Running the Application on Docker](./docker/README.md) to details.

## High Level Architecture

The architecture diagrams for JASPER can be found here; [JASPER - Architecture Diagrams](https://jag.gov.bc.ca/wiki/display/JASPER/Architecture+Diagrams)

### API Documentation

For high level View API documentation refer to the diagram above. For details, refer to the [router](./web/src/router/index.ts) and [view components](./web/src/components/) source code.

For backend API documentation refer to the Swagger API documentation page available at the `api/` endpoint of the running application. For example, if you are running the application locally in docker, the Swagger page can be found at https://localhost:8080/jasper/api/. Refer to [Running in Docker](#running-in-docker) section for details.

### Sharing Code with the Court Viewer

Refer to [Sharing Code Changes Between JASPER and the Court Viewer](./doc/sharing-changes-with-cv.md) for details on making the decision on when and how to share code between JASPER and the Court Viewer.

## Getting Help or Reporting an Issue

To report bugs/issues/feature requests, please file an [issue](../../issues).

## How to Contribute

If you would like to contribute, please see our [CONTRIBUTING](./CONTRIBUTING.md) guidelines.

Please note that this project is released with a [Contributor Code of Conduct](./CODE_OF_CONDUCT.md).
By participating in this project you agree to abide by its terms.
