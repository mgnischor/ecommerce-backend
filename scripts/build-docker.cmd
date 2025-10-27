@echo off

cd ..

docker rm -f ecommerce-backend-dev
docker rm -f ecommerce-backend-prod

docker build --build-arg BUILD_DEVELOPMENT=1 -t ecommerce-backend:dev .
docker build --build-arg BUILD_DEVELOPMENT=0 -t ecommerce-backend:prod .

docker run -d -p 5049:5049 --name ecommerce-backend-dev ecommerce-backend:dev
docker run -d -p 80:80 --name ecommerce-backend-prod ecommerce-backend:prod
