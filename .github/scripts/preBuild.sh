#!/bin/bash

# This script is run before the build starts. 
# It prepares the repository and creates release tags automatically.

git fetch --all

# This function gets the next minor version number using alreayd existing tags.
getNextMinorVersion()
{
  maxMajorVersion=1
  maxMinorVersion=-1
  allTags=`git tag`

  # Loop through all tags and find the highest major and minor version numbers.
  while read -r tag; do

    if [[ $tag =~ ^([0-9]).([0-9]).[0-9]*$ ]] ; then

      majorVersion="${BASH_REMATCH[1]}"
      minorVersion="${BASH_REMATCH[2]}"

      if [[ $majorVersion -gt $maxMajorVersion ]]; then
        maxMajorVersion=$majorVersion
        minorVersion=1
      fi

      if [[ $majorVersion -eq $maxMajorVersion && $minorVersion -gt $maxMinorVersion ]]; then
        maxMinorVersion=$minorVersion
      fi

    fi

  done <<< "$allTags"

  # Increment the minor version number.
  let maxMinorVersion++

  # Return detected version numbers.
  echo $maxMajorVersion.$maxMinorVersion.0
}

# Get next fix version from branch name and existing tags.
getNextFixVersion()
{
    majorMinorVersion=${$GITHUB_REF//'refs/heads/main-fix/'/}
    maxFixVersion=-1
    
    allTags=`git tag`
        
    while read -r tag; do
        
        if [[ $tag =~ ^$majorMinorVersion\.([0-9]*)$ ]] ;
        then
        
            fixVersion="${BASH_REMATCH[1]}"
            
            if [[ $fixVersion -gt $maxFixVersion ]]
            then
                maxFixVersion=$fixVersion
            fi
                    
        fi
    
    done <<< "$allTags"
    
    let "maxFixVersion++"

    echo $majorMinorVersion.$maxFixVersion

}


# Setup version informations according to the branch name.
if [[ $GITHUB_REF == *heads/main ]]; then

  tag=$(getNextMinorVersion)

  echo main branch
  echo tag: $tag

  echo "VERSION_TAG=$tag" >> $GITHUB_ENV

elif [[ $GITHUB_REF == *heads/main-fix/ ]]; then

  tag=$(getNextFixVersion)

  echo main-fix branch
  echo tag: $tag

  echo "VERSION_TAG=$tag" >> $GITHUB_ENV

else

  echo "No version tag created."
  tag=$(getNextMinorVersion)
  echo tag: $tag

fi