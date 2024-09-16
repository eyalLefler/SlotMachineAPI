# Introduction

This project is a Slot Machine API built with .NET Core and MongoDB, designed to simulate a slot machine game with configurable parameters. The application is containerized using Docker, making it easy to set up and run without installing dependencies locally.

The API provides functionality for players to place bets and simulate spins on a slot machine. The key features include:

- Spin Method: Allows a player to place a bet, and the slot machine returns the result matrix along with the win amount and updated player balance.
- Update Balance Method: Enables adding a specified amount to a player's balance in MongoDB


## Key Concepts and Implementation Details
1. Atomic Updates:
	- The API ensures that balance updates are performed atomically to prevent race conditions, especially important when multiple concurrent requests are made.
    - This is achieved by using MongoDB's FindOneAndUpdateAsync method, which combines the balance check and update in a single atomic operation.  
2. Error Handling:
    - The API handles scenarios where a player's balance is insufficient for the bet by returning appropriate error messages.
    - It also includes checks for player existence and invalid configurations, ensuring robustness.
  
3. Unit and Integration Testing:
    - Comprehensive unit tests are included to verify the logic of the spin and balance update methods.
    - Integration tests simulate realistic scenarios, including concurrent requests to validate the atomicity and consistency of operations.


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

Once the container is running, it will be accessible at:


- **API Base URL**: [http://localhost:5000](http://localhost:5000)
- **Swagger UI**: [http://localhost:5000/swagger](http://localhost:5000/swagger)
- You can also start a Debug sesstion using visual studio - This is the only way to debug, make sure the container is up and runing.


## Areas for Improvement    
1. Enhanced Error Handling:
   - Implement more granular error handling and use custom exception types to provide clearer error messages.
   - Improve logging for debugging and monitoring purposes, especially for identifying failed transactions or invalid operations.
2. Performance Optimization:
   - Currently, the project uses MongoDB for all operations, which works well for small-scale deployments. However, for high-throughput scenarios, consider implementing caching mechanisms (e.g., using Redis) to reduce the load on MongoDB.
   - Batch processing can be introduced for handling multiple spins in one request to improve performance.
3. Scalability:
   - Introduce a sharded MongoDB cluster to handle larger datasets and higher throughput.
   - Implement rate limiting to prevent abuse and ensure fair use of the API.
4. Advanced Game Logic:
   - Expand the game logic to include more complex slot machine features, such as different types of paylines, bonus rounds, and jackpots.
   - Add user-defined configurations, allowing players to select the number of lines they want to bet on or the size of the slot matrix.
5. Security Enhancements:
   - Implement authentication and authorization to secure the API, ensuring that only authorized users can access and modify player balances.
   - Use HTTPS and secure database connections to protect sensitive data during transmission.
6. Monitoring and Analytics:
   - Integrate monitoring tools (e.g., Prometheus, Grafana) to track API performance and player activities.
   - Collect analytics to understand player behavior and optimize game mechanics.

## Conclusion
The current implementation provides a solid foundation for a slot machine API, focusing on atomic operations and concurrency handling. There are several avenues for enhancement, particularly in terms of performance, scalability, and feature richness. By iterating on these areas, the API can be further developed into a robust and engaging gaming platform.