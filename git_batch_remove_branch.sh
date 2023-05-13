#!/bin/bash
git branch -d develop
git push origin --delete develop

# git branch -d hotfix
# git push origin --delete hotfix

# git branch -d introduce/readme
# git push origin --delete introduce/readme

# git branch -d deploy/local-test
# git push origin --delete deploy/local-test

# git branch -d deploy/azure
# git push origin --delete deploy/azure

git branch -d feature-activities
git push origin --delete feature-activities

git branch -d feature-backend-API-image-uplaod
git push origin --delete feature-backend-API-image-uplaod

git branch -d feature-backend-identity
git push origin --delete feature-backend-identity

git branch -d feature-error-handing
git push origin --delete feature-error-handing

git branch -d feature-frontend-UI-design
git push origin --delete feature-frontend-UI-design

git branch -d feature-frontend-client-image-upload
git push origin --delete feature-frontend-client-image-upload

git branch -d feature-frontend-client-side-attendance
git push origin --delete feature-frontend-client-side-attendance

git branch -d feature-frontend-user-identity
git push origin --delete feature-frontend-user-identity

git branch -d feature-migration
git push origin --delete feature-migration

git branch -d feature-route
git push origin --delete feature-route