#!/bin/bash

# This script is run after the build has run.
# It will create a release tag and push it to the repository.

git config user.name "${GITHUB_ACTOR}"
git config user.email "${GITHUB_ACTOR}@users.noreply.github.com"

if [[ $GITHUB_REF == *heads/main ]]; then

  echo tag: $VERSION_TAG

  git tag $VERSION_TAG
  git push origin $VERSION_TAG

  # Create master-fix branch
  masterFixBranchName=main-fix/`echo $VERSION_TAG | sed -e 's/\.[0-9]*$//'`

  echo create branch ${masterFixBranchName}

  git branch ${masterFixBranchName}
  git push --set-upstream origin ${masterFixBranchName}

elif [[ $GITHUB_REF == *heads/main-fix/* ]]; then

  echo tag: $VERSION_TAG

  git tag $VERSION_TAG
  git push origin $VERSION_TAG

fi