
You can use MangosSharp in docker

# Setup
1. Install docker for your OS (Windows 10, Linux, Mac)
2. Install docker-compose tool (in docker desktop is installed by default)
3. Extract dbc and place it in 'dbc' folder
4. Run in this folder: docker-compose up -d mysql
5. Configure database and apply all db migrations
6. Run in this folder: docker-compose up -d
7. Run World Of Warcraft and check connection

# How to start server
1) docker-compose up -d mysql
2) Wait 5 seconds
3) docker-compose up -d

# How to stop server
1) docker-compose down