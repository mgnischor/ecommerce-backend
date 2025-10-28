@echo off

echo ====================================
echo  Docker Cleanup Script
echo ====================================
echo.

echo Stopping containers...
docker stop ecommerce-backend-dev ecommerce-backend-prod ecommerce-postgres 2>nul

echo Removing containers...
docker rm -f ecommerce-backend-dev ecommerce-backend-prod ecommerce-postgres 2>nul

echo Removing network...
docker network rm ecommerce-network 2>nul

echo.
echo Cleanup completed!
echo.
