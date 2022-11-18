This folder contain docker images for development purpose

# How to prepare dev database
1. Install docker for your OS (Windows 10, Linux, Mac)
2. Install docker-compose tool (in docker desktop is installed by default)
3. Extract dbc and place it in 'dbc' folder
4. Build docker images from command line: `docker-compose build`
5. Start database: `docker-compose up -d mysql`
6. Clone and import database from https://github.com/mangosvb/DatabaseZero
7. Apply migrations from `./sql` folder

# How to start server
1) `docker-compose up -d realm`
2) Wait 5 seconds
3) `docker-compose up -d game`

# How to stop server
    docker-compose down
