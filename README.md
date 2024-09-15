# Introduction

This project is a Slot Machine API built with .NET Core and MongoDB, designed to simulate a slot machine game with configurable parameters. The application is containerized using Docker, making it easy to set up and run without installing dependencies locally.

## Prerequisites

Before you begin, ensure you have met the following requirements:

- **Git**: Download and install [Git](https://git-scm.com/downloads)
- **Docker**: Download and install [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Getting Started

### Clone the Repository

Clone the project repository to your local machine using Git:

```bash
git clone https://github.com/eyalLefler/SlotMachineAPI.git
```

### Environment Setup

No additional environment setup is required as the application runs inside Docker containers.

### Build and Run the Application

Use Docker Compose to build and run the application along with MongoDB:

```bash
docker-compose up --build
```

This command will:

- Build the Docker image for the Slot Machine API
- Pull the MongoDB Docker image
- Run both containers, linking them together as defined in the `docker-compose.yml` file

Use Docker Compose to stop and clean everything including the Docker volumes and with them the MongoDB itself.

```bash
docker-compose down -v
```


### Access the Application

Once the application is running, it will be accessible at:

- **API Base URL**: [http://localhost:5000](http://localhost:5000)
- **Swagger UI**: [http://localhost:5000/swagger](http://localhost:5000/swagger)
